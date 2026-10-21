using UnityEngine;
/// <summary>保持済みの弾の参照へ発射設定を渡す</summary>

public class ProjectileLauncher : MonoBehaviour
{
    [SerializeField] private BulletData bulletData;
    [SerializeField] private Sounder sounder;
    public Sounder Sounder => sounder;
    /// <summary>発射した個体へダメージを設定する</summary>

    public void Shoot(Transform shotPoint, Vector3 direction, int damage)
    {
        if (bulletData == null || bulletData.AmmpPrefab == null)
            return;

        PooledInstance bullet = bulletData.SpawnInstance(shotPoint, direction);
       

        if (bullet!=null)
        {
            bullet.GameObject.GetComponent<IInitialize>()?.Initialize(
           damage,
           0f
           );

        }

       
    }

  
}
