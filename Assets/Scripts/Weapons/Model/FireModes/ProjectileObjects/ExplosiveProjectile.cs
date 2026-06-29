using UnityEngine;
using System.Collections.Generic;

public class ExplosiveProjectile : ProjectileObject
{
    [SerializeField] private List<GameObject> damageAreaPrefabs;

    protected override void HandleHit(Collider collider)
    {
        if (collider.TryGetComponent(out IDamageable damageable))
        {
            if ((damageable.TeamType & targetTeam) == 0)
            {
                return;
            }

            foreach (var damageAreaPrefab in damageAreaPrefabs)
            {
                GameObject area = PoolManager.Instance.Get(damageAreaPrefab);
                area.transform.position = transform.position;
                area.GetComponent<DamageArea>().Initialize(damage, knockbackForce);
            }
            Release();
            hasHit = true;
            
        }
        else
        {
            foreach (var damageAreaPrefab in damageAreaPrefabs)
            {
                GameObject area = PoolManager.Instance.Get(damageAreaPrefab);
                area.transform.position = transform.position;
                area.GetComponent<DamageArea>().Initialize(damage, knockbackForce);
            }

            Release();
            hasHit = true;
        }

    }
}