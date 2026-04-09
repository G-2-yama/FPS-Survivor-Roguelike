using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

public class EnemyGenerator : MonoBehaviour
{
    public GameObject enemyPrefab;

    public Vector3 defaultpos;
    float time = 0f;

    public float moverange = 5;
    public int capenemy = 5;
    public float mintime = 2;
    public float maxtime = 4;

    ObjectPool<GameObject> pool;

    void Start()
    {
        defaultpos = transform.position;

        pool = new ObjectPool<GameObject>(
            CreateEnemy,
            OnGetEnemy,
            OnReleaseEnemy,
            OnDestroyEnemy,
            true,
            10,
            20
        );

        StartCoroutine(Generate());
    }

    void Update()
    {
        time += Time.deltaTime * 2;
        transform.position =
            new Vector3(Mathf.Sin(time), 0, 0) * moverange + defaultpos;
    }

    GameObject CreateEnemy()
    {
        return Instantiate(enemyPrefab);
    }

    void OnGetEnemy(GameObject obj)
    {
        obj.SetActive(true);
    }

    void OnReleaseEnemy(GameObject obj)
    {
        obj.SetActive(false);
    }

    void OnDestroyEnemy(GameObject obj)
    {
        Destroy(obj);
    }

    IEnumerator Generate()
    {
        while (true)
        {
            if (pool.CountActive < capenemy)
            {
                GameObject obj = pool.Get();

                Enemycondition enemy = obj.GetComponent<Enemycondition>();

                enemy.Init(() =>
                {
                    pool.Release(obj);
                });

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