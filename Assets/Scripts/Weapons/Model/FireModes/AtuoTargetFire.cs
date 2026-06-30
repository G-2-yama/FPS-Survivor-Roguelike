using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/AutoTargetProjectile")]
public class AutoTargetFire : FireModeData
{
    [SerializeField] private float searchRadius = 20f;
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 1f, 0f);

    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Collider target = FindNearestEnemy(weaponOwner.transform.position);

        Vector3 direction = target != null
            ? (target.transform.position - Camera.main.transform.position).normalized
            : GetFireDirection(weapon);

        Quaternion rotation = Quaternion.LookRotation(direction);

        GameObject bullet = PoolManager.Instance.Get(prefab);
        bullet.transform.SetPositionAndRotation(Camera.main.transform.position + rotation * spawnOffset, rotation);

        var damage = bullet.GetComponent<DamageBase>();
        damage.Initialize(weapon.WeaponData.Damage, weapon.WeaponData.KnockbackForce);

        var movement = bullet.GetComponent<MovementBase>();
        movement?.Initialize(weaponOwner.transform, direction);
    }

    private Collider FindNearestEnemy(Vector3 center)
    {
        Collider[] colliders = Physics.OverlapSphere(center, searchRadius);

        Collider nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            if (!col.TryGetComponent(out IDamageable damageable))
                continue;

            if ((damageable.TeamType & (TeamType.Enemy | TeamType.Boss)) == 0)
                continue;

            float sqrDistance = (col.transform.position - center).sqrMagnitude;

            if (sqrDistance < nearestDistance)
            {
                nearestDistance = sqrDistance;
                nearest = col;
            }
        }

        return nearest;
    }
}