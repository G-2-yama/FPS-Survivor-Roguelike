using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Profiling;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>同一の接触コールバックとプール操作で整理前後のCPUと管理メモリを比較する</summary>
[InitializeOnLoad]
public static class CombatPerformanceComparison
{
    private const string PendingKey = "CombatPerformanceComparison.Pending";
    private const int Warmup = 300;
    private const int Samples = 600;
    private const int Repetitions = 5;
    private static readonly List<string> results = new();

    /// <summary>初回インポート後のプレイ開始も拾えるよう検証開始を監視する</summary>
    static CombatPerformanceComparison() => EditorApplication.update += CheckPending;

    /// <summary>隔離プロジェクトの空Sceneで同条件の計測を開始する</summary>
    public static void RunBatch()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("計測は隔離したバッチ環境で実行してください");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetBool(PendingKey, true);
        EditorApplication.EnterPlaymode();
    }

    /// <summary>プレイ中にコールバックを直接呼び、描画やEditorの待ち時間を測定から除く</summary>
    private static void CheckPending()
    {
        if (!EditorApplication.isPlaying || !SessionState.GetBool(PendingKey, false)) return;
        SessionState.SetBool(PendingKey, false);
        try
        {
            Time.fixedDeltaTime = 0.02f;
            results.Clear();
            results.Add("Scenario,MedianUs,P95Us,AllocationsPerSample,ManagedRetainedBytes");
            using (var recorder = StartAllocationRecorder())
            {
                if (FinishAllocationRecorder(recorder) != 0)
                    throw new InvalidOperationException("GC.Allocの空計測に割り当てが混入しました");
            }
            using (var recorder = StartAllocationRecorder())
            {
                GC.KeepAlive(new byte[1024]);
                if (FinishAllocationRecorder(recorder) != 1)
                    throw new InvalidOperationException("GC.Allocの校正に失敗しました");
            }
            var manager = new GameObject("計測管理");
            manager.AddComponent<LifetimeRunner>();
            PoolManager pool = manager.AddComponent<PoolManager>();
            for (int run = 0; run < Repetitions; run++) MeasurePool(pool, false);
            MeasurePool(pool, true);
            MeasureContacts(24);
            MeasureContacts(200);
            MeasureArea(pool);
            File.WriteAllLines(Path.Combine(Directory.GetCurrentDirectory(), "CombatPerformanceResults.csv"), results);
            UnityEngine.Debug.Log("CPUと管理メモリの同条件計測が完了しました");
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            UnityEngine.Debug.LogException(error);
            EditorApplication.Exit(1);
        }
    }

    /// <summary>初回生成と再利用を分け、200個体の参照キャッシュを比較する</summary>
    private static void MeasurePool(PoolManager pool, bool reuse)
    {
        var source = new GameObject(reuse ? "再利用弾Prefab" : "初回弾Prefab");
        source.SetActive(false);
        source.AddComponent<SphereCollider>();
        source.AddComponent<Rigidbody>().isKinematic = true;
        source.AddComponent<DamageObject>();
        const int count = 200;
        var handles = new PooledInstance[count];
        var owners = new PoolableObject[count];
        long before = GC.GetTotalMemory(true);
        using var allocationRecorder = StartAllocationRecorder();
        long started = Stopwatch.GetTimestamp();
        for (int i = 0; i < count; i++) handles[i] = pool.Rent(source);
        double initialUs = ElapsedUs(started);
        long initialAllocated = FinishAllocationRecorder(allocationRecorder);
        for (int i = 0; i < count; i++) owners[i] = handles[i].GameObject.GetComponent<PoolableObject>();
        long retained = GC.GetTotalMemory(true) - before;
        if (!reuse) results.Add($"PoolInitial200,{initialUs:F3},{initialUs:F3},{initialAllocated},{retained}");
        for (int i = 0; i < count; i++) owners[i].Release();
        if (reuse)
        {
            Measure("PoolReuse200", step =>
            {
                for (int i = 0; i < count; i++) handles[i] = pool.Rent(source);
                for (int i = 0; i < count; i++) owners[i].Release();
            }, retained);
        }
    }

    /// <summary>24体と200体の継続接触と再侵入を同じ順序で再現する</summary>
    private static void MeasureContacts(int count)
    {
        var createPlayer = typeof(PlayerSettingsValidation).GetMethod("CreatePlayer", BindingFlags.Static | BindingFlags.NonPublic);
        Player player = (Player)createPlayer.Invoke(null, new object[] { "計測プレイヤー", Vector3.zero });
        var collisionSettings = new SerializedObject(player.Settings.Collision);
        collisionSettings.FindProperty("contactDamageEnabled").boolValue = false;
        collisionSettings.ApplyModifiedPropertiesWithoutUndo();
        PlayerEnemyPushZone zone = player.GetComponentInChildren<PlayerEnemyPushZone>();
        Action<Collider> stay = BindCollider(zone, "OnTriggerStay");
        Action<Collider> exit = BindCollider(zone, "OnTriggerExit");
        Action tick = (Action)Delegate.CreateDelegate(typeof(Action), zone,
            typeof(PlayerEnemyPushZone).GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.NonPublic));
        var colliders = new Collider[count];
        var roots = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            var enemy = new GameObject("計測接触対象");
            roots[i] = enemy;
            enemy.layer = LayerMask.NameToLayer("Enemy");
            enemy.transform.position = new Vector3(0.2f, 0f, 0.2f);
            Rigidbody body = enemy.AddComponent<Rigidbody>();
            body.isKinematic = true;
            colliders[i] = enemy.AddComponent<SphereCollider>();
            enemy.AddComponent<PlayerContactTarget>().BeginLife(body, null);
        }
        Physics.SyncTransforms();
        Measure("Contact" + count, step =>
        {
            tick();
            for (int i = 0; i < count; i++) stay(colliders[i]);
            // 一定間隔の再侵入で接触状態の生成と解除も同条件で比較する
            if (step % 25 == 0) for (int i = 0; i < count; i++) exit(colliders[i]);
        });
        for (int i = 0; i < count; i++) Object.DestroyImmediate(roots[i]);
        Object.DestroyImmediate(player.gameObject);
    }

    /// <summary>200対象の継続範囲ダメージと再侵入を比較する</summary>
    private static void MeasureArea(PoolManager pool)
    {
        var source = new GameObject("計測対象Prefab");
        source.SetActive(false);
        source.AddComponent<SphereCollider>();
        source.AddComponent<CombatDamageProbe>();
        var colliders = new Collider[200];
        for (int i = 0; i < colliders.Length; i++) colliders[i] = pool.Rent(source).GameObject.GetComponent<Collider>();
        var areaSource = new GameObject("計測範囲Prefab");
        areaSource.SetActive(false);
        areaSource.AddComponent<SphereCollider>().isTrigger = true;
        var prefabArea = areaSource.AddComponent<DamageArea>();
        var data = new SerializedObject(prefabArea);
        data.FindProperty("mode").enumValueIndex = (int)DamageAreaMode.Interval;
        data.FindProperty("targetTeam").intValue = (int)TeamType.Enemy;
        data.ApplyModifiedPropertiesWithoutUndo();
        var area = pool.Rent(areaSource).GameObject.GetComponent<DamageArea>();
        Action<Collider> stay = BindCollider(area, "OnTriggerStay");
        Action<Collider> exit = BindCollider(area, "OnTriggerExit");
        Measure("Area200", step =>
        {
            for (int i = 0; i < colliders.Length; i++) stay(colliders[i]);
            if (step % 25 == 0) for (int i = 0; i < colliders.Length; i++) exit(colliders[i]);
        });
        area.Release();
    }

    /// <summary>ウォームアップ後に5回計測し、中央の実行の統計を記録する</summary>
    private static void Measure(string name, Action<int> action, long retained = 0)
    {
        long before = GC.GetTotalMemory(true);
        for (int i = 0; i < Warmup; i++) action(i);
        long memory = GC.GetTotalMemory(true) - before;
        var runs = new List<(double median, double p95, double allocated)>();
        var samples = new double[Samples];
        for (int run = 0; run < Repetitions; run++)
        {
            for (int i = 0; i < Samples; i++)
            {
                long start = Stopwatch.GetTimestamp();
                action(i);
                samples[i] = ElapsedUs(start);
            }
            // UnityのMonoでは通常の.NET割り当てカウンターを使えないため、GC.Allocを別走行で記録する
            using var allocationRecorder = StartAllocationRecorder();
            for (int i = 0; i < Samples; i++) action(i);
            long allocations = FinishAllocationRecorder(allocationRecorder);
            Array.Sort(samples);
            runs.Add((samples[Samples / 2], samples[(int)(Samples * 0.95)], (double)allocations / Samples));
        }
        runs.Sort((left, right) => left.median.CompareTo(right.median));
        var middle = runs[Repetitions / 2];
        results.Add($"{name},{middle.median:F3},{middle.p95:F3},{middle.allocated},{retained + memory}");
    }

    /// <summary>計測前に一度だけ物理通知の呼び出し先を確保する</summary>
    private static Action<Collider> BindCollider(object target, string method) =>
        (Action<Collider>)Delegate.CreateDelegate(typeof(Action<Collider>), target,
            target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic));

    /// <summary>ストップウォッチの差分をマイクロ秒へ変換する</summary>
    private static double ElapsedUs(long started) => (Stopwatch.GetTimestamp() - started) * 1000000d / Stopwatch.Frequency;

    /// <summary>メインスレッドの割り当てだけをUnityのGC.Allocから収集する</summary>
    private static ProfilerRecorder StartAllocationRecorder()
    {
        UnityEngine.Profiling.Profiler.enabled = true;
        return ProfilerRecorder.StartNew(ProfilerCategory.Internal, "GC.Alloc", 1,
            ProfilerRecorderOptions.SumAllSamplesInFrame | ProfilerRecorderOptions.CollectOnlyOnCurrentThread);
    }

    /// <summary>同じフレーム内の収集を確定して割り当て回数を返す</summary>
    private static long FinishAllocationRecorder(ProfilerRecorder recorder)
    {
        recorder.Stop();
        UnityEngine.Profiling.Profiler.enabled = false;
        if (!recorder.Valid) throw new InvalidOperationException("GC.Allocの計測を開始できません");
        // GC.AllocのValueは処理時間なので、割り当て量と混同せずCountを使用する
        return recorder.Count == 0 ? 0 : recorder.GetSample(0).Count;
    }
}
