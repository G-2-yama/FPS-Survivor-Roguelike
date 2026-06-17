using UnityEngine;

public class ExplosiveProjectile : ProjectileObject
{
    [SerializeField] private GameObject damageAreaPrefab;

    protected override void HandleHit(Collider col)
    {
        GameObject area = PoolManager.Instance.Get(damageAreaPrefab);
        area.transform.position = transform.position;
        area.GetComponent<DamageArea>().Initialize(damage);
        
        Release();
    }
}