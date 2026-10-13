using UnityEngine;
/// <summary>照準を基準に複数方向へ攻撃個体を発射する</summary>

[CreateAssetMenu(menuName = "Weapons/FireMode/SurroundedProjectile")]
public class SurroundedFire : FireModeData
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector3 Offset;
    [SerializeField] private int projectileCount = 8;
    [SerializeField] private bool isTracking = true;


    /// <summary>武器と所有者の設定を使って攻撃を実行する</summary>
    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Vector3 baseDirection = GetFireDirection(weapon);

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * 360f / projectileCount;
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * baseDirection;

            PooledInstance spawned = PoolManager.Instance.Rent(prefab);

            GameObject bullet = spawned.GameObject;

            var damage = spawned.RequireDamage();
            damage.Initialize(GetDamageAmount(weapon, weaponOwner), GetKnockbackForce(weapon, weaponOwner));

            MovementBase movement = spawned.Movement;
            movement?.Initialize(weaponOwner.transform, direction);

            Quaternion rotation = Quaternion.LookRotation(direction);
            bullet.transform.rotation = rotation;
            bullet.transform.position = weapon.AimTransform.position + rotation * Offset;
            if(isTracking)
            {
                bullet.transform.SetParent(weapon.AimTransform);
            }
        }
    }
}
