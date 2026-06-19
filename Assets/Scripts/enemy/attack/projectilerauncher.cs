using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    [SerializeField] private BulletData bulletData;

    public void Shoot(Transform shotPoint, Vector3 direction, int damage)
    {
        if (bulletData == null || bulletData.Prefab == null)
            return;

        GameObject bullet = bulletData.Spawn(shotPoint, direction);
        var projectile = bullet.GetComponent<ProjectileObject>();

        if (projectile!=null)
        {
            projectile.Initialize(
           damage,
           bulletData.Lifetime);

        }

       
    }

  
}
