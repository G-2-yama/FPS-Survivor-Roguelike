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
                return;
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

}