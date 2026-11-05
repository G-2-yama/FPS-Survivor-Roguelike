using System;
using System.Collections.Generic;
using System.Reflection;
using EnemyBalance;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>共有SOの独立性、中ボスの生存期間、新旧Generator接続を一時オブジェクトで検証する</summary>
public static class EnemyCycleValidation
{
    private static readonly Type RuntimeType = typeof(EnemyGenerator).Assembly.GetType("EnemyCycleRuntime");
    private static int checks;

    [MenuItem("Tools/Enemy/敵サイクルPhaseを検証")]
    public static void Validate()
    {
        if (Application.isPlaying) throw new InvalidOperationException("検証はPlayを停止して実行してください");
        checks = 0;
        var objects = new List<Object>();
        var runtimes = new List<IDisposable>();
        try
        {
            GameObject template = NewEnemy("Cycle validation template", objects);
            var source = ScriptableObject.CreateInstance<EnemyCyclePhase>();
            objects.Add(source);
            Set(source, "normalDuration", 2f);
            Set(source, "rushDuration", 3f);
            Set(source, "minSpawnCount", 10);
            Set(source, "spawnInterval", 1f);
            Set(source, "bossSpawnRateMultiplier", 0.5f);
            Set(source, "bossMinimumMultiplier", 0.5f);
            Set(source, "rushSpawnRateMultiplier", 2f);
            Set(source, "rushMinimumMultiplier", 2f);
            Set(source, "bossPrefab", template);
            var entry = new EnemySpawnData();
            Set(entry, "prefab", template);
            Set(source, "enemies", new List<EnemySpawnData> { entry });
            Check(source.Validate(out _), "有効な新Phaseの設定");
            string original = JsonUtility.ToJson(source);

            int spawns = 0;
            GameObject lastBoss = null;
            Func<GameObject, PhaseData, GameObject> spawn = (prefab, phase) =>
            {
                spawns++;
                lastBoss = NewEnemy("Cycle validation boss", objects);
                return lastBoss;
            };
            object runtime = Runtime(source, spawn, runtimes);
            Tick(runtime, 0f);
            Check(Stage(runtime) == EnemyCycleStage.Normal && Get<bool>(runtime, "CanSpawnAdds"), "通常の雑魚区間");
            Call(runtime, "RecordDamage", 242);
            Tick(runtime, 1.99f);
            Check(spawns == 0, "通常区間の間は中ボスを生成しない");
            Set(source, "bossSpawnRateMultiplier", 5f);
            Tick(runtime, 2f);
            var bossAdds = Get<PhaseData>(runtime, "SpawnPhase");
            Check(Stage(runtime) == EnemyCycleStage.Boss && spawns == 1, "中ボス区間へ入り1体生成");
            Check(Get<double>(runtime, "NormalDps") == 121d
                && Mathf.Abs(Get<float>(runtime, "BossHpMultiplier") - 1.1f) < 0.0001f,
                "通常区間の実測DPSから旧EXと同じ平方根倍率を確定");
            Check(bossAdds.MinSpawnCount == 5 && bossAdds.SpawnInterval == 2f,
                "実行時コピーの倍率で雑魚を継続し、元SOの変更は反映しない");
            Check(Get<int>(runtime, "BossCount") == 1, "中ボスを雑魚から分離して数える");
            Tick(runtime, 3f);
            Check(Stage(runtime) == EnemyCycleStage.Boss && spawns == 1, "インターバル中は生存中の中ボスを追加しない");
            ValidateBossCarry(lastBoss, objects);
            Call(runtime, "RecordDamage", 100000);
            Check(Get<double>(runtime, "NormalDps") == 121d
                && Mathf.Abs(Get<float>(runtime, "BossHpMultiplier") - 1.1f) < 0.0001f,
                "中ボス戦中のダメージで確定済みHP倍率を変えない");
            Health firstHealth = lastBoss.GetComponent<Enemy>().Health;
            firstHealth.TakeDamage(100);
            Tick(runtime, 4f);
            Check(Stage(runtime) == EnemyCycleStage.Boss, "早期撃破でもインターバルを短縮しない");
            Tick(runtime, 12f);
            var rush = Get<PhaseData>(runtime, "SpawnPhase");
            Check(Stage(runtime) == EnemyCycleStage.Rush && rush.MinSpawnCount == 20 && rush.SpawnInterval == 0.5f,
                "設定時刻で大群の設定へ移る");
            Tick(runtime, 15f);
            Check(Get<bool>(runtime, "IsComplete") && !Get<bool>(runtime, "CanSpawnAdds"), "大群終了後は追加停止");
            JsonUtility.FromJsonOverwrite(original, source);
            Check(JsonUtility.ToJson(source) == original, "実行中に元Phaseの設定を書き換えない");

            spawns = 0;
            object first = Runtime(source, spawn, runtimes);
            object second = Runtime(source, spawn, runtimes);
            Tick(first, 0f);
            Call(first, "RecordDamage", 800);
            Tick(first, 2f);
            GameObject lostBoss = lastBoss;
            Health lostHealth = lostBoss.GetComponent<Enemy>().Health;
            Tick(second, 1f);
            Check(spawns == 1 && Stage(second) == EnemyCycleStage.Normal, "同じSOを使う別Generatorの進行は独立");
            Check(Mathf.Abs(Get<float>(first, "BossHpMultiplier") - 1.3f) < 0.0001f
                && Get<float>(second, "BossHpMultiplier") == 1f,
                "HP倍率上限とGeneratorごとのDPS計測の独立性");
            lostBoss.SetActive(false);
            Tick(first, 2.1f);
            lostHealth.TakeDamage(100);
            Check(Stage(first) == EnemyCycleStage.Boss, "プール返却を撃破に数えず古いHealthの購読を解除");
            Tick(first, 3f);
            Check(spawns == 2 && lastBoss != lostBoss && Stage(first) == EnemyCycleStage.Boss, "失われた中ボスを再生成");
            Check(Mathf.Abs(Get<float>(first, "BossHpMultiplier") - 1.3f) < 0.0001f,
                "中ボスの再生成でも最初に確定したHP倍率を維持");
            lastBoss.GetComponent<Enemy>().Health.TakeDamage(100);
            Tick(first, 4f);
            Check(Stage(first) == EnemyCycleStage.Boss, "再生成した個体の撃破後も設定時刻まで待つ");
            Tick(first, 12f);
            Check(Stage(first) == EnemyCycleStage.Rush, "インターバル終了で大群へ移る");

            Set(source, "bossSpawnRateMultiplier", 0f);
            object stopped = Runtime(source, spawn, runtimes);
            Tick(stopped, 2f);
            Check(!Get<bool>(stopped, "CanSpawnAdds"), "出現倍率0なら最低維持数があっても補充停止");
            JsonUtility.FromJsonOverwrite(original, source);
            ValidateBossCounts(source, objects, runtimes);
            ValidateGenerator(source, objects);
        }
        finally
        {
            foreach (IDisposable runtime in runtimes) runtime.Dispose();
            for (int i = objects.Count - 1; i >= 0; i--) if (objects[i] != null) Object.DestroyImmediate(objects[i]);
        }
        Debug.Log($"EnemyCycleValidation: {checks} checks passed");
    }

