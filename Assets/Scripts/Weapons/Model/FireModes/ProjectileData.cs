using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/Projectile")]
public class ProjectileData : FireModeData
{
    [SerializeField] private float speed = 80f;
    public float Speed => speed;

    [SerializeField] private float lifetime = 2f;
    public float Lifetime => lifetime;

    [SerializeField] private GameObject prefab;
    public GameObject Prefab => prefab;

    /// <inheritdoc />
    public override void Fire(Weapon weapon, Vector3 direction)
    {
        GameObject bullet = Instantiate(
            prefab,
            Camera.main.transform.position,
            Quaternion.LookRotation(direction)
        );

        var projectile = bullet.GetComponent<ProjectileObject>();
        projectile.Initialize(weapon, this, lifetime);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = direction * speed;
    }
}
