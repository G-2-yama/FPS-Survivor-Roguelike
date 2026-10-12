using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.UI;

public class MinimapManager : MonoBehaviour
{
    [System.Serializable]
    public class MinimapSource
    {
        public EnemyGenerator generator;
        public GameObject iconPrefab;
    }

    private class SourceIcons
    {
        public MinimapIconMesh iconMesh;
        public TransformAccessArray transforms;
        public readonly List<GameObject> targets = new();
        public readonly List<int> seenStamps = new();
        public readonly Dictionary<GameObject, int> indices = new();
    }

    [SerializeField] private List<MinimapSource> sources = new();

    [SerializeField] private Transform player;
    [SerializeField] private RectTransform iconParent;
    [SerializeField] private RectTransform playerIcon;

    [SerializeField] private float displayRange = 20f;
    [SerializeField] private float minimapRadius = 100f;


    private SourceIcons[] sourceIcons;

    private int currentStamp;


    private void Start()
    {
        sourceIcons = new SourceIcons[sources.Count];

        for (int i = 0; i < sources.Count; i++)
        {
            if (sources[i].iconPrefab != null)
            {
                sourceIcons[i] = new SourceIcons
                {
                    iconMesh = CreateIconMesh(sources[i].iconPrefab),
                    transforms = new TransformAccessArray(64)
                };
            }
        }
    }


    private void OnDestroy()
    {
        if (sourceIcons == null)
        {
            return;
        }

        foreach (SourceIcons icons in sourceIcons)
        {
            if (icons != null && icons.transforms.isCreated)
            {
                icons.transforms.Dispose();
            }
        }
    }


    private void Update()
    {
        currentStamp++;

        UpdateTargetIcons();
        UpdatePlayerIcon();
    }


    private void UpdateTargetIcons()
    {
        Vector3 playerPosition = player.position;

        for (int s = 0; s < sources.Count; s++)
        {
            SourceIcons icons = sourceIcons[s];

            if (icons == null)
            {
                continue;
            }

            CollectTargets(icons, sources[s].generator);
            RemoveUnseenTargets(icons);

            MinimapIconMesh iconMesh = icons.iconMesh;

            IconVertexJob job = new IconVertexJob
            {
                playerPosition = playerPosition,
                displayRange = displayRange,
                minimapRadius = minimapRadius,
                cornerMin = iconMesh.CornerMin,
                cornerMax = iconMesh.CornerMax,
                maxIcons = MinimapIconMesh.MaxIcons,
                vertices = iconMesh.PrepareVertices(icons.transforms.length)
            };

            job.ScheduleReadOnly(icons.transforms, 64).Complete();

            iconMesh.ApplyIcons();
        }
    }


    private void CollectTargets(SourceIcons icons, EnemyGenerator generator)
    {
        if (generator == null)
        {
            return;
        }

        IReadOnlyList<GameObject> targets = generator.ActiveEnemies;

        for (int i = 0; i < targets.Count; i++)
        {
            GameObject target = targets[i];

            if (target == null)
            {
                continue;
            }

            if (icons.indices.TryGetValue(target, out int index))
            {
                icons.seenStamps[index] = currentStamp;
                continue;
            }

            icons.indices.Add(target, icons.targets.Count);
            icons.targets.Add(target);
            icons.seenStamps.Add(currentStamp);
            icons.transforms.Add(target.transform);
        }
    }


    private void RemoveUnseenTargets(SourceIcons icons)
    {
        for (int i = icons.targets.Count - 1; i >= 0; i--)
        {
            if (icons.seenStamps[i] == currentStamp)
            {
                continue;
            }

            int last = icons.targets.Count - 1;

            icons.indices.Remove(icons.targets[i]);
            icons.transforms.RemoveAtSwapBack(i);

            if (i != last)
            {
                icons.targets[i] = icons.targets[last];
                icons.seenStamps[i] = icons.seenStamps[last];
                icons.indices[icons.targets[i]] = i;
            }

            icons.targets.RemoveAt(last);
            icons.seenStamps.RemoveAt(last);
        }
    }


    private MinimapIconMesh CreateIconMesh(GameObject prefab)
    {
        RectTransform prefabRect = prefab.GetComponent<RectTransform>();

        GameObject meshObj = new GameObject(prefab.name, typeof(RectTransform));
        meshObj.layer = iconParent.gameObject.layer;

        RectTransform meshRect = (RectTransform)meshObj.transform;
        meshRect.SetParent(iconParent, false);
        meshRect.anchorMin = prefabRect.anchorMin;
        meshRect.anchorMax = prefabRect.anchorMax;
        meshRect.anchoredPosition = Vector2.zero;
        meshRect.sizeDelta = Vector2.zero;

        MinimapIconMesh iconMesh = meshObj.AddComponent<MinimapIconMesh>();
        iconMesh.Setup(prefab.GetComponent<Image>(), prefabRect);

        return iconMesh;
    }


    [BurstCompile]
    private struct IconVertexJob : IJobParallelForTransform
    {
        public Vector3 playerPosition;
        public float displayRange;
        public float minimapRadius;
        public Vector2 cornerMin;
        public Vector2 cornerMax;
        public int maxIcons;

        [WriteOnly, NativeDisableParallelForRestriction]
        public NativeArray<Vector3> vertices;

        /// <summary>
        /// Iconのミニマップ上の位置を更新
        /// </summary>
        public void Execute(int index, TransformAccess transform)
        {
            if (index >= maxIcons)
            {
                return;
            }

            Vector3 position = transform.position;

            float x = (position.x - playerPosition.x) / displayRange * minimapRadius;
            float y = (position.z - playerPosition.z) / displayRange * minimapRadius;

            float sqrMagnitude = x * x + y * y;

            if (sqrMagnitude > minimapRadius * minimapRadius)
            {
                float magnitude = math.sqrt(sqrMagnitude);

                x = x / magnitude * minimapRadius;
                y = y / magnitude * minimapRadius;
            }


            // アイコン上方向をPlayerへ向ける
            float cos = 1f;
            float sin = 0f;

            float sqrDistance = x * x + y * y;

            if (sqrDistance > 0.001f)
            {
                float distance = math.sqrt(sqrDistance);

                cos = -y / distance;
                sin = x / distance;
            }

            int v = index * 4;

            vertices[v] = Corner(x, y, cos, sin, cornerMin.x, cornerMin.y);
            vertices[v + 1] = Corner(x, y, cos, sin, cornerMin.x, cornerMax.y);
            vertices[v + 2] = Corner(x, y, cos, sin, cornerMax.x, cornerMax.y);
            vertices[v + 3] = Corner(x, y, cos, sin, cornerMax.x, cornerMin.y);
        }


        private static Vector3 Corner(float x, float y, float cos, float sin, float cornerX, float cornerY)
        {
            return new Vector3(
                x + cornerX * cos - cornerY * sin,
                y + cornerX * sin + cornerY * cos,
                0f
            );
        }
    }



    /// <summary>
    /// PlayerIconだけ実際のPlayer方向に合わせる
    /// </summary>
    private void UpdatePlayerIcon()
    {
        Vector3 forward = player.forward;

        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            return;

        forward.Normalize();

        float angle =
            Vector3.SignedAngle(
                Vector3.forward,
                forward,
                Vector3.up
            );

        playerIcon.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                -angle
            );
    }
}