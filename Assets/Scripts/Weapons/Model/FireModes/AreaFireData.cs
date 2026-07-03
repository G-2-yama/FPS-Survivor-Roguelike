using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/AreaFire")]
public class AreaFireData : FireModeData
{
    [SerializeField] private Vector2 areaSize = new Vector2(10f, 10f);
    [SerializeField] private float spawnHeight = 20f;
    [SerializeField] private int projectileCount = 1;
    [SerializeField] private GameObject prefab;

    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        for (int i = 0; i < projectileCount; i++)
        {
            FireSingle(weapon, weaponOwner);
        }
    }

    private void FireSingle(Weapon weapon, Player weaponOwner)
    {
        Vector3 center = weaponOwner.transform.position;

        Vector3 spawnPosition = new Vector3(
            center.x + Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
            center.y + spawnHeight,
            center.z + Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f));

        Vector3 direction = Vector3.down;

        GameObject bullet = PoolManager.Instance.Get(prefab);

        bullet.transform.SetPositionAndRotation(spawnPosition, Quaternion.LookRotation(direction));

        var damage = bullet.GetComponent<DamageBase>();
        damage.Initialize(weapon.WeaponData.Damage, weapon.WeaponData.KnockbackForce);

        var movement = bullet.GetComponent<MovementBase>();
        movement?.Initialize(weaponOwner.transform, direction);
    }
}