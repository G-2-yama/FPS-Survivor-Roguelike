using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/Projectile")]
public class NormalProjectileFire : FireModeData
{
    [SerializeField] private GameObject prefab;
    public GameObject Prefab => prefab;
    [SerializeField] private Vector3 Offset = new Vector3(0f, 0f, 0.0f);


    /// <inheritdoc />
    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Vector3 direction = GetFireDirection(weapon);

        GameObject bullet = PoolManager.Instance.Get(prefab);

        var damage = bullet.GetComponent<DamageBase>();
        damage.Initialize(weapon.WeaponData.Damage, weapon.WeaponData.KnockbackForce);

        var movement = bullet.GetComponent<MovementBase>();
        movement?.Initialize(weaponOwner.transform, direction);

        Quaternion rotation = Quaternion.LookRotation(direction);
        bullet.transform.rotation = rotation;
        bullet.transform.position =Camera.main.transform.position + rotation * Offset;
    }
    
}
