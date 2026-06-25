using System.Collections.Generic;
using UnityEngine;

public class scriptableinitiarised : MonoBehaviour
{
    [SerializeField] private List<EnemyConfig> enemylist;
    void Awake()
    {
        for(int i=0;i<enemylist.Count;i++)
        {
            enemylist[i].Initialize();
           
        }
    }

    
}
