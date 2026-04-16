using UnityEngine;

[CreateAssetMenu(menuName = "enemy/BulletData")]
public class enemybulletData : ScriptableObject
{
    public float Speed = 10f;
    public float Lifetime = 2f;
    public GameObject Prefab;

    public void Shot(Transform shotPoint, Vector3 direction)
    {
        GameObject bullet = PoolManager.Instance.Get(Prefab);
        bullet.transform.position = shotPoint.position;
        bullet.transform.rotation = Quaternion.LookRotation(direction);

        var projectile = bullet.GetComponent<ProjectileObject>();
        projectile.Initialize((col) => TryApplyDamage(col), Lifetime);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = direction * Speed;
    }

    public bool TryApplyDamage(Collider hitCollider)
    {
        var damageable = hitCollider.GetComponent<IDamageable>();
        if (damageable == null) return false;

        // É`Å[ÉÄîªíË
        if (damageable.TeamType == TeamType.Enemy) return false;

        damageable.TakeDamage(1); // à–óÕÇÕÇ±Ç±Ç©enemyDataÇ©ÇÁéÊìæ
        return true;
    }
}