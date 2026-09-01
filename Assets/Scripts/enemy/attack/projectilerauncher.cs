using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    [SerializeField] private BulletData bulletData;
    [SerializeField] private Sounder sounder;
    public Sounder Sounder => sounder;

    public void Shoot(Transform shotPoint, Vector3 direction, int damage)
    {
        if (bulletData == null || bulletData.AmmpPrefab == null)
            return;

        GameObject bullet = bulletData.Spawn(shotPoint, direction);
        var projectile = bullet.GetComponent<enemyplojectileobject>();

        if (projectile!=null)
        {
            projectile.Initialize(
           damage,
           0f
           );

        }

       
    }

  
}
