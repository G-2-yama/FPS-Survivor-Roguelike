using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>攻撃参照の移行と登録表およびプールの回帰検証を提供する</summary>
[InitializeOnLoad]
public static class CombatReferenceValidation
{
    private const string BatchKey = "CombatReferenceValidation.Pending";
    private static readonly List<string> checks = new();

    /// <summary>バッチ検証のプレイ開始を監視する</summary>
    static CombatReferenceValidation()
    {
        EditorApplication.playModeStateChanged += HandlePlayState;
        EditorApplication.update += CheckPendingTests;
    }

    /// <summary>初回インポートの再読み込み後も検証の開始を復元する</summary>
    private static void CheckPendingTests()
    {
        if (EditorApplication.isPlaying && SessionState.GetBool(BatchKey, false)) RunTests();
    }

    /// <summary>既存PrefabとSceneへ明示的な攻撃参照を設定する</summary>
    [MenuItem("Tools/Combat/参照を移行して検証")]
    public static void MigrateAssets()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        SceneSetup[] previousScenes = EditorSceneManager.GetSceneManagerSetup();
        try
        {
            int changed = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".prefab") || path.Contains("/_Recovery/")) continue;
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    if (ConfigureBindings(root))
                    {
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                        changed++;
                    }
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                GameObject[] roots = scene.GetRootGameObjects();
                Camera camera = roots.SelectMany(root => root.GetComponentsInChildren<Camera>(true))
                    .FirstOrDefault(candidate => candidate.CompareTag("MainCamera"));
                bool dirty = false;
                foreach (GameObject root in roots)
                {
                    dirty |= ConfigureBindings(root);
                    if (camera == null) continue;
                    foreach (Weapon weapon in root.GetComponentsInChildren<Weapon>(true))
                    {
                        var serialized = new SerializedObject(weapon);
                        SerializedProperty property = serialized.FindProperty("aimTransform");
                        if (property.objectReferenceValue != null) continue;
                        property.objectReferenceValue = camera.transform;
                        serialized.ApplyModifiedPropertiesWithoutUndo();
                        dirty = true;
                    }
                }
                if (dirty) { EditorSceneManager.SaveScene(scene); changed++; }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"攻撃参照の移行と検証に成功しました 変更した資産数={changed}");
        }
        finally
        {
            if (!Application.isBatchMode) EditorSceneManager.RestoreSceneManagerSetup(previousScenes);
        }
    }

    /// <summary>同一GameObjectの対象だけにColliderを対応付ける</summary>
    private static bool ConfigureBindings(GameObject root)
    {
        bool dirty = false;
        foreach (MonoBehaviour component in root.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (!(component is IDamageable)) continue;
            GameObject owner = component.gameObject;
            DamageTargetBinding binding = owner.GetComponent<DamageTargetBinding>();
            if (binding == null) { binding = owner.AddComponent<DamageTargetBinding>(); dirty = true; }
            var serialized = new SerializedObject(binding);
            SerializedProperty source = serialized.FindProperty("targetSource");
            MonoBehaviour expected = owner.GetComponent<IDamageable>() as MonoBehaviour;
            if (source.objectReferenceValue != expected) { source.objectReferenceValue = expected; dirty = true; }
            SerializedProperty targets = serialized.FindProperty("targetColliders");
            Collider[] colliders = owner.GetComponents<Collider>();
            if (targets.arraySize != colliders.Length) { targets.arraySize = colliders.Length; dirty = true; }
            for (int i = 0; i < colliders.Length; i++)
            {
                var entry = targets.GetArrayElementAtIndex(i);
                if (entry.objectReferenceValue == colliders[i]) continue;
                entry.objectReferenceValue = colliders[i];
                dirty = true;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert(binding != null && expected != null, "攻撃対象の参照を設定できません: " + owner.name);
        }
        return dirty;
    }

    /// <summary>隔離プロジェクトで資産移行とプレイ中の回帰検証を開始する</summary>
    public static void RunBatch()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("バッチ検証は隔離プロジェクトで実行してください");
        try
        {
            MigrateAssets();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(BatchKey, true);
            EditorApplication.EnterPlaymode();
        }
        catch (Exception exception) { Finish(exception); }
    }

    /// <summary>プレイ開始後に実際の初期化とプール処理を検証する</summary>
    private static void HandlePlayState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(BatchKey, false))
            RunTests();
    }

    /// <summary>登録と命中と活動期間の切り替えを実際の個体で確認する</summary>
    private static void RunTests()
    {
        SessionState.SetBool(BatchKey, false);
        try
        {
            checks.Clear();
            var poolObject = new GameObject("検証プール");
            poolObject.AddComponent<LifetimeRunner>();
            poolObject.AddComponent<EnemyBrainRunner>();
            poolObject.AddComponent<BoostFireCullerRunner>();
            poolObject.AddComponent<DamagePopupRunner>();
            PoolManager pool = poolObject.AddComponent<PoolManager>();
            var source = new GameObject("検証対象Prefab");
            source.SetActive(false);
            source.AddComponent<BoxCollider>();
            source.AddComponent<SphereCollider>();
            source.AddComponent<CombatDamageProbe>();
            var child = new GameObject("対象未設定の子Collider");
            child.transform.SetParent(source.transform, false);
            child.AddComponent<BoxCollider>();

            PooledInstance first = pool.Rent(source);
            CombatDamageProbe probe = first.Poolable as CombatDamageProbe;
            Collider collider = first.GameObject.GetComponent<BoxCollider>();
            Assert(DamageTargetRegistry.TryResolve(collider, out IDamageable resolved) && ReferenceEquals(resolved, probe),
                "登録表が命中対象を解決できません");
            Assert(DamageTargetRegistry.TryResolve(first.GameObject.GetComponent<SphereCollider>(), out resolved) && ReferenceEquals(resolved, probe),
                "複数Colliderの対応が異なります");
            Collider childCollider = first.Transform.GetChild(0).GetComponent<Collider>();
            Assert(!DamageTargetRegistry.TryResolve(childCollider, out _), "親への対象拡張が発生しました");
            Assert(first.Movement == null, "存在しない部品が解決されました");
            Record("同一GameObjectの対象、複数Collider、親への判定拡張なし、任意部品の欠落");

            var projectileSource = new GameObject("検証攻撃Prefab");
            projectileSource.SetActive(false);
            projectileSource.AddComponent<SphereCollider>();
            var damageSource = projectileSource.AddComponent<DamageObject>();
            SetValue(damageSource, "targetTeam", (int)TeamType.Enemy);
            PooledInstance projectile = pool.Rent(projectileSource);
            DamageObject damage = projectile.RequireDamage() as DamageObject;
            damage.Initialize(11, 0f);
            damage.SendMessage("OnTriggerEnter", collider);
            Assert(probe.ReceivedDamage == 11 && !projectile.GameObject.activeSelf, "実体弾の命中または返却が不正です");
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Assert(!DamageTargetRegistry.TryResolve(wall.GetComponent<Collider>(), out _), "壁がダメージ対象として登録されました");
            Record("登録済みの対象への実体弾ダメージ、弾の返却、壁の区別");

            probe.Release();
            Assert(!DamageTargetRegistry.TryResolve(collider, out _), "返却済みの対象が登録に残っています");
            PooledInstance second = pool.Rent(source);
            Assert(ReferenceEquals(first, second) && ReferenceEquals(second.Poolable, probe),
                "プール再利用で部品参照が作り直されました");
            Assert(probe.GetCount == 2 && probe.ReleaseCount == 1 && probe.ReceivedDamage == 0,
                "プールの活動期間またはコールバックが不正です");
            Assert(DamageTargetRegistry.TryResolve(collider, out _), "再取得後の対象が登録されません");
            Record("プール個体とキャッシュの再利用、返却時の解除、再取得時の再登録");

            var areaSource = new GameObject("検証範囲Prefab");
            areaSource.SetActive(false);
            areaSource.AddComponent<SphereCollider>();
            var sourceArea = areaSource.AddComponent<DamageArea>();
            SetValue(sourceArea, "targetTeam", (int)TeamType.Enemy);
            SetValue(sourceArea, "mode", (int)DamageAreaMode.Interval);
            PooledInstance areaInstance = pool.Rent(areaSource);
            var area = areaInstance.RequireDamage() as DamageArea;
            area.SendMessage("OnTriggerStay", collider);
            Assert(TimerCount(area) == 1, "範囲攻撃の接触時間が記録されません");
            probe.Release();
            Assert(TimerCount(area) == 0, "返却された対象の接触時間が残っています");
            area.Release();
            Record("範囲攻撃の対象返却時の接触時間と購読解除");

            var generatorSource = new GameObject("検証爆発生成Prefab");
            generatorSource.SetActive(false);
            var sourceGenerator = generatorSource.AddComponent<DamageGenerator>();
            var generatorSettings = new SerializedObject(sourceGenerator);
            var explosions = generatorSettings.FindProperty("explosionPrefabs");
            explosions.arraySize = 1;
            explosions.GetArrayElementAtIndex(0).objectReferenceValue = areaSource;
            generatorSettings.ApplyModifiedPropertiesWithoutUndo();
            int originalDamage = DamageValue(sourceArea);
            var generator = pool.Rent(generatorSource).GameObject.GetComponent<DamageGenerator>();
            generator.Initialize(31, 2f);
            generator.SendMessage("Explode");
            Assert(areaInstance.GameObject.activeSelf && DamageValue(area) == 31, "爆発個体へダメージが渡りません");
            Assert(DamageValue(sourceArea) == originalDamage, "共有Prefabのダメージが変更されました");
            area.Release();
            generator = pool.Rent(generatorSource).GameObject.GetComponent<DamageGenerator>();
            generator.Initialize(53, 4f);
            generator.SendMessage("Explode");
            Assert(DamageValue(area) == 53 && DamageValue(sourceArea) == originalDamage,
                "爆発再利用時に前回のダメージが混在しました");
            Record("共有Prefabを変更しない爆発、異なるダメージでの再利用");
            TestAttackModes(pool, source, projectileSource);
            TestContactStateReuse(pool, source);
            TestEnemyPrefabs(pool);
            Finish(null);
        }
        catch (Exception exception) { Finish(exception); }
    }

    /// <summary>実際の敵とアイテムボックスの参照が再利用されることを確認する</summary>
    private static void TestEnemyPrefabs(PoolManager pool)
    {
        foreach (string path in new[]
        {
            "Assets/Datas/enemyDatas/TestEnemy/Gatling_01.prefab",
            "Assets/spownobject/アイテムボックス.prefab"
        })
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            PooledInstance instance = pool.Rent(source);
            Enemy enemy = instance.Enemy;
            Rigidbody body = instance.Body;
            Assert(PlayerContactTarget.TryResolve(body, out PlayerContactTarget contact), "敵の接触対象が登録されません: " + path);
            bool isBox = path.Contains("アイテムボックス");
            Assert(contact.CanBePushed != isBox, "敵とアイテムボックスの押し出し設定が異なります");
            Assert(instance.GameObject.GetComponent<Collider>().gameObject.layer == LayerMask.NameToLayer(isBox ? "PlayerObstacle" : "Enemy"),
                "敵とアイテムボックスのレイヤー設定が不正です");
            Assert(DamageTargetRegistry.TryResolve(instance.GameObject.GetComponent<Collider>(), out IDamageable target) && ReferenceEquals(target, enemy),
                "実際の敵Prefabへ命中できません");
            enemy.Release();
            Assert(!PlayerContactTarget.TryResolve(body, out _), "返却した敵の接触対象が残っています");
            PooledInstance reused = pool.Rent(source);
            Assert(ReferenceEquals(instance, reused) && PlayerContactTarget.TryResolve(body, out var next) && ReferenceEquals(contact, next),
                "再取得時に敵の接触参照が作り直されました");
            enemy.Release();
        }
        Record("実際の敵とアイテムボックスのレイヤー、押し出し例外、命中と接触参照の再利用");
    }

    /// <summary>実際の射線と所有者の能力を使って各攻撃方式を確認する</summary>
    private static void TestAttackModes(PoolManager pool, GameObject targetSource, GameObject projectileSource)
    {
        var createPlayer = typeof(PlayerSettingsValidation).GetMethod("CreatePlayer", BindingFlags.Static | BindingFlags.NonPublic);
        var owner = (Player)createPlayer.Invoke(null, new object[] { "検証射撃プレイヤー", new Vector3(100f, 10f, 100f) });
        var aim = new GameObject("検証照準").transform;
        aim.position = new Vector3(100f, 10f, 102f);
        PooledInstance first = pool.Rent(targetSource);
        PooledInstance second = pool.Rent(targetSource);
        first.Transform.position = new Vector3(100f, 10f, 106f);
        second.Transform.position = new Vector3(103f, 10f, 106f);
        // 複数Colliderの登録は別の検証で確認し射線の検証では一つの形状に限定する
        foreach (var instance in new[] { first, second })
        {
            instance.GameObject.GetComponent<SphereCollider>().enabled = false;
            instance.Transform.GetChild(0).GetComponent<Collider>().enabled = false;
        }
        var weaponRoot = new GameObject("検証武器");
        weaponRoot.SetActive(false);
        var weapon = weaponRoot.AddComponent<Weapon>();
        var view = weaponRoot.AddComponent<CombatWeaponViewProbe>();
        var sound = weaponRoot.AddComponent<Sounder>();
        var weaponReferences = new SerializedObject(weapon);
        WeaponData data = Object.Instantiate(AssetDatabase.LoadAssetAtPath<WeaponData>(
            "Assets/Datas/WeaponDatas/Main/AssaultRifle/AssaultRifle.asset"));
        weaponReferences.FindProperty("weaponData").objectReferenceValue = data;
        weaponReferences.FindProperty("weaponOwner").objectReferenceValue = owner;
        weaponReferences.FindProperty("weaponView").objectReferenceValue = view;
        weaponReferences.FindProperty("sounder").objectReferenceValue = sound;
        weaponReferences.FindProperty("aimTransform").objectReferenceValue = aim;
        weaponReferences.ApplyModifiedPropertiesWithoutUndo();
        weaponRoot.SetActive(true);
        int expected = weapon.GetDamageAmount();
        var ignored = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ignored.transform.position = new Vector3(100f, 10f, 103f);
        Collider ignoredCollider = ignored.GetComponent<Collider>();
        ignoredCollider.isTrigger = true;
        AttackCollisionFilter.Register(ignoredCollider);
        Physics.SyncTransforms();
        var hitScan = ScriptableObject.CreateInstance<HitScanData>();
        hitScan.Fire(weapon, owner);
        var firstProbe = first.Poolable as CombatDamageProbe;
        var secondProbe = second.Poolable as CombatDamageProbe;
        Assert(firstProbe.ReceivedDamage == expected, "ヒットスキャンが補助判定の先の敵に命中しません");
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position = new Vector3(100f, 10f, 104f);
        Physics.SyncTransforms();
        hitScan.Fire(weapon, owner);
        Assert(firstProbe.ReceivedDamage == expected, "壁の先にヒットスキャンが貫通しました");
        wall.SetActive(false);
        Physics.SyncTransforms();
        var chain = ScriptableObject.CreateInstance<ChainFire>();
        chain.Fire(weapon, owner);
        Assert(firstProbe.ReceivedDamage == expected * 2 && secondProbe.ReceivedDamage == expected,
            "登録済みの敵への連鎖攻撃が不正です");
        Record("実際のヒットスキャンと連鎖攻撃、補助判定の除外、壁による遮蔽");

        var auto = ScriptableObject.CreateInstance<AutoTargetFire>();
        SetObject(auto, "prefab", projectileSource);
        auto.Fire(weapon, owner);
        DamageObject fired = Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)
            .First(candidate => candidate.gameObject.name == projectileSource.name + "(Clone)");
        Assert(Vector3.Dot(fired.transform.forward, (first.Transform.position - aim.position).normalized) > 0.999f,
            "自動照準が保持した照準位置から最寄りの敵へ向きません");
        fired.Release();
        foreach (FireModeData mode in new FireModeData[]
        {
            ScriptableObject.CreateInstance<NormalProjectileFire>(), ScriptableObject.CreateInstance<SurroundedFire>(),
            ScriptableObject.CreateInstance<StepFire>(), ScriptableObject.CreateInstance<AreaFireData>()
        })
        {
            SetObject(mode, "prefab", projectileSource);
            mode.Fire(weapon, owner);
            DamageObject[] active = Object.FindObjectsByType<DamageObject>(FindObjectsSortMode.None)
                .Where(candidate => candidate.gameObject.name == projectileSource.name + "(Clone)").ToArray();
            Assert(active.Length == (mode is SurroundedFire ? 8 : 1), "攻撃個体の生成数が異なります: " + mode.GetType().Name);
            Assert(active.All(candidate => DamageValue(candidate) == expected), "攻撃個体へ所有者のダメージが渡りません");
            foreach (DamageObject candidate in active) candidate.Release();
        }
        Record("自動照準と通常弾と周囲攻撃と移動攻撃と上空攻撃の生成とダメージ設定");

        var trackingSource = new GameObject("検証追尾Prefab");
        trackingSource.SetActive(false);
        trackingSource.AddComponent<SphereCollider>();
        var trackingDamage = trackingSource.AddComponent<DamageObject>();
        SetValue(trackingDamage, "targetTeam", (int)TeamType.Enemy);
        trackingSource.AddComponent<TrackingMovement>();
        PooledInstance trackingInstance = pool.Rent(trackingSource);
        trackingInstance.Transform.position = aim.position;
        var tracking = trackingInstance.Movement as TrackingMovement;
        tracking.Initialize(owner.transform, Vector3.forward);
        Physics.SyncTransforms();
        tracking.SendMessage("SearchTarget");
        FieldInfo targetField = typeof(TrackingMovement).GetField("target", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert((Transform)targetField.GetValue(tracking) == first.Transform, "追尾対象が最寄りの登録済み敵ではありません");
        firstProbe.Release();
        Physics.SyncTransforms();
        tracking.SendMessage("SearchTarget");
        Assert((Transform)targetField.GetValue(tracking) == second.Transform, "返却された追尾対象を参照し続けています");
        Record("追尾対象の選択と返却された個体からの切り替え");
    }

    /// <summary>複数Colliderの時間加算と接触状態の再利用および再入を検証する</summary>
    private static void TestContactStateReuse(PoolManager pool, GameObject targetPrefab)
    {
        var first = pool.Rent(targetPrefab);
        var second = pool.Rent(targetPrefab);
        var firstProbe = first.GameObject.GetComponent<CombatDamageProbe>();
        var secondProbe = second.GameObject.GetComponent<CombatDamageProbe>();
        Collider[] colliders = first.GameObject.GetComponents<Collider>();
        foreach (Collider collider in colliders) collider.enabled = true;
        Collider otherCollider = second.GameObject.GetComponent<Collider>();
        otherCollider.enabled = true;
        var source = new GameObject("接触再利用検証Prefab");
        source.SetActive(false);
        source.AddComponent<SphereCollider>();
        var prefabArea = source.AddComponent<DamageArea>();
        SetValue(prefabArea, "mode", (int)DamageAreaMode.Interval);
        SetValue(prefabArea, "targetTeam", (int)TeamType.Enemy);
        var area = pool.Rent(source).GameObject.GetComponent<DamageArea>();
        var stay = (Action<Collider>)Delegate.CreateDelegate(typeof(Action<Collider>), area,
            typeof(DamageArea).GetMethod("OnTriggerStay", BindingFlags.Instance | BindingFlags.NonPublic));
        var exit = (Action<Collider>)Delegate.CreateDelegate(typeof(Action<Collider>), area,
            typeof(DamageArea).GetMethod("OnTriggerExit", BindingFlags.Instance | BindingFlags.NonPublic));
        var settings = new SerializedObject(area);
        settings.FindProperty("interval").floatValue = float.MaxValue;
        settings.ApplyModifiedPropertiesWithoutUndo();
        stay(colliders[0]);
        object original = AreaContacts(area)[firstProbe];
        float firstTime = ContactTimer(original);
        stay(colliders[1]);
        Assert(Mathf.Approximately(ContactTimer(original), firstTime * 2f), "Colliderごとの時間加算が変わりました");
        exit(colliders[0]);
        Assert(TimerCount(area) == 0, "1つのColliderの退出で時間がリセットされません");
        stay(colliders[1]);
        Assert(ReferenceEquals(original, AreaContacts(area)[firstProbe]), "接触状態が再利用されません");
        firstProbe.Release();
        Assert(TimerCount(area) == 0, "対象の返却で接触状態が解除されません");
        first = pool.Rent(targetPrefab);
        colliders = first.GameObject.GetComponents<Collider>();
        Assert(firstTime > 0f, "物理時間が進んでいないため接触時間を検証できません");
        settings.Update();
        settings.FindProperty("interval").floatValue = 0f;
        settings.ApplyModifiedPropertiesWithoutUndo();
        firstProbe.OnDamageApplied = () =>
        {
            firstProbe.OnDamageApplied = null;
            firstProbe.Release();
            settings.Update();
            settings.FindProperty("interval").floatValue = float.MaxValue;
            settings.ApplyModifiedPropertiesWithoutUndo();
            stay(otherCollider);
        };
        stay(colliders[0]);
        Assert(TimerCount(area) == 1 && ContactTimer(AreaContacts(area)[secondProbe]) > 0f,
            "ダメージ適用中の返却と状態再利用で次の接触時間が上書きされました");
        area.gameObject.SetActive(false);
        Assert(TimerCount(area) == 0, "範囲の無効化で接触状態が残りました");
        area.Release();
        area = pool.Rent(source).GameObject.GetComponent<DamageArea>();
        Assert(TimerCount(area) == 0, "範囲の再取得に前回の接触状態が残りました");
        area.Release();
        secondProbe.Release();
        Record("接触状態の再利用、複数Colliderの既存タイミング、ダメージ適用中の返却と再入");
    }

    /// <summary>範囲が保持している対象別の接触状態を取得する</summary>
    private static System.Collections.IDictionary AreaContacts(DamageArea area) =>
        (System.Collections.IDictionary)typeof(DamageArea).GetField("contacts", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(area);

    /// <summary>検証用に接触状態の経過時間を読み取る</summary>
    private static float ContactTimer(object contact) => (float)contact.GetType().GetField("Timer").GetValue(contact);

    /// <summary>検証用の部品とPrefabの参照を設定する</summary>
    private static void SetObject(Object target, string field, Object value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(field).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>検証用の数値設定を対象個体へ反映する</summary>
    private static void SetValue(Object target, string field, int value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(field).intValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>範囲攻撃に保持された対象数を確認する</summary>
    private static int TimerCount(DamageArea area) =>
        ((System.Collections.IDictionary)typeof(DamageArea).GetField("contacts", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(area)).Count;

    /// <summary>爆発元と生成した個体のダメージを比較する</summary>
    private static int DamageValue(DamageBase target) =>
        (int)typeof(DamageBase).GetField("damage", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);

    /// <summary>検証条件を満たさない場合に理由を通知する</summary>
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    /// <summary>成功した検証項目を記録する</summary>
    private static void Record(string message)
    {
        checks.Add(message);
        Debug.Log("検証成功: " + message);
    }

    /// <summary>検証結果を保存してバッチ処理を終了する</summary>
    private static void Finish(Exception exception)
    {
        SessionState.SetBool(BatchKey, false);
        File.WriteAllText("CombatReferenceValidationResults.txt", string.Join("\n", checks) + "\n"
            + (exception == null ? "PASS" : exception.ToString()));
        if (exception != null) Debug.LogException(exception);
        EditorApplication.Exit(exception == null ? 0 : 1);
    }
}