    private static void ValidateBossCounts(EnemyCyclePhase source, List<Object> objects, List<IDisposable> owners)
    {
        string original = JsonUtility.ToJson(source);
        var generated = new List<GameObject>();
        Set(source, "bossSpawnCount", 3);
        object runtime = Runtime(source, (prefab, phase) =>
        {
            var boss = NewEnemy("Multiple boss validation", objects);
            generated.Add(boss);
            return boss;
        }, owners);
        Tick(runtime, 0f);
        Call(runtime, "RecordDamage", 242);
        Tick(runtime, 2f);
        Check(generated.Count == 3 && Get<int>(runtime, "BossCount") == 3, "設定数3体の中ボスを生成・追跡");
        foreach (GameObject boss in generated)
            Check((bool)Call(runtime, "IsBoss", boss), "複数の中ボスをそれぞれ識別");
        generated[0].GetComponent<Enemy>().Health.TakeDamage(100);
        Tick(runtime, 3f);
        Check(generated.Count == 3 && Get<int>(runtime, "BossCount") == 2, "撃破済みの中ボスは補充しない");
        Health returnedHealth = generated[1].GetComponent<Enemy>().Health;
        generated[1].SetActive(false);
        Tick(runtime, 3.1f);
        returnedHealth.TakeDamage(100);
        Tick(runtime, 4f);
        Check(generated.Count == 4 && Get<int>(runtime, "BossCount") == 2, "返却された未撃破の個体だけを再生成");
        Check(Mathf.Abs(Get<float>(runtime, "BossHpMultiplier") - 1.1f) < 0.0001f,
            "複数体と再生成にも同じ確定DPS倍率を使用");
        Tick(runtime, 15f);
        Check(Get<bool>(runtime, "IsComplete"), "複数体が未撃破でも設定時刻でPhase完了");

        Set(source, "bossSpawnCount", 0);
        Set(source, "bossPrefab", null);
        Check(source.Validate(out _), "生成数0は中ボスPrefabなしでも有効");
        int attempts = 0;
        object empty = Runtime(source, (prefab, phase) => { attempts++; return null; }, owners);
        Tick(empty, 2f);
        Tick(empty, 12f);
        Check(attempts == 0 && Get<int>(empty, "BossCount") == 0 && Stage(empty) == EnemyCycleStage.Rush,
            "生成数0でもインターバルと大群の固定スケジュールを維持");
        Set(source, "bossSpawnCount", -1);
        Check(!source.Validate(out _), "負の生成数を拒否");
        JsonUtility.FromJsonOverwrite(original, source);
    }

