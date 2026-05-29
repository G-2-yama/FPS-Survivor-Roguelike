using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    [SerializeField] private BulletData bulletData;
    [SerializeField] private TeamType ownerTeam = TeamType.Enemy;

    public void Shoot(Transform shotPoint, Vector3 direction, int damage)
    {
        if (bulletData == null || bulletData.Prefab == null)
            return;

        GameObject bullet = bulletData.Spawn(shotPoint, direction);

        if (!bullet.TryGetComponent<ProjectileObject>(out var projectile))
        {
            Debug.LogError($"{name}: ProjectileObject ��������܂���B");
            return;
        }

        projectile.Initialize(
            damage,
            bulletData.Lifetime);
    }

    private bool TryApplyDamage(Collider hitCollider, int damage)
    {
        if (hitCollider == null) return false;

        var damageable = hitCollider.GetComponent<IDamageable>();
        if (damageable == null) return false;
        if (damageable.TeamType == ownerTeam) return false;

        damageable.TakeDamage(damage);
        return true;
    }
}
