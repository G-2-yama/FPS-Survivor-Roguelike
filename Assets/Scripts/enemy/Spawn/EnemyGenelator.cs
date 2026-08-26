using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] private List<PhaseData> phases;

    private int currentPhaseIndex;
    private PhaseData currentPhase;

    [SerializeField] private float spawnRadius = 15f;

    // 敵のスポーン方向のデータ
    [SerializeField] private SpawnDirectionData[] spawnDirections;
    [SerializeField] private Timer timer;
    [SerializeField] private int maxSpawnTry = 5;
    private List<GameObject> activeEnemies = new List<GameObject>();

    //敵生成上限数
    [SerializeField] private int maxEnemyCount = 10;

    private void Start()
    {
        phases.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

        currentPhaseIndex = 0;
        currentPhase = phases[0];

        StartCoroutine(SpawnRoutine());
    }

    public void Update()
    {
        if (currentPhaseIndex + 1 >= phases.Count)
            return;

        PhaseData nextPhase = phases[currentPhaseIndex + 1];

        if (timer.ElapsedTime >= nextPhase.StartTime)
        {
            Debug.Log("フェーズ: " + currentPhaseIndex + " -> " + (currentPhaseIndex + 1));
            currentPhaseIndex++;
            currentPhase = phases[currentPhaseIndex];
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // 最低スポーン数を満たすように生成
            if (activeEnemies.Count < currentPhase.MinSpawnCount)
            {
                int spawnCount = currentPhase.MinSpawnCount - activeEnemies.Count;

                for (int i = 0; i < spawnCount; i++)
                {
                    SpawnEnemy(currentPhase);
                }
            }
            else
            {
                SpawnEnemy(currentPhase);
            }

            yield return new WaitForSeconds(currentPhase.SpawnInterval);
        }
    }

    public void RemoveActiveEnemy(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
    }

    private void SpawnEnemy(PhaseData phase)
    {
        EnemySpawnData enemyData = GetRandomEnemy(phase);

        if (enemyData == null)
            return;

        for (int i = 0; i < maxSpawnTry; i++)
        {
            // Playerの向いている方向を基準にスポーン方向を決定
            Vector3 direction = GetSpawnDirection();
            Vector3 spawnPosition = transform.position + direction * spawnRadius;

            // 敵の最大数を超えたら最も遠い敵を削除
            if (activeEnemies.Count >= maxEnemyCount)
            {
                GameObject oldEnemy = GetFarthestEnemy();

                if (oldEnemy != null)
                {
                    RemoveActiveEnemy(oldEnemy);
                    oldEnemy.GetComponent<Enemy>()?.Release();
                }
            }

            if (IsSpawnable(spawnPosition))
            {
                GameObject enemy = PoolManager.Instance.Get(enemyData.Prefab);

                enemy.transform.position = spawnPosition;

                activeEnemies.Add(enemy);

                enemy.GetComponent<Enemy>()?.SetSpawner(this);

                return;
            }
        }
    }

    private bool IsSpawnable(Vector3 position)
    {
        return Physics.OverlapSphere(position, 0.5f).Length == 0;
    }

    /// <summary>
    /// フェーズの敵リストからランダムに敵を選択する
    /// </summary>
    /// <param name="phase">フェーズデータ</param>
    /// <returns>敵データ</returns>
    private EnemySpawnData GetRandomEnemy(PhaseData phase)
    {
        // 全体の重みを計算
        int totalWeight = 0;
        foreach (var enemy in phase.Enemies)
        {
            totalWeight += enemy.SpawnWeight;
        }

        // ランダムに敵を選択
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var enemy in phase.Enemies)
        {
            currentWeight += enemy.SpawnWeight;

            // 敵決定
            if (randomValue < currentWeight)
            {
                return enemy;
            }
        }

        return null;
    }

    /// <summary>
    /// 敵をスポーンさせる方向を決定する
    /// </summary>
    /// <returns>スポーン方向</returns>
    private Vector3 GetSpawnDirection()
    {
        Vector3 forward = transform.forward;

        // 全体の重みを計算
        float totalWeight = 0f;
        foreach (var data in spawnDirections)
        {
            totalWeight += data.weight;
        }

        // ランダムに方向を選択
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var data in spawnDirections)
        {
            currentWeight += data.weight;

            // 方向決定
            if (randomValue < currentWeight)
            {
                float angle = Random.Range(-data.angle, data.angle);
                return Quaternion.Euler(0f, angle, 0f) * forward;
            }
        }

        return forward;
    }

    private GameObject GetFarthestEnemy()
    {
        GameObject farthestEnemy = null;
        float maxSqrDistance = -1f;

        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy == null || !enemy.activeSelf)
                continue;

            float sqrDistance = (enemy.transform.position - this.transform.position).sqrMagnitude;

            if (sqrDistance > maxSqrDistance)
            {
                maxSqrDistance = sqrDistance;
                farthestEnemy = enemy;
            }
        }

        return farthestEnemy;
    }
}