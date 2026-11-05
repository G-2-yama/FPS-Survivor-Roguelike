using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>雑魚→雑魚を伴う中ボス→大群を1つにまとめた敵専用Phase</summary>
[CreateAssetMenu(fileName = "EnemyCyclePhase", menuName = "EnemyGenerator/Enemy Cycle Phase")]
public sealed class EnemyCyclePhase : PhaseData
{
    [Header("通常の雑魚区間")]
    [SerializeField, Min(0f), Tooltip("この秒数だけ基準の雑魚を出現させてから設定数の中ボスを生成する")]
    private float normalDuration = 30f;

    [Header("中ボス区間（通常と大群のインターバル）")]
    [SerializeField, Min(0f), Tooltip("通常終了から大群開始までの秒数。中ボスの撃破時刻によらず固定")]
    private float bossInterval = 10f;
    [SerializeField, Min(0), Tooltip("このPhaseで生成する中ボスの合計数。撃破した個体は補充しない。0で中ボスなし")]
    private int bossSpawnCount = 1;
    [SerializeField, Tooltip("ルートにEnemyがあるPrefab。位置が確保できるまで再試行する")]
    private GameObject bossPrefab;
    [SerializeField, Tooltip("設定した場合はこちらの重みから個体ごとに中ボスを選ぶ。空の場合はBoss Prefabを使用")]
    private List<EnemySpawnData> bossCandidates = new List<EnemySpawnData>();
    [SerializeField, Min(0f), Tooltip("中ボス戦中の雑魚の出現頻度。1で通常と同じ、0で追加停止")]
    private float bossSpawnRateMultiplier = 0.4f;
    [SerializeField, Min(0f), Tooltip("中ボス戦中の雑魚の最低維持数の倍率。中ボス自身は数に含めない")]
    private float bossMinimumMultiplier = 0.4f;

    [Header("大群区間")]
    [SerializeField, Min(0f), Tooltip("インターバル終了からこの秒数だけ大群を生成し、次のPhaseへ進む")]
    private float rushDuration = 25f;
    [SerializeField, Min(0f), Tooltip("大群の出現頻度の倍率。0で追加停止")]
    private float rushSpawnRateMultiplier = 3f;
    [SerializeField, Min(0f), Tooltip("大群の雑魚の最低維持数の倍率")]
    private float rushMinimumMultiplier = 3f;
    [SerializeField, Tooltip("空なら基準の敵構成を使う。設定すると大群区間だけこの構成に切り替える")]
    private List<EnemySpawnData> rushEnemies = new List<EnemySpawnData>();

    public float NormalDuration => normalDuration;
    public float RushDuration => rushDuration;
    public float BossInterval => bossInterval;
    public int BossSpawnCount => bossSpawnCount;
    public float TotalDuration => normalDuration + bossInterval + rushDuration;
    public GameObject BossPrefab => bossPrefab;
    public float BossSpawnRateMultiplier => bossSpawnRateMultiplier;
    public float RushSpawnRateMultiplier => rushSpawnRateMultiplier;

    internal GameObject ChooseBossPrefab()
    {
        if (bossCandidates == null || bossCandidates.Count == 0) return bossPrefab;
        int total = 0;
        foreach (EnemySpawnData candidate in bossCandidates) total += candidate.SpawnWeight;
        int selected = UnityEngine.Random.Range(0, total);
        foreach (EnemySpawnData candidate in bossCandidates)
        {
            selected -= candidate.SpawnWeight;
            if (selected < 0) return candidate.Prefab;
        }
        return bossCandidates[bossCandidates.Count - 1].Prefab;
    }

    internal PhaseData CreateBossAdds() => CreateAdds(bossMinimumMultiplier, bossSpawnRateMultiplier);
    internal PhaseData CreateRush() => CreateAdds(rushMinimumMultiplier, rushSpawnRateMultiplier, rushEnemies);

    private PhaseData CreateAdds(float minimumMultiplier, float rateMultiplier, List<EnemySpawnData> enemies = null)
    {
        int minimum = (int)Math.Min(int.MaxValue, Math.Ceiling((double)MinSpawnCount * minimumMultiplier));
        float interval = rateMultiplier > 0f ? Mathf.Max(0.01f, SpawnInterval / rateMultiplier) : SpawnInterval;
        return CreateSpawnVariant(minimum, interval, enemies);
    }

    public bool Validate(out string error)
    {
        if (!Finite(StartTime) || StartTime < 0f) { error = "Start Timeは0以上の有限値にしてください"; return false; }
        if (MinSpawnCount < 0 || !Finite(SpawnInterval) || SpawnInterval <= 0f)
        { error = "基準の最低維持数は0以上、出現間隔は0より大きい有限値にしてください"; return false; }
        if (!Finite(normalDuration) || normalDuration < 0f || !Finite(rushDuration) || rushDuration < 0f
            || !Finite(bossInterval) || bossInterval < 0f || !Finite(TotalDuration) || TotalDuration <= 0f)
        { error = "各区間の時間は0以上、合計は0より大きい有限値にしてください"; return false; }
        if (!Finite(bossSpawnRateMultiplier) || bossSpawnRateMultiplier < 0f
            || !Finite(bossMinimumMultiplier) || bossMinimumMultiplier < 0f
            || !Finite(rushSpawnRateMultiplier) || rushSpawnRateMultiplier < 0f
            || !Finite(rushMinimumMultiplier) || rushMinimumMultiplier < 0f)
        { error = "雑魚の出現倍率と維持数倍率は0以上の有限値にしてください"; return false; }
        if (bossSpawnCount < 0) { error = "中ボスの生成数は0以上にしてください"; return false; }
        if (bossSpawnCount > 0 && bossCandidates != null && bossCandidates.Count > 0)
        {
            if (!ValidEnemies(bossCandidates, true))
            { error = "中ボス候補にはルートにEnemyがあるPrefabと正の重みを設定してください"; return false; }
        }
        else if (bossSpawnCount > 0 && (bossPrefab == null || bossPrefab.GetComponent<Enemy>() == null))
        { error = "中ボスにはルートにEnemyがあるPrefabを設定してください"; return false; }
        if (!Finite(ReferenceDps) || !Finite(MinHpMultiplier) || !Finite(MaxHpMultiplier))
        { error = "中ボスHP補正のDPSと倍率は有限値にしてください"; return false; }
        if (!ValidEnemies(Enemies)) { error = "基準の敵構成に敵Prefabと正の重みを設定してください"; return false; }
        if (rushEnemies != null && rushEnemies.Count > 0 && !ValidEnemies(rushEnemies))
        { error = "大群の敵構成に敵Prefabと正の重みを設定してください"; return false; }
        error = null;
        return true;
    }

    private static bool ValidEnemies(IReadOnlyList<EnemySpawnData> enemies, bool bossOnly = false)
    {
        if (enemies == null || enemies.Count == 0) return false;
        long totalWeight = 0;
        foreach (EnemySpawnData entry in enemies)
        {
            if (entry == null || entry.Prefab == null || entry.SpawnWeight <= 0
                || (entry.Prefab.GetComponent<Enemy>() == null
                    && (bossOnly || entry.Prefab.GetComponent<enemyplojectileobject>() == null)))
                return false;
            totalWeight += entry.SpawnWeight;
        }
        return totalWeight <= int.MaxValue;
    }

    private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}
