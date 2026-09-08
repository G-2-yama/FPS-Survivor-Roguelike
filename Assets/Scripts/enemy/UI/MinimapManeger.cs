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

    [SerializeField] private List<MinimapSource> sources = new();

    [SerializeField] private Transform player;
    [SerializeField] private RectTransform iconParent;
    [SerializeField] private RectTransform playerIcon;

    [SerializeField] private float displayRange = 20f;
    [SerializeField] private float minimapRadius = 100f;


    // 対象Object → 対応するIcon
    private Dictionary<GameObject, MinimapIcon> targetIcons = new();

    // 現在存在している全対象
    private HashSet<GameObject> activeTargets = new();

    // 対象 → 使用するPrefab
    private Dictionary<GameObject, GameObject> targetPrefabs = new();

    private List<GameObject> removeList = new();


    private void Update()
    {
        CollectActiveTargets();

        CreateIcons();
        RemoveIcons();
        UpdateIconPositions();
        UpdatePlayerIcon();
    }


    /// <summary>
    /// 全Generatorから現在ActiveなObjectを集める
    /// </summary>
    private void CollectActiveTargets()
    {
        activeTargets.Clear();
        targetPrefabs.Clear();

        foreach (MinimapSource source in sources)
        {
            if (source.generator == null ||
                source.iconPrefab == null)
            {
                continue;
            }

            var targets = source.generator.ActiveEnemies;

            foreach (GameObject target in targets)
            {
                if (target == null ||
                    !target.activeInHierarchy)
                {
                    continue;
                }

                activeTargets.Add(target);

                // このObjectがどのIconPrefabを使うか
                targetPrefabs[target] = source.iconPrefab;
            }
        }
    }


    /// <summary>
    /// Iconを持っていない対象にPoolからIconを割り当てる
    /// </summary>
    private void CreateIcons()
    {
        foreach (GameObject target in activeTargets)
        {
            if (targetIcons.ContainsKey(target))
                continue;

            GameObject prefab = targetPrefabs[target];

            GameObject iconObj =
                PoolManager.Instance.Get(prefab);

            // Canvas配下へ
            iconObj.transform.SetParent(iconParent, false);

            MinimapIcon icon =
                iconObj.GetComponent<MinimapIcon>();

            targetIcons.Add(target, icon);
        }
    }


    /// <summary>
    /// Generatorから消えた対象のIconをPoolへ返す
    /// </summary>
    private void RemoveIcons()
    {
        removeList.Clear();

        foreach (var pair in targetIcons)
        {
            GameObject target = pair.Key;

            if (target == null ||
                !target.activeInHierarchy ||
                !activeTargets.Contains(target))
            {
                removeList.Add(target);
            }
        }

        foreach (GameObject target in removeList)
        {
            MinimapIcon icon = targetIcons[target];

            icon.Release();

            targetIcons.Remove(target);
        }
    }


    /// <summary>
    /// 各Iconのミニマップ上の位置を更新
    /// </summary>
    private void UpdateIconPositions()
    {
        foreach (var pair in targetIcons)
        {
            GameObject target = pair.Key;
            MinimapIcon icon = pair.Value;

            Vector3 offset =
                target.transform.position - player.position;

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