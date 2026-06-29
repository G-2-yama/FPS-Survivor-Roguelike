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
    [SerializeField] private Vector3 Offset = new Vector3(0f, 0f, 0.0f);


    /// <inheritdoc />
    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Vector3 direction = GetFireDirection(weapon);

        GameObject bullet = PoolManager.Instance.Get(prefab);

        bullet.transform.position = Camera.main.transform.position + direction * 0.5f + Offset;
        bullet.transform.rotation = Quaternion.LookRotation(direction);

        var projectile = bullet.GetComponent<ProjectileObject>();
        projectile.Initialize(weapon.WeaponData.Damage, weapon.WeaponData.KnockbackForce, lifetime);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = direction * speed;
    }

    
}
