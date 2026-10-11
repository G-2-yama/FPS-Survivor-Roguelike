using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapManager : MonoBehaviour
{
    [System.Serializable]
    public class MinimapSource
    {
        public EnemyGenerator generator;
        public GameObject iconPrefab;
    }

    [SerializeField] private List<MinimapSource> sources = new();

    [SerializeField] private Transform player;
    [SerializeField] private RectTransform iconParent;
    [SerializeField] private RectTransform playerIcon;

    [SerializeField] private float displayRange = 20f;
    [SerializeField] private float minimapRadius = 100f;


    private MinimapIconMesh[] iconMeshes;


    private void Start()
    {
        iconMeshes = new MinimapIconMesh[sources.Count];

        for (int i = 0; i < sources.Count; i++)
        {
            if (sources[i].iconPrefab != null)
            {
                iconMeshes[i] = CreateIconMesh(sources[i].iconPrefab);
            }
        }
    }


    private void Update()
    {
        UpdateTargetIcons();
        UpdatePlayerIcon();
    }


    private void UpdateTargetIcons()
    {
        Vector3 playerPosition = player.position;

        for (int s = 0; s < sources.Count; s++)
        {
            MinimapIconMesh iconMesh = iconMeshes[s];

            if (iconMesh == null)
            {
                continue;
            }

            iconMesh.ClearIcons();

            EnemyGenerator generator = sources[s].generator;

            if (generator != null)
            {
                IReadOnlyList<GameObject> targets = generator.ActiveEnemies;

                for (int i = 0; i < targets.Count; i++)
                {
                    GameObject target = targets[i];

                    if (target == null)
                    {
                        continue;
                    }

                    UpdateIconPosition(iconMesh, target.transform, playerPosition);
                }
            }

            iconMesh.ApplyIcons();
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


    /// <summary>
    /// Iconのミニマップ上の位置を更新
    /// </summary>
    private void UpdateIconPosition(MinimapIconMesh iconMesh, Transform target, Vector3 playerPosition)
    {
        Vector3 offset =
            target.position - playerPosition;

        Vector2 mapPosition =
            new Vector2(
                offset.x,
                offset.z
            );

        mapPosition =
            mapPosition / displayRange * minimapRadius;

        mapPosition =
            Vector2.ClampMagnitude(
                mapPosition,
                minimapRadius
            );


        // アイコン上方向をPlayerへ向ける
        Vector2 directionToPlayer = -mapPosition;

        float sqrDistance = directionToPlayer.sqrMagnitude;

        if (sqrDistance > 0.001f)
        {
            float distance = Mathf.Sqrt(sqrDistance);

            iconMesh.AddIcon(
                mapPosition,
                directionToPlayer.y / distance,
                -directionToPlayer.x / distance
            );
        }
        else
        {
            iconMesh.AddIcon(
                mapPosition,
                1f,
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