using UnityEngine;
/// <summary>範囲内の登録済み対象へ攻撃個体を発射する</summary>

[CreateAssetMenu(menuName = "Weapons/FireMode/AutoTargetProjectile")]
public class AutoTargetFire : FireModeData
{
    [SerializeField] private float searchRadius = 20f;
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 1f, 0f);

    /// <summary>武器と所有者の設定を使って攻撃を実行する</summary>
    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Collider target = FindNearestEnemy(weaponOwner.transform.position);

        Vector3 direction = target != null
            ? (target.transform.position - weapon.AimTransform.position).normalized
            : GetFireDirection(weapon);

        Quaternion rotation = Quaternion.LookRotation(direction);

        PooledInstance spawned = PoolManager.Instance.Rent(prefab);
        GameObject bullet = spawned.GameObject;
        bullet.transform.SetPositionAndRotation(weapon.AimTransform.position + rotation * spawnOffset, rotation);

        var damage = spawned.RequireDamage();
        damage.Initialize(GetDamageAmount(weapon, weaponOwner), GetKnockbackForce(weapon, weaponOwner));

        MovementBase movement = spawned.Movement;
        movement?.Initialize(weaponOwner.transform, direction);
    }
    /// <summary>物理判定の候補から条件を満たす最も近い登録済み対象を選ぶ</summary>

    private Collider FindNearestEnemy(Vector3 center)
    {
        Collider[] colliders = Physics.OverlapSphere(center, searchRadius);

        Collider nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            if (!DamageTargetRegistry.TryResolve(col, out IDamageable damageable))
                continue;

            if ((damageable.TeamType & (TeamType.Enemy | TeamType.Boss)) == 0)
                continue;

            float sqrDistance = (col.transform.position - center).sqrMagnitude;

            if (sqrDistance < nearestDistance)
            {
                nearestDistance = sqrDistance;
                nearest = col;
            }
        }

        return nearest;
    }
}
