using UnityEngine;

public class ExplosiveProjectile : ProjectileObject
{
    [SerializeField] private GameObject damageAreaPrefab;

    protected override void HandleHit(Collider collider)
    {
        GameObject area = PoolManager.Instance.Get(damageAreaPrefab);
        area.transform.position = transform.position;
        area.GetComponent<DamageArea>().Initialize(damage);
    }
}