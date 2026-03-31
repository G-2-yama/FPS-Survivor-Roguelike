using UnityEngine;
using System.Collections;
public class enemygenelate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject enemy;
    public Vector3 defaultpos;
    public int enemycount = 0;
    float time = 0f;
    public float moverange = 5;
    public int capenemy = 5;
    public float mintime = 4;
    public float maxtime = 7;
    void Start()
    {
        StartCoroutine(gene());
        defaultpos = transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime * 2;
        transform.position = new Vector3(Mathf.Sin(time), 0, 0) * moverange + defaultpos;
    }
    IEnumerator gene()
    {
        while (true)
        {
            enemycount++;
            GameObject newenemy = Instantiate(enemy, transform.position, transform.rotation);
            if (enemycount < capenemy)
            {
                yield return new WaitForSeconds(Random.Range(mintime, maxtime));
            }
            else
            {
                yield break;
            }

        }
    }
}
