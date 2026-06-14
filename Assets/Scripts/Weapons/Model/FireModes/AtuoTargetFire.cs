using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/AutoTargetProjectile")]
public class AutoTargetFire : FireModeData
{
    [SerializeField] private float searchRadius = 20f;
    [SerializeField] private float speed = 80f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 1f, 0);
    

    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Collider target = FindNearestEnemy(weaponOwner.transform.position);

        Vector3 direction;

        if (target != null)
        {
            direction = (target.transform.position - Camera.main.transform.position).normalized;
        }
        else
        {
            direction = GetFireDirection(weapon);
        }

        GameObject bullet = PoolManager.Instance.Get(prefab);

        bullet.transform.position = Camera.main.transform.position + direction * 0.5f + spawnOffset;
        bullet.transform.rotation = Quaternion.LookRotation(direction);

        var projectile = bullet.GetComponent<ProjectileObject>();
        projectile.Initialize(weapon.WeaponData.Damage, lifetime);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = direction * speed;
    }

    private Collider FindNearestEnemy(Vector3 center)
    {
        Collider[] colliders = Physics.OverlapSphere(center, searchRadius);

        Collider nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            IDamageable damageable = col.GetComponent<IDamageable>();

            if (damageable == null)
                continue;

            if (damageable.TeamType != TeamType.Enemy && damageable.TeamType != TeamType.Boss)
                continue;

            float distance = Vector3.Distance(center, col.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = col;
            }
        }

        return nearest;
    }
}