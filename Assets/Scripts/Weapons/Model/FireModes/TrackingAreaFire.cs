using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/TrackingAreaFire")]
public class TrackingAreaFire : FireModeData
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector3 Offset;

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
        bullet.transform.SetParent(Camera.main.transform);
    }
    
}
