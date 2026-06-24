using UnityEngine;

public class ExpObject : PoolableObject
{
    [SerializeField] private float expAmount = 1;
    public float ExpAmount => expAmount;

    [SerializeField] private EnemyConfig expdata;
    public EnemyConfig Expdata => expdata;
    void Awake()
    {
        expdata.ResetDistance();
    }
  
  

}