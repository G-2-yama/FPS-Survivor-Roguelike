using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>固定コンポーネントの配置と参照設定をEditor内で完了させる</summary>
public static class FixedComponentSetup
{
    /// <summary>既存PrefabとSceneの不足する構成を事前配置へ移行する</summary>
    [MenuItem("Tools/Combat/固定コンポーネントを事前配置")]
    public static void MigrateAssets()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("プレイを停止してから移行してください");
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        SceneSetup[] previous = EditorSceneManager.GetSceneManagerSetup();
        try
        {
            int changed = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("/_Recovery/")) continue;
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    bool dirty = ConfigureHierarchy(root);
                    ValidateHierarchy(root);
                    if (dirty)
                    {
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                        changed++;
                    }
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" }))
            {
                Scene scene = EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guid), OpenSceneMode.Single);
                bool dirty = false;
                foreach (GameObject root in scene.GetRootGameObjects()) dirty |= ConfigureHierarchy(root);
                dirty |= ConfigureSceneRunners(scene);
                ValidateScene(scene);
                if (dirty) { EditorSceneManager.SaveScene(scene); changed++; }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"固定コンポーネントを事前配置しました 変更資産数={changed}");
        }
        finally
        {
            if (!Application.isBatchMode) EditorSceneManager.RestoreSceneManagerSetup(previous);
        }
    }

    /// <summary>Inspector参照の未設定だけを補完し、既存の構成と値を保持する</summary>
    public static bool ConfigureHierarchy(GameObject root)
    {
        bool dirty = false;
        foreach (Enemy enemy in root.GetComponentsInChildren<Enemy>(true))
        {
            var target = enemy.GetComponent<PlayerContactTarget>();
            if (target == null) { target = enemy.gameObject.AddComponent<PlayerContactTarget>(); dirty = true; }
            dirty |= AssignMissing(enemy, "contactTarget", target);
            dirty |= AssignMissing(enemy, "contactDamageSource", enemy.GetComponentInChildren<ToutchDamageController>(true));
            dirty |= AssignMissing(enemy, "playerObstacle", enemy.GetComponent<PlayerObstacle>());
            var data = new SerializedObject(enemy);
            var colliders = data.FindProperty("layerColliders");
            if (colliders.arraySize == 0)
            {
                Collider[] entries = enemy.GetComponentsInChildren<Collider>(true);
                colliders.arraySize = entries.Length;
                for (int i = 0; i < entries.Length; i++)
                    colliders.GetArrayElementAtIndex(i).objectReferenceValue = entries[i];
                data.ApplyModifiedPropertiesWithoutUndo();
                dirty = true;
            }
        }
        foreach (PlayerCollisionController collision in root.GetComponentsInChildren<PlayerCollisionController>(true))
        {
            var data = new SerializedObject(collision);
            var zone = data.FindProperty("pushZone").objectReferenceValue as PlayerEnemyPushZone;
            if (zone == null) zone = collision.GetComponentInChildren<PlayerEnemyPushZone>(true);
            if (zone == null)
            {
                var child = new GameObject("EnemyPushZone");
                child.transform.SetParent(collision.transform, false);
                var collider = child.AddComponent<CapsuleCollider>();
                collider.isTrigger = true;
                zone = child.AddComponent<PlayerEnemyPushZone>();
                dirty = true;
            }
            dirty |= AssignMissing(collision, "pushZone", zone);
            dirty |= AssignMissing(collision, "pushZoneCollider", zone.GetComponent<CapsuleCollider>());
        }
        foreach (PlayerActionTrail trail in root.GetComponentsInChildren<PlayerActionTrail>(true))
        {
            var data = new SerializedObject(trail);
            var bindings = data.FindProperty("stateBindings");
            // 既存の軌跡を最初のアクションに再利用して見た目と位置を維持する
            var used = new HashSet<TrailRenderer>();
            for (int i = 0; i < bindings.arraySize; i++)
            {
                var renderer = bindings.GetArrayElementAtIndex(i).FindPropertyRelative("trailRenderer").objectReferenceValue as TrailRenderer;
                if (renderer != null) used.Add(renderer);
            }
            var primary = trail.GetComponent<TrailRenderer>();
            for (int i = 0; i < bindings.arraySize; i++)
            {
                var binding = bindings.GetArrayElementAtIndex(i);
                var reference = binding.FindPropertyRelative("trailRenderer");
                if (reference.objectReferenceValue != null) continue;
                TrailRenderer renderer;
                if (primary != null && used.Add(primary)) renderer = primary;
                else
                {
                    string action = binding.FindPropertyRelative("action").enumDisplayNames[binding.FindPropertyRelative("action").enumValueIndex];
                    var child = new GameObject("Player Trail - " + action);
                    child.transform.SetParent(trail.transform, false);
                    renderer = child.AddComponent<TrailRenderer>();
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    used.Add(renderer);
                }
                renderer.emitting = false;
                reference.objectReferenceValue = renderer;
                dirty = true;
            }
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        return dirty;
    }

    /// <summary>実際に戦闘を行うSceneへ共通の更新担当を配置する</summary>
    private static bool ConfigureSceneRunners(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        if (!roots.Any(root => root.GetComponentInChildren<PoolManager>(true) != null)) return false;
        Type[] types = { typeof(LifetimeRunner), typeof(EnemyBrainRunner), typeof(BoostFireCullerRunner), typeof(DamagePopupRunner) };
        GameObject container = null;
        bool dirty = false;
        foreach (Type type in types)
        {
            if (roots.Any(root => root.GetComponentInChildren(type, true) != null)) continue;
            if (container == null)
            {
                container = new GameObject("CombatRunners");
                SceneManager.MoveGameObjectToScene(container, scene);
            }
            container.AddComponent(type);
            dirty = true;
        }
        return dirty;
    }

    /// <summary>未設定のシリアライズ参照にだけEditorで取得した部品を割り当てる</summary>
    private static bool AssignMissing(UnityEngine.Object owner, string field, UnityEngine.Object value)
    {
        var data = new SerializedObject(owner);
        var property = data.FindProperty(field);
        if (property.objectReferenceValue != null || value == null) return false;
        property.objectReferenceValue = value;
        data.ApplyModifiedPropertiesWithoutUndo();
        return true;
    }

    /// <summary>現在のScene内にある必須参照と更新担当の構成を検査する</summary>
    [MenuItem("Tools/Combat/固定コンポーネントの設定を確認")]
    public static void ValidateOpenScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++) ValidateScene(SceneManager.GetSceneAt(i));
        Debug.Log("固定コンポーネントの設定に不足はありません");
    }

    /// <summary>階層内の参照が想定した所有者と部品に対応しているか検査する</summary>
    public static void ValidateHierarchy(GameObject root)
    {
        foreach (Enemy enemy in root.GetComponentsInChildren<Enemy>(true))
        {
            var target = new SerializedObject(enemy).FindProperty("contactTarget").objectReferenceValue as PlayerContactTarget;
            Require(target != null && target.gameObject == enemy.gameObject, enemy.name + ": Contact Targetを同じオブジェクトに設定してください");
        }
        foreach (PlayerCollisionController collision in root.GetComponentsInChildren<PlayerCollisionController>(true))
        {
            var data = new SerializedObject(collision);
            var zone = data.FindProperty("pushZone").objectReferenceValue as PlayerEnemyPushZone;
            var collider = data.FindProperty("pushZoneCollider").objectReferenceValue as CapsuleCollider;
            Require(zone != null && collider != null && zone.gameObject == collider.gameObject && zone.transform.IsChildOf(collision.transform),
                collision.name + ": EnemyPushZoneの子オブジェクトとCollider参照を設定してください");
        }
        foreach (PlayerActionTrail trail in root.GetComponentsInChildren<PlayerActionTrail>(true))
        {
            var bindings = new SerializedObject(trail).FindProperty("stateBindings");
            var renderers = new HashSet<UnityEngine.Object>();
            var actions = new HashSet<int>();
            for (int i = 0; i < bindings.arraySize; i++)
            {
                var binding = bindings.GetArrayElementAtIndex(i);
                var renderer = binding.FindPropertyRelative("trailRenderer").objectReferenceValue as TrailRenderer;
                Require(renderer != null && (renderer.transform == trail.transform || renderer.transform.IsChildOf(trail.transform)) && renderers.Add(renderer)
                    && actions.Add(binding.FindPropertyRelative("action").intValue), trail.name + ": 各アクションに異なる専用Trail Rendererを設定してください");
            }
        }
    }

    /// <summary>Scene内の構成と戦闘用更新担当の配置数を検査する</summary>
    public static void ValidateScene(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        foreach (GameObject root in roots) ValidateHierarchy(root);
        if (!roots.Any(root => root.GetComponentInChildren<PoolManager>(true) != null)) return;
        Type[] types = { typeof(LifetimeRunner), typeof(EnemyBrainRunner), typeof(BoostFireCullerRunner), typeof(DamagePopupRunner) };
        foreach (Type type in types)
        {
            Component[] components = roots.SelectMany(root => root.GetComponentsInChildren(type, true)).ToArray();
            Require(components.Length == 1 && components[0].gameObject.activeInHierarchy && ((Behaviour)components[0]).enabled,
                scene.path + ": 有効な" + type.Name + "を1つ配置してください");
        }
    }

    /// <summary>構成が不正な場合は具体的な修正箇所を示して停止する</summary>
    private static void Require(bool valid, string message)
    {
        if (!valid) throw new BuildFailedException(message);
    }
}

/// <summary>ビルド前のPrefabと各Sceneの処理時に必須構成の不足を検出する</summary>
public sealed class FixedComponentBuildValidation : IPreprocessBuildWithReport, IProcessSceneWithReport
{
    public int callbackOrder => 0;

    /// <summary>Prefabの必須参照をビルド前に検査する</summary>
    public void OnPreprocessBuild(BuildReport report)
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("/_Recovery/")) continue;
            FixedComponentSetup.ValidateHierarchy(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        }
    }

    /// <summary>ビルドに含まれるSceneの参照と更新担当を検査する</summary>
    public void OnProcessScene(Scene scene, BuildReport report) => FixedComponentSetup.ValidateScene(scene);
}
