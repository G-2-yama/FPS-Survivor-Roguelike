using System;
using UnityEditor;
using UnityEngine;

namespace BalanceSim.Editor
{
    public static class SerializedReader
    {
        public static SerializedProperty Prop(UnityEngine.Object target, string name)
        {
            SerializedProperty property = new SerializedObject(target).FindProperty(name);
            if (property == null)
            {
                throw new InvalidOperationException($"{target.GetType().Name}.{name} が見つかりません。フィールド名が変わった可能性があります（{target.name}）");
            }
            return property;
        }

        public static float Float(UnityEngine.Object target, string name) => Prop(target, name).floatValue;

        public static int Int(UnityEngine.Object target, string name) => Prop(target, name).intValue;

        public static T Ref<T>(UnityEngine.Object target, string name) where T : UnityEngine.Object
        {
            return Prop(target, name).objectReferenceValue as T;
        }

        public static Bounds LocalBounds(GameObject root, Func<Collider, bool> filter)
        {
            Transform rootTransform = root.transform;
            Quaternion inverseRotation = Quaternion.Inverse(rootTransform.rotation);
            bool hasBounds = false;
            var bounds = new Bounds();
            foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
            {
                if (!filter(collider) || !TryGetColliderLocalBounds(collider, out Bounds local))
                {
                    continue;
                }

                Vector3 min = local.min;
                Vector3 max = local.max;
                for (int i = 0; i < 8; i++)
                {
                    var corner = new Vector3((i & 1) == 0 ? min.x : max.x, (i & 2) == 0 ? min.y : max.y, (i & 4) == 0 ? min.z : max.z);
                    Vector3 world = collider.transform.TransformPoint(corner);
                    Vector3 point = inverseRotation * (world - rootTransform.position);
                    if (hasBounds)
                    {
                        bounds.Encapsulate(point);
                    }
                    else
                    {
                        bounds = new Bounds(point, Vector3.zero);
                        hasBounds = true;
                    }
                }
            }
            return bounds;
        }

        public static float HorizontalRadius(GameObject root, Func<Collider, bool> filter)
        {
            Bounds bounds = LocalBounds(root, filter);
            return (bounds.extents.x + bounds.extents.z) * 0.5f;
        }

        private static bool TryGetColliderLocalBounds(Collider collider, out Bounds bounds)
        {
            switch (collider)
            {
                case BoxCollider box:
                    bounds = new Bounds(box.center, box.size);
                    return true;
                case SphereCollider sphere:
                    bounds = new Bounds(sphere.center, Vector3.one * (sphere.radius * 2f));
                    return true;
                case CapsuleCollider capsule:
                {
                    var size = Vector3.one * (capsule.radius * 2f);
                    size[capsule.direction] = Mathf.Max(capsule.height, capsule.radius * 2f);
                    bounds = new Bounds(capsule.center, size);
                    return true;
                }
                case MeshCollider mesh when mesh.sharedMesh != null:
                    bounds = mesh.sharedMesh.bounds;
                    return true;
                default:
                    bounds = default;
                    return false;
            }
        }
    }
}
