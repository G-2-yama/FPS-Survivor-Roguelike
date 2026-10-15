using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>developの演出と足元生成をダメージ登録およびプール再利用と合わせて検証する</summary>
[InitializeOnLoad]
public static class MergeCompatibilityValidation
{
    private const string PendingKey = "MergeCompatibilityValidation.Pending";
    private static int checks;

    /// <summary>プレイ開始後に実際の初期化順序で検証を実行する</summary>
    static MergeCompatibilityValidation() => EditorApplication.update += CheckPending;

    /// <summary>隔離したUnityの空Sceneでマージ後の統合検証を開始する</summary>
    public static void RunBatch()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("隔離したバッチ環境で実行してください");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetBool(PendingKey, true);
        EditorApplication.EnterPlaymode();
    }

    /// <summary>Sceneの構成を変更せずに専用個体で演出と生成位置を確認する</summary>
    private static void CheckPending()
    {
        if (!EditorApplication.isPlaying || !SessionState.GetBool(PendingKey, false)) return;
        SessionState.SetBool(PendingKey, false);
        try
        {
            checks = 0;
            var manager = new GameObject("マージ検証プール");
            manager.AddComponent<LifetimeRunner>();
            PoolManager pool = manager.AddComponent<PoolManager>();
            TestStepFire(pool);
            TestChargeBeam(pool, "Assets/Datas/WeaponDatas/Ability/ChargeBeam/00_ChargeBeam.prefab");
            TestChargeBeam(pool, "Assets/Datas/WeaponDatas/Ability/ChargeBeam/EvoChargeBeam.prefab");
            TestExplosion(pool, "Assets/Datas/WeaponDatas/Ability/Grenade/01_GranadeDamageArea.prefab");
            TestExplosion(pool, "Assets/Prehab/Ammo/DamageArea.prefab");
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "MergeCompatibilityResults.txt"), $"PASS {checks} checks");
            Debug.Log($"developとの統合検証に成功しました {checks}項目");
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "MergeCompatibilityResults.txt"), error.ToString());
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
    }

    /// <summary>本体の下端から生成され、姿勢変更と参照がない場合の位置に追従することを確認する</summary>
    private static void TestStepFire(PoolManager pool)
    {
        var create = typeof(PlayerSettingsValidation).GetMethod("CreatePlayer", BindingFlags.Static | BindingFlags.NonPublic);
        var player = (Player)create.Invoke(null, new object[] { "足元生成検証", new Vector3(100f, 10f, 100f) });
        var body = player.GetComponent<CharacterController>();
        var root = new GameObject("足元生成検証武器");
        var aim = new GameObject("足元と区別する照準位置");
        aim.transform.position = player.transform.position + Vector3.up * 2f;
        root.SetActive(false);
        var weapon = root.AddComponent<Weapon>();
        var data = Object.Instantiate(AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Datas/WeaponDatas/Main/AssaultRifle/AssaultRifle.asset"));
        SetReference(weapon, "weaponData", data);
        SetReference(weapon, "weaponOwner", player);
        SetReference(weapon, "weaponView", root.AddComponent<CombatWeaponViewProbe>());
        SetReference(weapon, "sounder", root.AddComponent<Sounder>());
        SetReference(weapon, "aimTransform", aim.transform);
        root.SetActive(true);
        foreach (string path in new[]
        {
            "Assets/Datas/WeaponDatas/Auto/StepFire/00_StepFireF.asset",
            "Assets/Datas/WeaponDatas/Auto/StepFire/EvoStepFireF.asset"
        })
        {
            var step = Object.Instantiate(AssetDatabase.LoadAssetAtPath<StepFire>(path));
            var settings = new SerializedObject(step);
            var prefab = settings.FindProperty("prefab").objectReferenceValue as GameObject;
            Assert(settings.FindProperty("Offset").vector3Value == Vector3.zero, "developの足元用Offsetが変わりました");
            settings.FindProperty("stepInterval").floatValue = 0f;
            settings.ApplyModifiedPropertiesWithoutUndo();
            for (int posture = 0; posture < 2; posture++)
            {
                body.height = posture == 0 ? 2f : 1f;
                body.center = Vector3.zero;
                Physics.SyncTransforms();
                Vector3 expected = player.transform.position;
                expected.y = body.bounds.min.y;
                Assert((player.FootPosition - expected).sqrMagnitude < 0.000001f, "姿勢変更後の足元位置が本体下端と一致しません");
                step.Fire(weapon, player);
                var spawned = Array.Find(Object.FindObjectsByType<DamageArea>(FindObjectsSortMode.None),
                    area => area.gameObject.name == prefab.name + "(Clone)");
                Assert(spawned != null && (spawned.transform.position - expected).sqrMagnitude < 0.000001f,
                    "StepFireがカメラ位置から生成されています");
                spawned.Release();
            }
            Object.DestroyImmediate(step);
        }
        typeof(Player).GetMethod("SetBodyController", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(player, new object[] { null });
        Assert(player.FootPosition == player.transform.position, "本体参照がない場合の生成位置が変わりました");
        Object.DestroyImmediate(root);
        Object.DestroyImmediate(aim);
        Object.DestroyImmediate(player.gameObject);
        Object.DestroyImmediate(data);
    }

    /// <summary>太さ演出とダメージ登録が共存し、再取得時に発射直後の太さへ戻ることを確認する</summary>
    private static void TestChargeBeam(PoolManager pool, string path)
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var originalLines = source.GetComponentsInChildren<LineRenderer>(true);
        var first = pool.Rent(source);
        var width = first.GameObject.GetComponent<ChargeBeamWidth>();
        var lines = first.GameObject.GetComponentsInChildren<LineRenderer>(true);
        var settings = new SerializedObject(width);
        float burst = settings.FindProperty("burstScale").floatValue;
        Assert(width != null && lines.Length > 0 && lines.Length == originalLines.Length, "ChargeBeamの演出部品が欠落しました");
        for (int i = 0; i < lines.Length; i++)
            Assert(Mathf.Approximately(lines[i].widthMultiplier, originalLines[i].widthMultiplier * burst), "発射直後の太さが変わりました");
        ValidateDamageRegistration(first);
        typeof(ChargeBeamWidth).GetField("elapsed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(width, 1f);
        typeof(ChargeBeamWidth).GetMethod("ApplyScale", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(width, new object[] { 0.25f });
        first.Poolable.Release();
        var second = pool.Rent(source);
        Assert(ReferenceEquals(first, second), "ChargeBeamのプール個体が再利用されません");
        for (int i = 0; i < lines.Length; i++)
            Assert(Mathf.Approximately(lines[i].widthMultiplier, originalLines[i].widthMultiplier * burst), "再取得に前回の太さが残りました");
        ValidateDamageRegistration(second);
        second.Poolable.Release();
    }

    /// <summary>高さフェードの設定と位置の更新が登録部品と共存することを確認する</summary>
    private static void TestExplosion(PoolManager pool, string path)
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var first = pool.Rent(source);
        var fade = first.GameObject.GetComponent<ExplosionHeightFade>();
        var renderers = first.GameObject.GetComponentsInChildren<ParticleSystemRenderer>(true);
        Assert(fade != null && renderers.Length > 0, "爆発の高さフェード部品が欠落しました");
        var settings = new SerializedObject(fade);
        float height = settings.FindProperty("fadeHeight").floatValue;
        float opacity = settings.FindProperty("opacity").floatValue;
        var update = (Action)Delegate.CreateDelegate(typeof(Action), fade,
            typeof(ExplosionHeightFade).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic));
        var block = new MaterialPropertyBlock();
        for (int life = 0; life < 2; life++)
        {
            first.Transform.position = new Vector3(0f, 7f + life, 0f);
            update();
            foreach (var renderer in renderers)
            {
                renderer.GetPropertyBlock(block);
                Assert(Mathf.Approximately(block.GetFloat("_FadeBaseY"), 7f + life) &&
                    Mathf.Approximately(block.GetFloat("_FadeHeight"), height) &&
                    Mathf.Approximately(block.GetFloat("_Opacity"), opacity), "高さフェードの設定と位置が反映されません");
            }
            ValidateDamageRegistration(first);
            first.Poolable.Release();
            if (life == 0) Assert(ReferenceEquals(first, pool.Rent(source)), "爆発のプール個体が再利用されません");
        }
    }

    /// <summary>通常Colliderが同じGameObjectのダメージ対象に登録されていることを確認する</summary>
    private static void ValidateDamageRegistration(PooledInstance instance)
    {
        var collider = instance.GameObject.GetComponent<Collider>();
        Assert(DamageTargetRegistry.TryResolve(collider, out var target) && ReferenceEquals(target, instance.Damage),
            "演出部品の追加でダメージ登録が失われました");
    }

    /// <summary>検証用個体へ参照を割り当てる</summary>
    private static void SetReference(Object target, string name, Object reference)
    {
        var settings = new SerializedObject(target);
        settings.FindProperty(name).objectReferenceValue = reference;
        settings.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>満たさない検証条件を通知し、成功した確認数を記録する</summary>
    private static void Assert(bool valid, string message)
    {
        if (!valid) throw new InvalidOperationException(message);
        checks++;
    }
}
