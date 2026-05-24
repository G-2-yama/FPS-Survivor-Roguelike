using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGenerateManager : MonoBehaviour
{
    [SerializeField] private List<Transform> enemyGenerators = new List<Transform>();

    [SerializeField] private float minSpawnTime = 2f;
    [SerializeField] private float maxSpawnTime = 4f;

    [SerializeField] private float changeInterval = 20f;

    [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>();

    [SerializeField] private GameObject timerObject;

    private Timer timer;

    
    private List<Coroutine> spawnCoroutines = new List<Coroutine>();

    private int currentEnemyIndex = 0;
    private float nextChangeTime;

    private void Start()
    {
        timer = timerObject.GetComponent<Timer>();

        StartSpawn(enemyPrefabs[currentEnemyIndex]);

        nextChangeTime = changeInterval;
    }

    private void Update()
    {
        if (timer.ElapsedTime >= nextChangeTime)
        {
            ChangeEnemy();

            nextChangeTime += changeInterval;
        }
    }

    private void ChangeEnemy()
    {
        // ç°ÇÃê∂ê¨í‚é~
        StopAllSpawn();

        // éüÇÃìGÇ÷
        currentEnemyIndex++;

        // ç≈å„Ç‹Ç≈çsÇ¡ÇΩÇÁÉãÅ[Év
        if (currentEnemyIndex >= enemyPrefabs.Count)
        {
            currentEnemyIndex = 0;
        }

        // êVÇµÇ¢ìGê∂ê¨äJén
        StartSpawn(enemyPrefabs[currentEnemyIndex]);
    }

    private void StartSpawn(GameObject prefab)
    {
        for (int i = 0; i < enemyGenerators.Count; i++)
        {
            Coroutine c = StartCoroutine(
                Generate(prefab, enemyGenerators[i])
            );

            spawnCoroutines.Add(c);
        }
    }

    private void StopAllSpawn()
    {
        for (int i = 0; i < spawnCoroutines.Count; i++)
        {
            StopCoroutine(spawnCoroutines[i]);
        }

        spawnCoroutines.Clear();
    }

    private IEnumerator Generate(GameObject prefab, Transform spawnPoint)
    {
        while (true)
        {
            var obj = PoolManager.Instance.Get(prefab);

            obj.transform.position = spawnPoint.position;
            obj.transform.rotation = spawnPoint.rotation;

            yield return new WaitForSeconds(
                Random.Range(minSpawnTime, maxSpawnTime)
            );
        }
    }
}