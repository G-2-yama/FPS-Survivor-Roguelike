using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/Projectile")]
public class NormalProjectileFire : FireModeData
{
    [SerializeField] private float speed = 80f;
    public float Speed => speed;

    [SerializeField] private float lifetime = 2f;
    public float Lifetime => lifetime;

    [SerializeField] private GameObject prefab;
    public GameObject Prefab => prefab;


    /// <inheritdoc />
    public override void Fire(Weapon weapon)
    {
        Vector3 direction = GetFireDirection(weapon);

        GameObject bullet = PoolManager.Instance.Get(prefab);

        bullet.transform.position = weapon.Muzzle.position;
        bullet.transform.rotation = Quaternion.LookRotation(direction);

        var projectile = bullet.GetComponent<ProjectileObject>();
        projectile.Initialize((col) => TryApplyDamage(weapon, col), weapon.WeaponData.Damage, lifetime);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = direction * speed;
    }

    
}
