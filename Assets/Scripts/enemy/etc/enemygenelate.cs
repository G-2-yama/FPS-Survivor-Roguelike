using UnityEngine;
using System.Collections;

public class EnemyGenerator : MonoBehaviour
{
    public GameObject enemyPrefab;

    Vector3 defaultpos;
    float time = 0f;

    public float moverange = 5;
    public int capenemy = 5;
    public float mintime = 2;
    public float maxtime = 4;

    int currentEnemyCount = 0;

    void Start()
    {
        defaultpos = transform.position;
        StartCoroutine(Generate());
    }

    void Update()
    {
        time += Time.deltaTime * 2;
        transform.position =
            new Vector3(Mathf.Sin(time), 0, Mathf.Cos(time)) * moverange + defaultpos;
    }

    IEnumerator Generate()
    {
        while (true)
        {
            if (currentEnemyCount < capenemy)
            {
                var obj = PoolManager.Instance.Get(enemyPrefab);
            
                currentEnemyCount++;

                
             

                obj.transform.position = transform.position;
                obj.transform.rotation = transform.rotation;

                yield return new WaitForSeconds(Random.Range(mintime, maxtime));
            }
            else
            {
                yield return null;
            }
        }
    }
}