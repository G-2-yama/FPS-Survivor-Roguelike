using System;
using EnemyBalance;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>Generatorごとに新敵Phaseのコピーと中ボスの生存期間を管理する</summary>
internal sealed class EnemyCycleRuntime : IDisposable
{
    private readonly EnemyCyclePhase settings;
    private readonly PhaseData bossAdds;
    private readonly PhaseData rush;
    private readonly EnemyCycleTimeline timeline;
    private readonly PhaseDpsTracker normalDps = new PhaseDpsTracker();
    private readonly double normalEndsAt;
    private readonly Func<GameObject, PhaseData, GameObject> spawnBoss;
    private readonly Action<GameObject> forgetBoss;
    private sealed class BossSlot
    {
        public GameObject Prefab;
        public GameObject Instance;
        public Enemy Enemy;
        public Health Health;
        public bool Defeated;
        public Action OnDeath;
    }
    private readonly BossSlot[] bosses;
    private float nextBossAttempt;
    private bool warned;
    private bool hpMultiplierLocked;

    public EnemyCycleStage Stage => timeline.Stage;
    public bool IsComplete => timeline.IsComplete;
    public double EndsAt => timeline.EndsAt;
    public PhaseData SpawnPhase => Stage == EnemyCycleStage.Boss ? bossAdds : Stage == EnemyCycleStage.Rush ? rush : settings;
    public bool CanSpawnAdds => timeline.HasStarted && !IsComplete
        && (Stage != EnemyCycleStage.Boss || settings.BossSpawnRateMultiplier > 0f)
        && (Stage != EnemyCycleStage.Rush || settings.RushSpawnRateMultiplier > 0f);
    public int BossCount
    {
        get
        {
            int count = 0;
            foreach (BossSlot slot in bosses) if (!slot.Defeated && IsCurrentLife(slot)) count++;
            return count;
        }
    }
    public double NormalDps => normalDps.PreviousNormalDps;
    public float BossHpMultiplier { get; private set; } = 1f;

    public EnemyCycleRuntime(EnemyCyclePhase source, double startsAt, Func<GameObject, PhaseData, GameObject> spawnBoss,
        Action<GameObject> forgetBoss = null)
    {
        settings = Object.Instantiate(source);
        settings.hideFlags = HideFlags.HideAndDontSave;
        bossAdds = settings.CreateBossAdds();
        rush = settings.CreateRush();
        // 固定スケジュールの境界をフレームごとの遅延でずらさない。
        timeline = new EnemyCycleTimeline(startsAt, settings.NormalDuration, settings.RushDuration, settings.BossInterval);
        normalEndsAt = startsAt + settings.NormalDuration;
        normalDps.BeginPhase(false, startsAt);
        bosses = new BossSlot[settings.BossSpawnCount];
        for (int i = 0; i < bosses.Length; i++)
        {
            var slot = new BossSlot { Prefab = settings.ChooseBossPrefab() };
            slot.OnDeath = () => slot.Defeated = true;
            bosses[i] = slot;
        }
        this.spawnBoss = spawnBoss;
        this.forgetBoss = forgetBoss;
    }

    private static bool IsCurrentLife(BossSlot slot) => slot.Instance != null && slot.Instance.activeSelf
        && slot.Enemy != null && ReferenceEquals(slot.Enemy.Health, slot.Health);

    public bool IsBoss(GameObject instance)
    {
        foreach (BossSlot slot in bosses)
            if (!slot.Defeated && instance == slot.Instance && IsCurrentLife(slot)) return true;
        return false;
    }

    public void Tick(float time, bool allowSpawning)
    {
        timeline.Advance(time, false);
        if (!hpMultiplierLocked && Stage != EnemyCycleStage.Normal)
        {
            // 旧isExPhaseと同じ計算を、今回の通常区間の終了時に一度だけ確定する。
            normalDps.BeginPhase(true, normalEndsAt);
            BossHpMultiplier = (float)normalDps.GetHpMultiplier(settings.ReferenceDps,
                settings.MinHpMultiplier, settings.MaxHpMultiplier);
            hpMultiplierLocked = true;
        }
        if ((Stage != EnemyCycleStage.Boss && Stage != EnemyCycleStage.Rush) || !allowSpawning) return;
        // 個体ごとに追跡する。撃破済みは補充せず、返却・破棄された未撃破の個体だけ再試行する。
        foreach (BossSlot slot in bosses)
        {
            if (slot.Defeated || IsCurrentLife(slot)) continue;
            if (!ReferenceEquals(slot.Instance, null)) forgetBoss?.Invoke(slot.Instance);
            Unsubscribe(slot);
        }
        if (time < nextBossAttempt) return;
        nextBossAttempt = time + 1f;
        foreach (BossSlot slot in bosses)
        {
            if (slot.Defeated || IsCurrentLife(slot)) continue;
            slot.Instance = spawnBoss(slot.Prefab, settings);
            slot.Enemy = slot.Instance != null ? slot.Instance.GetComponent<Enemy>() : null;
            slot.Health = slot.Enemy != null ? slot.Enemy.Health : null;
            if (slot.Health == null)
            {
                if (!warned)
                {
                    Debug.LogWarning($"{settings.name}: 中ボスを生成できません。Prefab設定・生成位置・上限数を確認してください。未生成分を1秒ごとに再試行します");
                    warned = true;
                }
                continue;
            }
            slot.Health.OnDeath += slot.OnDeath;
            if (slot.Health.IsDead) slot.Defeated = true;
        }
    }

    public void RecordDamage(int actualDamage)
    {
        if (timeline.HasStarted && Stage == EnemyCycleStage.Normal) normalDps.RecordDamage(actualDamage);
    }

    private static void Unsubscribe(BossSlot slot)
    {
        if (slot.Health != null) slot.Health.OnDeath -= slot.OnDeath;
        slot.Health = null;
        slot.Enemy = null;
        slot.Instance = null;
    }

    public void Dispose()
    {
        foreach (BossSlot slot in bosses) Unsubscribe(slot);
        Destroy(settings);
        Destroy(bossAdds);
        Destroy(rush);
    }

    private static void Destroy(Object value)
    {
        if (Application.isPlaying) Object.Destroy(value);
        else Object.DestroyImmediate(value);
    }
}
