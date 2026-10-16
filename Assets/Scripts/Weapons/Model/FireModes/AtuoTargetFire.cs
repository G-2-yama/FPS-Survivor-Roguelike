using UnityEngine;
/// <summary>範囲内の登録済み対象へ攻撃個体を発射する</summary>

[CreateAssetMenu(menuName = "Weapons/FireMode/AutoTargetProjectile")]
public class AutoTargetFire : FireModeData
{
    [SerializeField] private float searchRadius = 20f;
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 1f, 0f);

    protected override GameObject SpawnPrefab => prefab;

    /// <summary>武器と所有者の設定を使って攻撃を実行する</summary>
    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Collider target = DamageTargetSearch.FindNearest(
            weaponOwner.transform.position, searchRadius, TeamType.Enemy | TeamType.Boss, out _);

        Vector3 direction = target != null
            ? (target.transform.position - weapon.AimTransform.position).normalized
            : GetFireDirection(weapon);

        Quaternion rotation = Quaternion.LookRotation(direction);

        PooledInstance spawned = PoolManager.Instance.Rent(prefab);
        GameObject bullet = spawned.GameObject;
        bullet.transform.SetPositionAndRotation(weapon.AimTransform.position + rotation * spawnOffset, rotation);

        InitializeDamage(spawned, weapon);

        MovementBase movement = spawned.Movement;
        movement?.Initialize(weaponOwner.transform, direction);
    }
}
