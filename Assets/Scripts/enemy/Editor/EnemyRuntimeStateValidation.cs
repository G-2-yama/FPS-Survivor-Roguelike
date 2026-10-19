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
            Debug.Log("EnemyRuntimeStateValidation: PASS (10 checks)");
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

    private static void SetField(object owner, string name, object value)
    {
        owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
