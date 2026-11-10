using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using EnemyBalance;
using System.Linq;
/// <summary>フェーズ設定に従って敵の生成数と活動期間を管理する</summary>

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] private List<PhaseData> phases;

    private int currentPhaseIndex;
    private PhaseData currentPhase;
    private readonly PhaseDpsTracker phaseDps = new PhaseDpsTracker();
    public float CurrentHpMultiplier { get; private set; } = 1f;
    public double PreviousNormalDps => cycleRuntime != null ? cycleRuntime.NormalDps : phaseDps.PreviousNormalDps;

    [SerializeField] private float spawnRadius = 15f;

    // 敵のスポーン方向のデータ
    [SerializeField] private SpawnDirectionData[] spawnDirections;
    [SerializeField] private Timer timer;
    [SerializeField] private int maxSpawnTry = 5;
    [Tooltip("箱型Prefabの底面を地面に合わせ、本体全体が壁と重ならない位置に生成・再配置する")]
    [SerializeField] private bool groundedSpawns;
    [SerializeField]private List<GameObject> activeEnemies = new List<GameObject>();
    public IReadOnlyList<GameObject> ActiveEnemies => activeEnemies;

    //敵生成上限数
    /// <summary>ゲーム開始時に設定を反映して更新を開始する</summary>
    [SerializeField] private int maxEnemyCount = 10;

    [Tooltip("プレイヤーからこの距離より離れた敵を、生成と同じ位置の決め方で移し直す。spawnRadius より大きく、ステージのタイルが入れ替わる距離より小さくする")]
    [SerializeField, Min(0f)] private float relocateDistance = 85f;

    [Tooltip("プレイヤーよりこの高さ以上低くなった敵（足場から落ちた敵）を移し直す")]
    [SerializeField, Min(0f)] private float fallRelocateDepth = 30f;

    public float RelocateDistance => relocateDistance;
    public float FallRelocateDepth => fallRelocateDepth;

    private Coroutine spawnRoutine;
    private bool autoSpawnDisabled;
    // 新敵Phaseの進行は専用クラスへ委譲する。旧Phase（アイテムを含む）ではnullのまま。
    private EnemyCycleRuntime cycleRuntime;
    // 固定時間で次Phaseへ進んでも、生存中の中ボスを雑魚の入れ替えから保護する。
    private readonly Dictionary<GameObject, (Enemy enemy, Health health)> cycleBosses = new();

    private void Start()
    {
        if (phases.Any(phase => phase is EnemyCyclePhase))
        {
            // 新Phaseを同じ欄で併用するための接続。同じStartTimeではInspectorの並び順を保つ。
            foreach (EnemyCyclePhase cycle in phases.OfType<EnemyCyclePhase>())
                if (!cycle.Validate(out string error))
                {
                    Debug.LogError($"{name}: {cycle.name}: {error}", this);
                    enabled = false;
                    return;
                }
            phases = phases.OrderBy(phase => phase.StartTime).ToList();
        }
        else phases.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

        currentPhaseIndex = 0;
        currentPhase = phases[0];
        phaseDps.BeginPhase(currentPhase.IsExPhase, currentPhase.StartTime);
        UpdateHpMultiplier();
        BeginEnemyCycle(timer.ElapsedTime);

        if (!autoSpawnDisabled)
            spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    public void DisableAutoSpawn()
    {
        autoSpawnDisabled = true;

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }
    /// <summary>保持済みの参照を使って個体の状態を更新する</summary>

    public void Update()
    {
        AdvancePhase(timer.ElapsedTime);
    }

    private void AdvancePhase(float elapsedTime)
    {
        // 新Phaseは通常＋インターバル＋大群の完了を待つ。旧Phaseの絶対時刻による切替はそのまま。
        if (cycleRuntime != null)
        {
            cycleRuntime.Tick(elapsedTime, !autoSpawnDisabled);
            if (!cycleRuntime.IsComplete) return;
            if (timer.IsUnlimited && currentPhaseIndex == phases.Count - 1)
            {
                // 最終Phaseはエンドレス時だけ繰り返す。遅延しても固定の区間境界を維持する。
                var cycle = (EnemyCyclePhase)currentPhase;
                double startsAt = cycleRuntime.EndsAt;
                startsAt += System.Math.Floor((elapsedTime - startsAt) / cycle.TotalDuration) * cycle.TotalDuration;
                cycleRuntime.Dispose();
                cycleRuntime = null;
                phaseDps.BeginPhase(currentPhase.IsExPhase, startsAt);
                UpdateHpMultiplier();
                BeginEnemyCycle(elapsedTime, startsAt);
                return;
            }
        }
        while (currentPhaseIndex + 1 < phases.Count && elapsedTime >= phases[currentPhaseIndex + 1].StartTime)
        {
            bool followsCycle = cycleRuntime != null;
            cycleRuntime?.Dispose();
            cycleRuntime = null;
            Debug.Log("フェーズ: " + currentPhaseIndex + " -> " + (currentPhaseIndex + 1));
            currentPhaseIndex++;
            currentPhase = phases[currentPhaseIndex];
            phaseDps.BeginPhase(currentPhase.IsExPhase,
                followsCycle || currentPhase is EnemyCyclePhase ? elapsedTime : currentPhase.StartTime);
            UpdateHpMultiplier();
            BeginEnemyCycle(elapsedTime);
            if (cycleRuntime != null && !cycleRuntime.IsComplete) break;
        }
    }

    // 新Phaseの状態と生成個体はGeneratorごとに保持し、共有SOに進行状態を書き込まない。
    private void BeginEnemyCycle(float elapsedTime, double? startsAt = null)
    {
        if (!(currentPhase is EnemyCyclePhase cycle)) return;
        cycleRuntime = new EnemyCycleRuntime(cycle, startsAt ?? cycle.StartTime, (prefab, phase) => SpawnEnemy(phase, prefab), RemoveActiveEnemy);
        cycleRuntime.Tick(elapsedTime, !autoSpawnDisabled);
    }

    private void OnDestroy() => cycleRuntime?.Dispose();

    private void UpdateHpMultiplier()
    {
        CurrentHpMultiplier = currentPhase.IsExPhase
            ? (float)phaseDps.GetHpMultiplier(currentPhase.ReferenceDps, currentPhase.MinHpMultiplier, currentPhase.MaxHpMultiplier)
            : 1f;
    }

    public void RecordPhaseDamage(int actualDamage)
    {
        // Updateより先に被弾した場合も、Phase境界に従って計測する。
        if (currentPhase == null || timer == null)
            return;
        AdvancePhase(timer.ElapsedTime);
        // 旧Phase用の計測を維持しつつ、新Phaseの中ボス用には自分の通常区間だけを別途計測する。
        phaseDps.RecordDamage(actualDamage);
        if (cycleRuntime != null) cycleRuntime.RecordDamage(actualDamage);
    }
    /// <summary>フェーズの間隔と上限に従って敵の生成を繰り返す</summary>

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            AdvancePhase(timer.ElapsedTime);
            // 新Phaseでは区間別の実行時コピーを使用。旧Phaseの参照・生成処理には手を加えない。
            PhaseData spawnPhase = cycleRuntime != null ? cycleRuntime.SpawnPhase : currentPhase;
            if (cycleRuntime != null && !cycleRuntime.CanSpawnAdds)
            {
                yield return null;
                continue;
            }
            int activeCount = activeEnemies.Count;
            int minimum = spawnPhase.MinSpawnCount;
            if (cycleRuntime != null)
            {
                // 中ボスを雑魚の最低維持数に含めず、上限超過の補充・置換ループも防ぐ。
                int bossCount = ProtectedCycleBossCount();
                activeCount -= bossCount;
                minimum = Mathf.Min(minimum, Mathf.Max(0, maxEnemyCount - bossCount));
            }
            // 最低スポーン数を満たすように生成
            if (activeCount < minimum)
            {
                int spawnCount = minimum - activeCount;

                for (int i = 0; i < spawnCount; i++)
                {
                    SpawnEnemy(spawnPhase);
                }
            }
            else
            {
                SpawnEnemy(spawnPhase);
            }

            if (cycleRuntime == null) yield return new WaitForSeconds(spawnPhase.SpawnInterval);
            else
            {
                // 新Phaseだけは長い出現待ちの途中でも中ボス撃破・区間切替に反応する。
                EnemyCycleRuntime waitingCycle = cycleRuntime;
                EnemyCycleStage waitingStage = waitingCycle.Stage;
                float nextSpawnAt = timer.ElapsedTime + spawnPhase.SpawnInterval;
                do { yield return null; }
                while (cycleRuntime == waitingCycle && waitingCycle.Stage == waitingStage && timer.ElapsedTime < nextSpawnAt);
            }
        }
    }
    /// <summary>活動中の敵一覧から返却する個体を除く</summary>

    public void RemoveActiveEnemy(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
        cycleBosses.Remove(enemy);
    }

    private bool IsProtectedCycleBoss(GameObject enemy) => enemy != null && enemy.activeSelf
        && cycleBosses.TryGetValue(enemy, out var life) && life.enemy != null
        && ReferenceEquals(life.enemy.Health, life.health);

    private int ProtectedCycleBossCount()
    {
        int count = 0;
        foreach (var entry in cycleBosses) if (IsProtectedCycleBoss(entry.Key)) count++;
        return count;
    }
    /// <summary>生成位置を確認して敵の個体参照を関連付ける</summary>

    // 新Phaseの中ボスも既存の位置決定・プール・参照設定を使うため、Prefab指定と生成結果を追加。
    // 旧Phaseは従来どおり重みでPrefabを選び、呼び出し側は戻り値を使用しない。
    private GameObject SpawnEnemy(PhaseData phase, GameObject prefabOverride = null)
    {
        GameObject prefab = prefabOverride;
        if (prefab == null) prefab = GetRandomEnemy(phase)?.Prefab;
        if (prefab == null) return null;

        for (int i = 0; i < maxSpawnTry; i++)
        {
            // Playerの向いている方向を基準にスポーン方向を決定
            Vector3 spawnPosition = GetSpawnPosition();

            if (!TryResolveSpawnPosition(prefab, spawnPosition, out spawnPosition))
                continue;

            // 敵の最大数を超えたら最も遠い敵を削除
            if (activeEnemies.Count >= maxEnemyCount)
            {
                GameObject oldEnemy = GetFarthestEnemy();

                // 新Phaseでは中ボスしか残っていない場合、上限を超えて雑魚を生成しない。
                if (oldEnemy == null && (cycleRuntime != null || cycleBosses.Count > 0)) return null;

                if (oldEnemy != null)
                {
                    RemoveActiveEnemy(oldEnemy);
                    if (PoolManager.Instance.TryResolveInstance(oldEnemy, out var oldInstance))
                    {
                        oldInstance.Poolable?.Release();
                    }
                }
            }

            PooledInstance spawned = PoolManager.Instance.Rent(prefab);
            GameObject enemy = spawned.GameObject;

            if (groundedSpawns)
            {
                enemy.transform.SetPositionAndRotation(spawnPosition, prefab.transform.rotation);
                if (enemy.TryGetComponent(out Rigidbody body))
                {
                    body.position = spawnPosition;
                    body.rotation = prefab.transform.rotation;
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
            }
            else
            {
                enemy.transform.position = spawnPosition;
            }

            activeEnemies.Add(enemy);

            Enemy enemyComponent = spawned.Enemy;
            enemyplojectileobject projectile = spawned.EnemyProjectile;
            // 新PhaseのHP補正は中ボスだけ。雑魚は区間を問わず基礎HPのまま、旧Phaseは従来どおり。
            bool cycleBoss = cycleRuntime != null && prefabOverride != null;
            bool isExSpawn = cycleRuntime != null ? cycleBoss : phase.IsExPhase;
            float hpMultiplier = cycleBoss ? cycleRuntime.BossHpMultiplier : CurrentHpMultiplier;
            if (enemyComponent != null)
            {
                enemyComponent.SetSpawner(this, isExSpawn, hpMultiplier);
                if (cycleBoss) cycleBosses[enemy] = (enemyComponent, enemyComponent.Health);
                else cycleBosses.Remove(enemy);
            }
            else if (projectile != null)
            {
                projectile.SetSpawner(this, isExSpawn, hpMultiplier);
            }

            return enemy;
        }
        return null;
    }
    /// <summary>生成位置が他の物体と重ならないか確認する</summary>

    public bool TryGetRelocatePosition(GameObject source, out Vector3 position)
    {
        for (int i = 0; i < maxSpawnTry; i++)
        {
            position = GetSpawnPosition();

            if (TryResolveSpawnPosition(source, position, out position))
                return true;
        }

        position = default;
        return false;
    }

    private Vector3 GetSpawnPosition()
    {
        return transform.position + GetSpawnDirection() * spawnRadius;
    }

    private bool IsSpawnable(Vector3 position)
    {
        return !Physics.CheckSphere(position, 0.5f, Physics.AllLayers);
    }

    private bool TryResolveSpawnPosition(GameObject source, Vector3 candidate, out Vector3 position)
    {
        if (groundedSpawns)
            return GroundedSpawnPlacement.TryGetPosition(source, candidate, out position);

        position = candidate;
        return IsSpawnable(position);
    }

    /// <summary>
    /// フェーズの敵リストからランダムに敵を選択する
    /// </summary>
    /// <param name="phase">フェーズデータ</param>
    /// <returns>敵データ</returns>
    private EnemySpawnData GetRandomEnemy(PhaseData phase)
    {
        // 全体の重みを計算
        int totalWeight = 0;
        foreach (var enemy in phase.Enemies)
        {
            totalWeight += enemy.SpawnWeight;
        }

        // ランダムに敵を選択
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var enemy in phase.Enemies)
        {
            currentWeight += enemy.SpawnWeight;

            // 敵決定
            if (randomValue < currentWeight)
            {
                return enemy;
            }
        }

        return null;
    }

    /// <summary>
    /// 敵をスポーンさせる方向を決定する
    /// </summary>
    /// <returns>スポーン方向</returns>
    private Vector3 GetSpawnDirection()
    {
        Vector3 forward = transform.forward;

        // 全体の重みを計算
        float totalWeight = 0f;
        foreach (var data in spawnDirections)
        {
            totalWeight += data.weight;
        }

        // ランダムに方向を選択
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var data in spawnDirections)
        {
            currentWeight += data.weight;

            // 方向決定
            if (randomValue < currentWeight)
            {
                float angle = Random.Range(-data.angle, data.angle);
                return Quaternion.Euler(0f, angle, 0f) * forward;
            }
        }

        return forward;
    }
    /// <summary>活動中の敵から最も遠い個体を選ぶ</summary>

    private GameObject GetFarthestEnemy()
    {
        GameObject farthestEnemy = null;
        float maxSqrDistance = -1f;

        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy == null || !enemy.activeSelf)
                continue;

            // 新Phaseの中ボスは雑魚補充による遠方個体の入れ替えで消さない。
            if (IsProtectedCycleBoss(enemy)) continue;

            float sqrDistance = (enemy.transform.position - this.transform.position).sqrMagnitude;

            if (sqrDistance > maxSqrDistance)
            {
                maxSqrDistance = sqrDistance;
                farthestEnemy = enemy;
            }
        }

        return farthestEnemy;
    }
}
