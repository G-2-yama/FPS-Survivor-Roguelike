using UnityEngine;
/// <summary>プレイヤー周辺の範囲へ上空から攻撃個体を生成する</summary>

[CreateAssetMenu(menuName = "Weapons/FireMode/AreaFire")]
public class AreaFireData : FireModeData
{
    [SerializeField] private Vector2 areaSize = new Vector2(10f, 10f);
    [SerializeField] private float spawnHeight = 20f;
    [SerializeField] private int projectileCount = 1;
    [SerializeField] private GameObject prefab;

    /// <summary>武器と所有者の設定を使って攻撃を実行する</summary>
    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        for (int i = 0; i < projectileCount; i++)
        {
            FireSingle(weapon, weaponOwner);
        }
    }
    /// <summary>指定された範囲内へ攻撃個体を一つ生成する</summary>

    private void FireSingle(Weapon weapon, Player weaponOwner)
    {
        Vector3 center = weaponOwner.transform.position;

        Vector3 spawnPosition = new Vector3(
            center.x + Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
            center.y + spawnHeight,
            center.z + Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f));

        Vector3 direction = Vector3.down;

        PooledInstance spawned = PoolManager.Instance.Rent(prefab);
        GameObject bullet = spawned.GameObject;

        bullet.transform.SetPositionAndRotation(spawnPosition, Quaternion.LookRotation(direction));

        var damage = spawned.RequireDamage();
        damage.Initialize(GetDamageAmount(weapon, weaponOwner), GetKnockbackForce(weapon, weaponOwner));

        MovementBase movement = spawned.Movement;
        movement?.Initialize(weaponOwner.transform, direction);
    }
}
