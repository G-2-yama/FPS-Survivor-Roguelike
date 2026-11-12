using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>旧PhaseDataへの移行と、最終Phaseの継続を検証する。</summary>
public static class EnemyPhaseValidation
{
    [MenuItem("Tools/Enemy/旧Phase構成を検証")]
    public static void Validate()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Playを停止して実行してください");
        string scene = File.ReadAllText("Assets/Scenes/GameScene.unity");
        var phases = new List<PhaseData>();
        foreach (string suffix in new[] { "00", "01", "02", "02_B", "03", "03_B", "04", "05", "06", "07", "08", "09" })
        {
            foreach (string name in new[] { "Phase_" + suffix, "EXPhase_" + suffix, "Phase_" + suffix + "_2" })
            {
                string path = "Assets/Datas/Phase/" + name + ".asset";
                PhaseData phase = AssetDatabase.LoadAssetAtPath<PhaseData>(path);
                Check(phase != null && phase.GetType() == typeof(PhaseData), name + ": 旧PhaseData");
                Check(scene.Contains("guid: " + AssetDatabase.AssetPathToGUID(path)), name + ": Scene参照");
                Check(phase.IsExPhase == name.StartsWith("EX"), name + ": EX属性");
                Check(phase.SpawnInterval > 0f && phase.Enemies.Count > 0, name + ": 出現設定");
                Check(phase.Enemies.All(e => e.Prefab != null && e.SpawnWeight > 0), name + ": 敵参照");
                phases.Add(phase);
            }
        }
        Check(phases.Count == 36 && phases.Zip(phases.Skip(1), (a, b) => a.StartTime < b.StartTime).All(x => x),
            "36Phaseの開始時刻が昇順");

        var owner = new GameObject("Legacy phase validation");
        try
        {
            var generator = owner.AddComponent<EnemyGenerator>();
            var timer = owner.AddComponent<Timer>();
            Set(generator, "phases", phases);
            Set(generator, "timer", timer);
            generator.DisableAutoSpawn();
            typeof(EnemyGenerator).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(generator, null);
            generator.RecordPhaseDamage(3000);
            for (int i = 1; i < phases.Count; i++)
            {
                Tick(generator, timer, phases[i].StartTime - 0.001f);
                Check(ReferenceEquals(Get(generator, "currentPhase"), phases[i - 1]), "境界直前: " + phases[i].name);
                Tick(generator, timer, phases[i].StartTime);
                Check(ReferenceEquals(Get(generator, "currentPhase"), phases[i]), "境界: " + phases[i].name);
                if (i == 1) Check(Math.Abs(generator.PreviousNormalDps - 100d) < 0.001d, "通常DPSをEXへ引き継ぐ");
                if (!phases[i].IsExPhase) Check(generator.CurrentHpMultiplier == 1f, "通常・大量のHP補正をリセット");
            }
            foreach (float time in new[] { 650f, 660f, 1200f, 3600f })
            {
                Tick(generator, timer, time);
                Check(ReferenceEquals(Get(generator, "currentPhase"), phases[35]), "最終Phase継続: " + time);
            }
        }
        finally { Object.DestroyImmediate(owner); }
        Debug.Log("EnemyPhaseValidation: 36Phaseの参照・境界・DPS・エンドレス継続の検証に成功");
    }

    private static void Tick(EnemyGenerator generator, Timer timer, float time)
    {
        Set(timer, "<ElapsedTime>k__BackingField", time);
        generator.Update();
    }

    private static object Get(object owner, string name) => owner.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
    private static void Set(object owner, string name, object value) => owner.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value);
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
