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

    int enemycount = 0;

    void Start()
    {
        defaultpos = transform.position;


        StartCoroutine(Generate());
    }

   

    void Update()
    {
        time += Time.deltaTime * 2;
        transform.position =
            new Vector3(Mathf.Sin(time), 0, 0) * moverange + defaultpos;
    }

    IEnumerator Generate()
    {
        while (true)
        {
            if (enemycount < capenemy)
            {
               

                GameObject obj = Instantiate(enemyPrefab);
                obj.transform.position = transform.position;
                obj.transform.rotation = transform.rotation;

                enemycount++;

                yield return new WaitForSeconds(Random.Range(mintime, maxtime));
            }
            else
            {
                yield return null;
            }
        }
    }
}