    public static void RunBatch()
    {
        try { Validate(); EditorApplication.Exit(0); }
        catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
    }

    public static void RunMigrationBatch()
    {
        try
        {
            Validate();
            for (int i = 0; i < 10; i++)
            {
                string path = $"Assets/Datas/Phase/Phase_{i:00}.asset";
                EnemyCyclePhase phase = AssetDatabase.LoadAssetAtPath<EnemyCyclePhase>(path);
                Check(phase != null && phase.Validate(out _), path + "を新Phaseとして読み込める");
                Check(phase.NormalDuration == 30f && phase.BossInterval == 10f && phase.RushDuration == 25f
                    && phase.TotalDuration == 65f && phase.StartTime == i * 65f, path + "の区間時間と開始時刻");
                SerializedProperty rushEntries = new SerializedObject(phase).FindProperty("rushEnemies");
                Check(rushEntries.arraySize >= 2, path + "の大群専用の敵構成");
                Check(!phase.IsExPhase && phase.ReferenceDps > 0f
                    && phase.MinHpMultiplier >= 1f && phase.MaxHpMultiplier >= phase.MinHpMultiplier, path + "のHP補正値");
            }
            PhaseData item = AssetDatabase.LoadAssetAtPath<PhaseData>("Assets/Datas/Phase/itemPhase/Item_Phase.asset");
            Check(item != null && !(item is EnemyCyclePhase) && item.MinSpawnCount == 3 && item.SpawnInterval == 15f,
                "アイテム生成の旧Phaseを保持");
            Debug.Log($"EnemyCycleMigrationValidation: {checks} checks passed");
            EditorApplication.Exit(0);
        }
        catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
    }

    private static void ValidateBossCarry(GameObject boss, List<Object> objects)
    {
        var go = new GameObject("Cycle validation boss protection");
        objects.Add(go);
        var generator = go.AddComponent<EnemyGenerator>();
        var mob = new GameObject("Cycle validation mob");
        objects.Add(mob);
        boss.transform.position = Vector3.right * 1000f;
        mob.transform.position = Vector3.right * 10f;
        Set(generator, "activeEnemies", new List<GameObject> { boss, mob });
        Enemy enemy = boss.GetComponent<Enemy>();
        Health originalHealth = enemy.Health;
        var protectedBosses = (System.Collections.IDictionary)Field(generator, "cycleBosses");
        protectedBosses.Add(boss, (enemy, originalHealth));
        Check(ReferenceEquals(Call(generator, "GetFarthestEnemy"), mob), "前Phaseの中ボスを雑魚の入れ替えで消さない");
        Check((int)Call(generator, "ProtectedCycleBossCount") == 1, "残存中ボスを雑魚の最低維持数に含めない");
        Set(enemy, "<Health>k__BackingField", new Health(100));
        Check(ReferenceEquals(Call(generator, "GetFarthestEnemy"), boss), "同じGameObjectの別の生存期間を中ボス扱いしない");
        Set(enemy, "<Health>k__BackingField", originalHealth);
    }

