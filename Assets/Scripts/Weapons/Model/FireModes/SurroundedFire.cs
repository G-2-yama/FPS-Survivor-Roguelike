using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/SurroundedProjectile")]
public class SurroundedFire : FireModeData
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector3 Offset;
    [SerializeField] private int projectileCount = 8;
    [SerializeField] private bool isTracking = true;


    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Vector3 baseDirection = GetFireDirection(weapon);

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * 360f / projectileCount;
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * baseDirection;

            GameObject bullet = PoolManager.Instance.Get(prefab);

            var damage = bullet.GetComponent<DamageBase>();
            damage.Initialize(weapon.WeaponData.Damage, weapon.WeaponData.KnockbackForce);

            var movement = bullet.GetComponent<MovementBase>();
            movement?.Initialize(weaponOwner.transform, direction);

            Quaternion rotation = Quaternion.LookRotation(direction);
            bullet.transform.rotation = rotation;
            bullet.transform.position = Camera.main.transform.position + rotation * Offset;
            if(isTracking)
            {
                bullet.transform.SetParent(Camera.main.transform);
            }
        }
    }
}