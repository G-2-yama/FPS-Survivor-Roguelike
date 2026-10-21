using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>共有Configと個体の距離、および取得範囲の独立性を検証する</summary>
public static class EnemyRuntimeStateValidation
{
    [MenuItem("Tools/Enemy/実行時の距離を検証")]
    public static void Validate()
    {
        var config = ScriptableObject.CreateInstance<EnemyConfig>();
        var variant = Object.Instantiate(config);
        var movement = ScriptableObject.CreateInstance<stop>();
        var pickupSettings = ScriptableObject.CreateInstance<PlayerStatsSettings>();
        var firstObject = new GameObject("Distance validation A");
        var secondObject = new GameObject("Distance validation B");
        var target = new GameObject("Distance validation target");
        firstObject.SetActive(false);
        secondObject.SetActive(false);
        target.transform.position = Vector3.right * 4f;

        try
        {
            config.chaseMovedata = movement;
            config.combatMovedata = movement;
            SetField(config, "engagedistance", 5f);
            SetField(variant, "engagedistance", 9f);
            string originalConfig = JsonUtility.ToJson(config);

            EnemyBrain first = CreateBrain(firstObject, config, target.transform);
            EnemyBrain second = CreateBrain(secondObject, config, target.transform);
            Assert(first.EngageDistance == 5f && second.EngageDistance == 5f,
                "Configの初期値を初期化リストなしで読み込める");

            first.ChaseState.Update();
            Assert(first.StateMachine.CurrentState == first.CombatState, "初期距離内で戦闘へ移行する");
            first.SetEngageDistance(2f);
            first.CombatState.Update();
            Assert(first.StateMachine.CurrentState == first.ChaseState, "個体の変更後の距離で戦闘から離脱する");
            first.SetEngageDistance(6f);
            first.ChaseState.Update();
            Assert(first.StateMachine.CurrentState == first.CombatState, "個体の変更後の距離で戦闘へ移行する");
            Assert(second.EngageDistance == 5f && JsonUtility.ToJson(config) == originalConfig,
                "同じConfigを使う別個体と共有SOへ変更が波及しない");

            first.ResetBrain();
            Assert(first.EngageDistance == 5f, "再出現時に距離を初期値へ戻す");
            SetField(second, "config", variant);
            second.ResetBrain();
            Assert(second.EngageDistance == 9f, "差し替えたConfigから距離を読み込む");
            first.SetEngageDistance(-1f);
            Assert(first.EngageDistance == 0f, "負の距離を防ぐ");

            var firstStats = new PlayerStats(pickupSettings);
            var secondStats = new PlayerStats(pickupSettings);
            float initialRadius = pickupSettings.PickupRadius;
            firstStats.AddPickupRadius(5f);
            Assert(firstStats.PickupRadius == initialRadius + 5f
                && secondStats.PickupRadius == initialRadius
                && pickupSettings.PickupRadius == initialRadius
                && JsonUtility.ToJson(config) == originalConfig,
                "取得範囲アップは対象プレイヤーだけに適用し共有SOを変更しない");
            firstStats.AddPickupRadius(-5f);
            Assert(firstStats.PickupRadius == initialRadius, "取得範囲アップを解除できる");
            ValidateKnockback(first, second, config);
            ValidatePhaseHp(first, second, config);
            Debug.Log("EnemyRuntimeStateValidation: PASS (distance, pickup range, knockback cooldown, phase DPS and pooled HP)");
        }
        finally
        {
            Object.DestroyImmediate(firstObject);
            Object.DestroyImmediate(secondObject);
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(config);
            Object.DestroyImmediate(variant);
            Object.DestroyImmediate(movement);
            Object.DestroyImmediate(pickupSettings);
        }
    }

