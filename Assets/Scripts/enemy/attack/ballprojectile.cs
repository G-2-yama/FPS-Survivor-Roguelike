using UnityEngine;

public class ballprojectile : enemyplojectileobject
{
    
    [SerializeField] private GameObject prefab;
    


    protected override void HandleHit(Collider col)
    {
        GameObject area = PoolManager.Instance.Get(prefab);
        area.transform.position = transform.position;
       
        base.HandleHit(col);
    }
}
