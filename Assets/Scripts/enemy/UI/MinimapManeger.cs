using System.Collections.Generic;
using UnityEngine;

public class MinimapManager : MonoBehaviour
{
    [System.Serializable]
    public class MinimapSource
    {
        public EnemyGenerator generator;
        public GameObject iconPrefab;
    }

    private class TrackedIcon
    {
        public MinimapIcon icon;
        public Transform target;
        public int seenStamp;
    }

    [SerializeField] private List<MinimapSource> sources = new();

    [SerializeField] private Transform player;
    [SerializeField] private RectTransform iconParent;
    [SerializeField] private RectTransform playerIcon;

    [SerializeField] private float displayRange = 20f;
    [SerializeField] private float minimapRadius = 100f;


    // 対象Object → 対応するIcon
    private Dictionary<GameObject, TrackedIcon> targetIcons = new();

    private List<GameObject> removeList = new();

    private int currentStamp;


    private void Update()
    {
        currentStamp++;

        UpdateTargetIcons();
        RemoveIcons();
        UpdatePlayerIcon();
    }


    private void UpdateTargetIcons()
    {
        Vector3 playerPosition = player.position;

        foreach (MinimapSource source in sources)
        {
            if (source.generator == null ||
                source.iconPrefab == null)
            {
                continue;
            }

            IReadOnlyList<GameObject> targets = source.generator.ActiveEnemies;

            for (int i = 0; i < targets.Count; i++)
            {
                GameObject target = targets[i];

                if (target == null ||
                    !target.activeInHierarchy)
                {
                    continue;
                }

                if (!targetIcons.TryGetValue(target, out TrackedIcon tracked))
                {
                    tracked = CreateIcon(target, source.iconPrefab);
                }
                else if (tracked.seenStamp == currentStamp)
                {
                    continue;
                }

                tracked.seenStamp = currentStamp;

                UpdateIconPosition(tracked, playerPosition);
            }
        }
    }


    private TrackedIcon CreateIcon(GameObject target, GameObject prefab)
    {
        GameObject iconObj =
            PoolManager.Instance.Get(prefab);

        // Canvas配下へ
        iconObj.transform.SetParent(iconParent, false);

        TrackedIcon tracked = new TrackedIcon
        {
            icon = iconObj.GetComponent<MinimapIcon>(),
            target = target.transform
        };

        targetIcons.Add(target, tracked);

        return tracked;
    }


    /// <summary>
    /// Generatorから消えた対象のIconをPoolへ返す
    /// </summary>
    private void RemoveIcons()
    {
        removeList.Clear();

        foreach (var pair in targetIcons)
        {
            if (pair.Value.seenStamp != currentStamp)
            {
                removeList.Add(pair.Key);
            }
        }

        foreach (GameObject target in removeList)
        {
            MinimapIcon icon = targetIcons[target].icon;

            icon.Release();

            targetIcons.Remove(target);
        }
    }


    /// <summary>
    /// Iconのミニマップ上の位置を更新
    /// </summary>
    private void UpdateIconPosition(TrackedIcon tracked, Vector3 playerPosition)
    {
        MinimapIcon icon = tracked.icon;

        Vector3 offset =
            tracked.target.position - playerPosition;

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

        icon.RectTransform.anchoredPosition =
            mapPosition;


        // アイコン上方向をPlayerへ向ける
        Vector2 directionToPlayer = -mapPosition;

        if (directionToPlayer.sqrMagnitude > 0.001f)
        {
            float angle =
                Mathf.Atan2(
                    directionToPlayer.y,
                    directionToPlayer.x
                )
                * Mathf.Rad2Deg
                - 90f;

            icon.RectTransform.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    angle
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