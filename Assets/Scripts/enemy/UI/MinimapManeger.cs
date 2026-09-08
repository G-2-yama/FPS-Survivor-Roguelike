using System.Collections.Generic;
using UnityEngine;

public class MinimapManager : MonoBehaviour
{
    [SerializeField] private EnemyGenerator enemyGenerator;
    [SerializeField] private Transform player;

    [SerializeField] private GameObject enemyIconPrefab;
    [SerializeField] private RectTransform iconParent;

    [SerializeField] private float displayRange = 20f;
    [SerializeField] private float minimapRadius = 100f;

    // Enemy Å® MinimapIcon
    private Dictionary<GameObject, MinimapIcon> enemyIcons = new();

    private List<GameObject> removeList = new();

    private void Update()
    {
        var enemies = enemyGenerator.ActiveEnemies;

        CreateIcons(enemies);
        RemoveIcons(enemies);
        UpdateIconPositions();
    }

    private void CreateIcons(IReadOnlyList<GameObject> enemies)
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null)
                continue;

            // Ç∑Ç≈Ç…IconÇ™ë∂ç›ÇµÇƒÇ¢ÇΩÇÁâΩÇ‡ÇµÇ»Ç¢
            if (enemyIcons.ContainsKey(enemy))
                continue;

            GameObject iconObj =
                PoolManager.Instance.Get(enemyIconPrefab);

            // Canvasì‡Ç…à⁄ìÆ
            iconObj.transform.SetParent(iconParent, false);

            MinimapIcon icon =
                iconObj.GetComponent<MinimapIcon>();

            enemyIcons.Add(enemy, icon);
        }
    }

    private void RemoveIcons(IReadOnlyList<GameObject> enemies)
    {
        removeList.Clear();

        foreach (var pair in enemyIcons)
        {
            GameObject enemy = pair.Key;

            if (enemy == null ||
                !enemy.activeInHierarchy ||
                !ContainsEnemy(enemies, enemy))
            {
                removeList.Add(enemy);
            }
        }

        foreach (GameObject enemy in removeList)
        {
            MinimapIcon icon = enemyIcons[enemy];

            icon.Release();

            enemyIcons.Remove(enemy);
        }
    }

    private void UpdateIconPositions()
    {
        foreach (var pair in enemyIcons)
        {
            GameObject enemy = pair.Key;
            MinimapIcon icon = pair.Value;

            Vector3 offset =
                enemy.transform.position - player.position;

            Vector2 mapPosition =
                new Vector2(offset.x, offset.z);

            mapPosition =
                mapPosition / displayRange * minimapRadius;

            mapPosition =
                Vector2.ClampMagnitude(
                    mapPosition,
                    minimapRadius
                );

            icon.RectTransform.anchoredPosition =
                mapPosition;
        }
    }

    private bool ContainsEnemy(
        IReadOnlyList<GameObject> enemies,
        GameObject target)
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == target)
                return true;
        }

        return false;
    }
}