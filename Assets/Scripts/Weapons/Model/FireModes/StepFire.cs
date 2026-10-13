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
        damage.Initialize(GetDamageAmount(weapon, weaponOwner), GetKnockbackForce(weapon, weaponOwner));

        var movement = bullet.GetComponent<MovementBase>();
        movement?.Initialize(weaponOwner.transform, direction);

        Quaternion rotation = Quaternion.LookRotation(direction);
        bullet.transform.rotation = rotation;
        bullet.transform.position = GetFootPosition(weaponOwner) + rotation * Offset;
    }

    private static Vector3 GetFootPosition(Player weaponOwner)
    {
        Vector3 position = weaponOwner.transform.position;
        if (weaponOwner.TryGetComponent(out CharacterController controller))
        {
            position.y = controller.bounds.min.y;
        }
        return position;
    }
}