using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/StepFire")]
public class StepFire : FireModeData
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector3 Offset;
    [SerializeField] private float stepInterval = 0.5f;

    private Vector3 previousPosition;

    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Vector3 currentPosition = weaponOwner.transform.position;

        // 前回生成位置から一定距離以上移動していたら生成
        if (Vector3.Distance(currentPosition, previousPosition) >= stepInterval)
        {
            CreateDamageArea(weapon, weaponOwner);
            previousPosition = currentPosition;
        }
    }

    private void CreateDamageArea(Weapon weapon, Player weaponOwner)
    {
        Vector3 direction = weaponOwner.transform.up;

        GameObject bullet = PoolManager.Instance.Get(prefab);

        var damage = bullet.GetComponent<DamageBase>();
        damage.Initialize(weapon.WeaponData.Damage, weapon.WeaponData.KnockbackForce);

        var movement = bullet.GetComponent<MovementBase>();
        movement?.Initialize(weaponOwner.transform, direction);

        Quaternion rotation = Quaternion.LookRotation(direction);
        bullet.transform.rotation = rotation;
        bullet.transform.position = Camera.main.transform.position + rotation * Offset;
    }
}