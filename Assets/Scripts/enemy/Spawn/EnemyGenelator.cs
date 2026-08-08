using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] private List<PhaseData> phases;

    private int currentPhaseIndex;
    private PhaseData currentPhase;

    [SerializeField] private float spawnRadius = 15f;

    [SerializeField] private Timer timer;
    [SerializeField] private int maxSpawnTry = 5;
    private List<GameObject> activeEnemies
    = new List<GameObject>();
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
        UpdatePhase();
    }

    private void UpdatePhase()
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
            SpawnEnemy(currentPhase);

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
            // スポーンできる位置を決定する
            Vector2 direction = Random.insideUnitCircle.normalized;
            Vector3 spawnPosition = transform.position + new Vector3(direction.x, 0, direction.y) * spawnRadius;

            if(IsSpawnable(spawnPosition))
            {
                GameObject enemy = PoolManager.Instance.Get(enemyData.Prefab);
                enemy.transform.position = spawnPosition;
                activeEnemies.Add(enemy);
                enemy.GetComponent<Enemy>()?.SetSpawner(this);
                return;
            }
            //最も遠い敵を非アクティブにする
            if (activeEnemies.Count >= maxEnemyCount)
            {
                GameObject oldEnemy = GetFarthestEnemy();

                RemoveActiveEnemy(oldEnemy);

                oldEnemy.GetComponent<Enemy>()?.Release();
            }
        }
    }

    private bool IsSpawnable(Vector3 position)
    {
        return Physics.OverlapSphere(position, 0.5f).Length == 0;
    }

    private EnemySpawnData GetRandomEnemy(PhaseData phase)
    {
        int totalWeight = 0;
        foreach (var enemy in phase.Enemies)
        {
            totalWeight += enemy.SpawnWeight;
        }

        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var enemy in phase.Enemies)
        {
            currentWeight += enemy.SpawnWeight;
            if (randomValue < currentWeight)
            {
                return enemy;
            }
        }

        return null;
    }
    private GameObject GetFarthestEnemy()
    {
        GameObject farthestEnemy = null;
        float maxSqrDistance = -1f;

        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy == null || !enemy.activeSelf)
                continue;

            float sqrDistance =
                (enemy.transform.position - this.transform.position).sqrMagnitude;

            if (sqrDistance > maxSqrDistance)
            {
                maxSqrDistance = sqrDistance;
                farthestEnemy = enemy;
            }
        }

        return farthestEnemy;
    }

}