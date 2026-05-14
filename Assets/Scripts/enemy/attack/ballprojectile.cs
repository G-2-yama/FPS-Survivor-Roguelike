using UnityEngine;

public class ballprojectile : enemyplojectileobject
{
    
    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject expprefab;


    protected override void HandleHit(Collider col)
    {
        GameObject area = PoolManager.Instance.Get(prefab);
        area.transform.position = transform.position;
        GameObject expitem = PoolManager.Instance.Get(expprefab);
        expitem.transform.position = transform.position;
        base.HandleHit(col);
    }
}
