using UnityEngine;

public class ExplosiveProjectile : ProjectileObject
{
    [SerializeField] private GameObject damageAreaPrefab;

    protected override void HandleHit(Collider collider)
    {
        if (collider.TryGetComponent(out IDamageable damageable))
        {
            if ((damageable.TeamType & targetTeam) == 0)
            {
                return;
            }

            GameObject area = PoolManager.Instance.Get(damageAreaPrefab);
            area.transform.position = transform.position;
            area.GetComponent<DamageArea>().Initialize(damage);

            Release();
            hasHit = true;
            
        }
        else
        {
            GameObject area = PoolManager.Instance.Get(damageAreaPrefab);
            area.transform.position = transform.position;
            area.GetComponent<DamageArea>().Initialize(damage);

            Release();
            hasHit = true;
        }

    }
}