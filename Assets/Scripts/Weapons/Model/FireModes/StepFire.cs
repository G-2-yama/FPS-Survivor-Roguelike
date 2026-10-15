using UnityEngine;
/// <summary>移動した距離に応じて攻撃個体を生成する</summary>

[CreateAssetMenu(menuName = "Weapons/FireMode/StepFire")]
public class StepFire : FireModeData
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector3 Offset;
    [SerializeField] private float stepInterval = 0.5f;

    private Vector3 previousPosition;
    /// <summary>武器と所有者の設定を使って攻撃を実行する</summary>

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
    /// <summary>移動位置を基準に範囲攻撃を生成する</summary>

    private void CreateDamageArea(Weapon weapon, Player weaponOwner)
    {
        Vector3 direction = weaponOwner.transform.up;

        PooledInstance spawned = PoolManager.Instance.Rent(prefab);

        GameObject bullet = spawned.GameObject;

        var damage = spawned.RequireDamage();
        damage.Initialize(weapon.GetDamageAmount(), weapon.GetKnockbackForce());

        MovementBase movement = spawned.Movement;
        movement?.Initialize(weaponOwner.transform, direction);

        Quaternion rotation = Quaternion.LookRotation(direction);
        bullet.transform.rotation = rotation;
        bullet.transform.position = weaponOwner.FootPosition + rotation * Offset;
    }
}