    public static void RunBatch()
    {
        try
        {
            Validate();
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
    }

    private static EnemyBrain CreateBrain(GameObject root, EnemyConfig config, Transform target)
    {
        var brain = root.AddComponent<EnemyBrain>();
        var provider = root.AddComponent<EnemyTargetProvider>();
        provider.SetTarget(target);
        SetField(brain, "config", config);
        SetField(brain, "targetProvider", provider);
        typeof(EnemyBrain).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(brain, null);
        brain.ResetBrain();
        return brain;
    }

    private static void ValidateKnockback(EnemyBrain first, EnemyBrain second, EnemyConfig config)
    {
        SetField(config, "knockbackCooldown", 1f);
        SetField(second, "config", config);
        first.ResetBrain();
        second.ResetBrain();

        Assert(!first.KnockbackState.TryEnter(Vector3.back, 0f),
            "ノックバック力0では状態を切り替えない");
        SetField(config, "knockbackResistance", 1f);
        Assert(!first.KnockbackState.TryEnter(Vector3.back, 5f),
            "耐性100%では状態を切り替えない");
        SetField(config, "knockbackResistance", 0f);
        Assert(first.KnockbackState.TryEnter(Vector3.back, 5f), "初回のノックバックを受け付ける");
        Assert(!first.KnockbackState.TryEnter(Vector3.back, 10f), "硬直中の追加被弾で硬直を延長しない");

        first.ChangeState(first.ChaseState);
        first.ChaseState.Update();
        Assert(first.StateMachine.CurrentState == first.CombatState, "ノックバック後に戦闘へ復帰できる");
        Assert(!first.KnockbackState.TryEnter(Vector3.back, 5f)
            && first.StateMachine.CurrentState == first.CombatState,
            "終了後の耐性中は戦闘状態を中断しない");
        Assert(second.KnockbackState.TryEnter(Vector3.back, 5f), "耐性時間は別個体に波及しない");

        SetField(first.KnockbackState, "nextKnockbackTime", Time.time - 0.01f);
        Assert(first.KnockbackState.TryEnter(Vector3.back, 5f), "耐性時間を過ぎると再び受け付ける");
        first.ResetBrain();
        Assert(first.KnockbackState.TryEnter(Vector3.back, 5f), "再出現時に耐性時間をリセットする");

        SetField(config, "knockbackCooldown", 0f);
        first.ChangeState(first.ChaseState);
        Assert(first.KnockbackState.TryEnter(Vector3.back, 5f), "クールタイム0なら終了後すぐに受け付ける");
    }

    private static void ValidatePhaseHp(EnemyBrain first, EnemyBrain second, EnemyConfig config)
    {
        var generatorObject = new GameObject("Phase HP validation generator");
        var normalPhase = ScriptableObject.CreateInstance<PhaseData>();
        var exPhase = ScriptableObject.CreateInstance<PhaseData>();
        var generator = generatorObject.AddComponent<EnemyGenerator>();
        var timer = generatorObject.AddComponent<Timer>();
        config.MaxHp = 100;
        string original = JsonUtility.ToJson(config);
        Enemy firstEnemy = CreateEnemy(first, config);
        Enemy secondEnemy = CreateEnemy(second, config);
        try
        {
            SetField(exPhase, "startTime", 10f);
            SetField(exPhase, "isExPhase", true);
            SetField(exPhase, "referenceDps", 0.5f);
            SetField(generator, "phases", new System.Collections.Generic.List<PhaseData> { normalPhase, exPhase });
            SetField(generator, "currentPhase", normalPhase);
            SetField(generator, "timer", timer);
            var tracker = (EnemyBalance.PhaseDpsTracker)typeof(EnemyGenerator)
                .GetField("phaseDps", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(generator);
            tracker.BeginPhase(false, 0);

            firstEnemy.OnGet();
            firstEnemy.SetSpawner(generator, true, 1.3f);
            secondEnemy.OnGet();
            secondEnemy.SetSpawner(generator, true, 1f);
            Assert(firstEnemy.Health.MaxHP == 130 && secondEnemy.Health.MaxHP == 100,
                "共有Configから個体ごとに別のHPを作れる");
            firstEnemy.Health.TakeDamage(20);
            Assert(firstEnemy.Health.CurrentHP == 110 && secondEnemy.Health.CurrentHP == 100,
                "HPと被ダメージは別個体へ波及しない");

            firstEnemy.OnRelease();
            firstEnemy.OnGet();
            firstEnemy.SetSpawner(generator, false, 0.5f);
            Assert(firstEnemy.Health.CurrentHP == 100 && firstEnemy.Health.MaxHP == 100,
                "再出現時に前回倍率を残さず、通常Phaseでは基礎HPに戻る");

            SetField(timer, "<ElapsedTime>k__BackingField", 5f);
            firstEnemy.Health.TakeDamage(20);
            SetField(timer, "<ElapsedTime>k__BackingField", 10f);
            generator.Update();
            Assert(generator.PreviousNormalDps == 2 && Mathf.Abs(generator.CurrentHpMultiplier - 1.3f) < 0.0001f,
                "通常Phaseの実ダメージを集計しEx開始時に上限内の倍率へ確定する");
            firstEnemy.Health.TakeDamage(10);
            Assert(generator.PreviousNormalDps == 2 && secondEnemy.Health.MaxHP == 100,
                "Ex開始後の追加ダメージやPhase切替で既存の倍率と個体HPを再計算しない");
            Assert(JsonUtility.ToJson(config) == original, "敵の共有SOの基礎値を変更しない");
        }
        finally
        {
            firstEnemy.OnRelease();
            secondEnemy.OnRelease();
            Object.DestroyImmediate(generatorObject);
            Object.DestroyImmediate(normalPhase);
            Object.DestroyImmediate(exPhase);
        }
    }

    private static Enemy CreateEnemy(EnemyBrain brain, EnemyConfig config)
    {
        Enemy enemy = brain.gameObject.AddComponent<Enemy>();
        SetField(enemy, "config", config);
        SetField(enemy, "enemyBrain", brain);
        SetField(enemy, "contactTarget", brain.gameObject.AddComponent<PlayerContactTarget>());
        return enemy;
    }

    private static void SetField(object owner, string name, object value)
    {
        owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
