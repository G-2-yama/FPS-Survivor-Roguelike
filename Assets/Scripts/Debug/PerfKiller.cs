#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PerfKiller : MonoBehaviour
{
    public const float Interval = 5f;
    public const int KillCount = 500;
    private const string EnabledKey = "PerfKiller.Enabled";

    private static bool isEnabled;

    private EnemyGenerator generator;
    private float nextKillTime;
    private bool wasEnabled;
    private readonly List<GameObject> targets = new List<GameObject>();

    public static bool Enabled
    {
        get => EditorPrefs.GetBool(EnabledKey, false);
        set
        {
            isEnabled = value;
            EditorPrefs.SetBool(EnabledKey, value);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Hook()
    {
        isEnabled = EditorPrefs.GetBool(EnabledKey, false);

        SceneManager.sceneLoaded += (_, _) =>
        {
            if (Object.FindAnyObjectByType<EnemyGenerator>() == null) return;
            if (Object.FindAnyObjectByType<PerfKiller>() != null) return;

            new GameObject("PerfKiller").AddComponent<PerfKiller>();
        };
    }

    private void Update()
    {
        if (!isEnabled)
        {
            wasEnabled = false;
            return;
        }

        if (!wasEnabled)
        {
            wasEnabled = true;
            nextKillTime = Time.time + Interval;
        }

        if (generator == null) generator = PickGenerator();
        if (generator == null || Time.time < nextKillTime) return;
        nextKillTime = Time.time + Interval;

        targets.Clear();
        targets.AddRange(generator.ActiveEnemies);

        int killed = 0;
        foreach (var obj in targets)
        {
            if (killed >= KillCount) break;
            if (obj == null || !obj.activeSelf) continue;

            var enemy = obj.GetComponent<Enemy>();
            if (enemy == null || enemy.Health == null || enemy.Health.IsDead) continue;

            enemy.TakeDamage(enemy.Health.CurrentHP, 0f);
            killed++;
        }
        Debug.Log($"PerfKiller: {killed} / {targets.Count}");
    }

    private static EnemyGenerator PickGenerator()
    {
        EnemyGenerator best = null;
        foreach (var g in Object.FindObjectsByType<EnemyGenerator>(FindObjectsSortMode.None))
        {
            if (best == null || g.ActiveEnemies.Count > best.ActiveEnemies.Count) best = g;
        }
        return best;
    }
}
#endif
