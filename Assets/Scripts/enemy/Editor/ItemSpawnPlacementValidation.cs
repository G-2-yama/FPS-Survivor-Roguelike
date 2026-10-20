using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>実際の箱サイズで、地面への配置と壁の重なり判定を検証する</summary>
public static class ItemSpawnPlacementValidation
{
    [MenuItem("Tools/Enemy/アイテムボックスの生成位置を検証")]
    public static void Validate()
    {
        var root = new GameObject("Item placement validation");
        Vector3 origin = new Vector3(10000f, 0f, 10000f);
        try
        {
            GameObject floor = CreateBox(root.transform, origin + Vector3.down * 0.5f, new Vector3(40f, 1f, 40f));
            var source = new GameObject("Item box source");
            source.transform.SetParent(root.transform);
            source.transform.position = origin + Vector3.right * 100f;
            source.transform.localScale = Vector3.one * 50f;
            BoxCollider box = source.AddComponent<BoxCollider>();
            box.size = new Vector3(0.03415826f, 0.03270815f, 0.049666725f);
            box.center = new Vector3(0f, 0f, 0.00000762008f);
            source.SetActive(false);
            Physics.SyncTransforms();

            Vector3 candidate = origin + Vector3.up * 1.5f;
            Assert(GroundedSpawnPlacement.TryGetPosition(source, candidate, out Vector3 position), "空いている床に配置できる");
            float halfHeight = box.size.y * 25f;
            Assert(Mathf.Abs(position.y - halfHeight - 0.05f) < 0.001f, "底面が床より5cm上になる");

            GameObject wall = CreateBox(root.transform, origin + new Vector3(0f, 1.5f, 1.1f), new Vector3(5f, 3f, 0.2f));
            Physics.SyncTransforms();
            Assert(!Physics.CheckSphere(candidate, 0.5f), "従来の小さい球では壁を見落とす配置を再現する");
            Assert(!GroundedSpawnPlacement.TryGetPosition(source, candidate, out _), "本体の端だけが壁に重なる場合も拒否する");

            wall.GetComponent<BoxCollider>().isTrigger = true;
            Physics.SyncTransforms();
            Assert(GroundedSpawnPlacement.TryGetPosition(source, candidate, out position), "Trigger領域は配置を妨げない");
            wall.SetActive(false);

            source.transform.position = position;
            source.SetActive(true);
            Physics.SyncTransforms();
            Assert(GroundedSpawnPlacement.TryGetPosition(source, candidate, out _), "再配置判定で自身のColliderを除外する");
            source.SetActive(false);

            CreateBox(root.transform, position + Vector3.right * 1.1f, new Vector3(1.7f, 1.6f, 2.5f));
            Physics.SyncTransforms();
            Assert(!GroundedSpawnPlacement.TryGetPosition(source, candidate, out _), "他の箱との重なりを拒否する");

            floor.SetActive(false);
            Physics.SyncTransforms();
            Assert(!GroundedSpawnPlacement.TryGetPosition(source, origin + Vector3.right * 15f + Vector3.up, out _), "地面がない場所に生成しない");
            Debug.Log("ItemSpawnPlacementValidation: PASS (8 checks)");
        }
        finally
        {
            Object.DestroyImmediate(root);
            Physics.SyncTransforms();
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

    private static GameObject CreateBox(Transform parent, Vector3 position, Vector3 size)
    {
        var obj = new GameObject("Validation solid");
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.AddComponent<BoxCollider>().size = size;
        return obj;
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
