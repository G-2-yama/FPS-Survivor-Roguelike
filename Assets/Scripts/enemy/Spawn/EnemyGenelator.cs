using UnityEngine;
using System.Collections.Generic;
using System.Collections;
/// <summary>フェーズ設定に従って敵の生成数と活動期間を管理する</summary>

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] private List<PhaseData> phases;

    private int currentPhaseIndex;
    private PhaseData currentPhase;

    [SerializeField] private float spawnRadius = 15f;

    // 敵のスポーン方向のデータ
    [SerializeField] private SpawnDirectionData[] spawnDirections;
    [SerializeField] private Timer timer;
    [SerializeField] private int maxSpawnTry = 5;
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

    private void Start()
    {
        phases.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

        currentPhaseIndex = 0;
        currentPhase = phases[0];

        StartCoroutine(SpawnRoutine());
    }
    /// <summary>保持済みの参照を使って個体の状態を更新する</summary>

    public void Update()
    {
        if (currentPhaseIndex + 1 >= phases.Count)
            return;

        PhaseData nextPhase = phases[currentPhaseIndex + 1];

        if (timer.ElapsedTime >= nextPhase.StartTime)
        {
            Debug.Log("フェーズ: " + currentPhaseIndex + " -> " + (currentPhaseIndex + 1));
            currentPhaseIndex++;
            currentPhase = phases[currentPhaseIndex];
        }
    }
    /// <summary>フェーズの間隔と上限に従って敵の生成を繰り返す</summary>

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // 最低スポーン数を満たすように生成
            if (activeEnemies.Count < currentPhase.MinSpawnCount)
            {
                int spawnCount = currentPhase.MinSpawnCount - activeEnemies.Count;

                for (int i = 0; i < spawnCount; i++)
                {
                    SpawnEnemy(currentPhase);
                }
            }
            else
            {
                SpawnEnemy(currentPhase);
            }

            yield return new WaitForSeconds(currentPhase.SpawnInterval);
        }
    }
    /// <summary>活動中の敵一覧から返却する個体を除く</summary>

    public void RemoveActiveEnemy(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
    }
    /// <summary>生成位置を確認して敵の個体参照を関連付ける</summary>

    private void SpawnEnemy(PhaseData phase)
    {
        EnemySpawnData enemyData = GetRandomEnemy(phase);

        if (enemyData == null)
            return;

        for (int i = 0; i < maxSpawnTry; i++)
        {
            // Playerの向いている方向を基準にスポーン方向を決定
            Vector3 spawnPosition = GetSpawnPosition();

            // 敵の最大数を超えたら最も遠い敵を削除
            if (activeEnemies.Count >= maxEnemyCount)
            {
                GameObject oldEnemy = GetFarthestEnemy();

                if (oldEnemy != null)
                {
                    RemoveActiveEnemy(oldEnemy);
                    if (PoolManager.Instance.TryResolveInstance(oldEnemy, out var oldInstance))
                    {
                        oldInstance.Poolable?.Release();
                    }
                }
            }

            if (IsSpawnable(spawnPosition))
            {
                PooledInstance spawned = PoolManager.Instance.Rent(enemyData.Prefab);
                GameObject enemy = spawned.GameObject;

                enemy.transform.position = spawnPosition;

                activeEnemies.Add(enemy);

                Enemy enemyComponent = spawned.Enemy;
                enemyplojectileobject projectile = spawned.EnemyProjectile;
                if (enemyComponent != null)
                {
                    enemyComponent.SetSpawner(this);
                }
                else if (projectile != null)
                {
                    projectile.SetSpawner(this);
                }

                return;
            }
        }
    }
    /// <summary>生成位置が他の物体と重ならないか確認する</summary>

    public bool TryGetRelocatePosition(out Vector3 position)
    {
        for (int i = 0; i < maxSpawnTry; i++)
        {
            position = GetSpawnPosition();

            if (IsSpawnable(position))
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