    private static void ValidateGenerator(EnemyCyclePhase source, List<Object> objects)
    {
        var oldFirst = ScriptableObject.CreateInstance<PhaseData>();
        var oldNext = ScriptableObject.CreateInstance<PhaseData>();
        objects.Add(oldFirst);
        objects.Add(oldNext);
        Set(oldFirst, "minSpawnCount", 3);
        Set(oldFirst, "spawnInterval", 15f);
        Set(oldNext, "startTime", 10f);
        string oldJson = JsonUtility.ToJson(oldFirst);
        var go = new GameObject("Cycle validation generator");
        objects.Add(go);
        var generator = go.AddComponent<EnemyGenerator>();
        var timer = go.AddComponent<Timer>();
        Set(generator, "timer", timer);
        Set(generator, "phases", new List<PhaseData> { oldNext, oldFirst });
        generator.DisableAutoSpawn();
        Call(generator, "Start");
        Set(timer, "<ElapsedTime>k__BackingField", 9f);
        generator.Update();
        Check(ReferenceEquals(Field(generator, "currentPhase"), oldFirst), "旧アイテムPhaseは従来の時刻まで維持");
        Set(timer, "<ElapsedTime>k__BackingField", 10f);
        generator.Update();
        Check(ReferenceEquals(Field(generator, "currentPhase"), oldNext), "旧Phaseは絶対時刻で切り替える");
        Check(JsonUtility.ToJson(oldFirst) == oldJson && oldFirst.MinSpawnCount == 3 && oldFirst.SpawnInterval == 15f,
            "旧Phaseのアセット値・参照を保持");

        var mixedObject = new GameObject("Cycle validation mixed generator");
        objects.Add(mixedObject);
        var mixed = mixedObject.AddComponent<EnemyGenerator>();
        var mixedTimer = mixedObject.AddComponent<Timer>();
        Set(mixed, "timer", mixedTimer);
        Set(source, "startTime", 5f);
        Set(mixed, "phases", new List<PhaseData> { oldFirst, source, oldNext });
        mixed.DisableAutoSpawn();
        Call(mixed, "Start");
        Set(mixedTimer, "<ElapsedTime>k__BackingField", 5f);
        mixed.Update();
        Check(ReferenceEquals(Field(mixed, "currentPhase"), source), "既存Generatorで旧Phaseから新Phaseへ入る");
        Set(mixedTimer, "<ElapsedTime>k__BackingField", 16f);
        mixed.Update();
        Check(ReferenceEquals(Field(mixed, "currentPhase"), source), "次の旧Phaseの時刻を過ぎても区間時間を守る");
        Set(mixedTimer, "<ElapsedTime>k__BackingField", 20f);
        mixed.Update();
        Check(ReferenceEquals(Field(mixed, "currentPhase"), oldNext), "新Phase完了後に既存の旧Phaseへ戻る");
        Set(source, "startTime", 0f);
    }

    private static GameObject NewEnemy(string name, List<Object> objects)
    {
        var go = new GameObject(name);
        objects.Add(go);
        var enemy = go.AddComponent<Enemy>();
        Set(enemy, "<Health>k__BackingField", new Health(100));
        return go;
    }

    private static object Runtime(EnemyCyclePhase source, Func<GameObject, PhaseData, GameObject> spawn, List<IDisposable> owners)
    {
        object runtime = Activator.CreateInstance(RuntimeType, source, 0f, spawn, null);
        owners.Add((IDisposable)runtime);
        return runtime;
    }
    private static void Tick(object runtime, float time) => Call(runtime, "Tick", time, true);
    private static EnemyCycleStage Stage(object runtime) => Get<EnemyCycleStage>(runtime, "Stage");
    private static T Get<T>(object owner, string name) => (T)owner.GetType().GetProperty(name).GetValue(owner);
    private static object Call(object owner, string name, params object[] args) => owner.GetType()
        .GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(owner, args);
    private static FieldInfo FindField(object owner, string name)
    {
        for (Type type = owner.GetType(); type != null; type = type.BaseType)
        {
            FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null) return field;
        }
        throw new MissingFieldException(owner.GetType().Name, name);
    }
    private static object Field(object owner, string name) => FindField(owner, name).GetValue(owner);
    private static void Set(object owner, string name, object value) => FindField(owner, name).SetValue(owner, value);
    private static void Check(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException(label);
        checks++;
    }
}
