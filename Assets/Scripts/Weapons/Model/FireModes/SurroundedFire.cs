using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/SurroundedProjectile")]
public class SurroundedFire : FireModeData
{
    [SerializeField] private GameObject prefab;

    [SerializeField] private float speed = 30f;

    [SerializeField] private float lifetime = 2f;

    [SerializeField]
    private int projectileCount = 8;

    [SerializeField]
    private float spawnRadius = 1.5f;

    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Vector3 center = weaponOwner.transform.position;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * Mathf.PI * 2f / projectileCount;

            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            GameObject bullet = PoolManager.Instance.Get(prefab);

            // プレイヤーの周囲から出現
            bullet.transform.position = center + direction * spawnRadius;
            bullet.transform.rotation = Quaternion.LookRotation(direction);

            ProjectileObject projectile = bullet.GetComponent<ProjectileObject>();
            projectile.Initialize(
                weapon.WeaponData.DamageProfile.GetDamageAmount(weaponOwner),
                weapon.WeaponData.DamageProfile.GetKnockbackForce(weaponOwner),
                lifetime);

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.linearVelocity = direction * speed;
        }
    }
}