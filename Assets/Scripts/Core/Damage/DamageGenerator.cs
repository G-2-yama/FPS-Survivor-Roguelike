using UnityEngine;
using System.Collections.Generic;

public class DamageGenerator : DamageBase
{
    [SerializeField] private List<GameObject> explosionPrefabs;

    public override void Initialize(int damage, float knockback)
    {
        base.Initialize(damage, knockback);
        foreach (GameObject prefab in explosionPrefabs)
        {
            DamageBase damageBase = prefab.GetComponent<DamageBase>();
            damageBase.Initialize(damage, knockback);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isReleased)
            return;

        if (TryDamage(other, out _))
        {
            Explode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isReleased)
            return;

        // 敵でも壁でも爆発
        if (TryDamage(collision.collider, out _))
        {
            Explode();
        }
        else
        {
            Explode();
        }
    }

    private void Explode()
    {
        foreach (GameObject prefab in explosionPrefabs)
        {
            GameObject obj = PoolManager.Instance.Get(prefab);

            obj.transform.position = transform.position;

            obj.GetComponent<DamageBase>().Initialize(damage, knockbackForce);
        }

        Release();
    }
}