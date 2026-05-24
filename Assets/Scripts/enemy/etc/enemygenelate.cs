using UnityEngine;
using System.Collections;

public class EnemyGenerator : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float mintime = 2;
    public float maxtime = 4;

    
    void Start()
    {
       
        StartCoroutine(Generate());
    }

    
  

    public IEnumerator Generate()
    {
        while (true)
        {
          
            
                var obj = PoolManager.Instance.Get(enemyPrefab);
            
                obj.transform.position = transform.position;
                obj.transform.rotation = transform.rotation;

                yield return new WaitForSeconds(Random.Range(mintime, maxtime));
          
        }
    }
}