using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/AreaFire")]
public class AreaFireData : FireModeData
{
    [Header("エリア設定")]
    [SerializeField] private Vector2 areaSize = new Vector2(10f, 10f); // X,Z範囲
    [SerializeField] private float spawnHeight = 20f;   // 上空の高さ

    [Header("弾数設定")]
    [SerializeField] private int projectileCount = 1;

    [Header("プロジェクタイル設定")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float speed = 20f;

    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        for (int i = 0; i < projectileCount; i++)
        {
            FireSingle(weapon, weaponOwner);
        }
    }

    private void FireSingle(Weapon weapon, Player weaponOwner)
    {
        // エリア内のランダムなXZ座標を決定
        Vector3 center = weapon.transform.position;
        Vector3 spawnPosition = new Vector3(
            center.x + Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
            center.y + spawnHeight,
            center.z + Random.Range(-areaSize.y / 2f, areaSize.y / 2f)
        );

        Vector3 direction = Vector3.down;

        GameObject bullet = PoolManager.Instance.Get(prefab);
        bullet.transform.position = spawnPosition;
        bullet.transform.rotation = Quaternion.LookRotation(direction);

        var projectile = bullet.GetComponent<ProjectileObject>();
        projectile.Initialize(weapon.WeaponData.Damage, lifetime);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = direction * speed;
    }
}