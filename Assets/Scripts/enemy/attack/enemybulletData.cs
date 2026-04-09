using UnityEngine;
[CreateAssetMenu(menuName = "enemy/attack")]
public class enemybulletData:ScriptableObject
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float speed = 10f;
    public float Speed => speed;

    [SerializeField] private float lifetime = 2f;
    public float Lifetime => lifetime;

    [SerializeField] private GameObject prefab;
    public GameObject Prefab => prefab;
    [SerializeField] private enemyDatas enemydata;
    public enemyDatas EnemyData => enemydata;
  
    
    protected int GetDamageAmount(enemyDatas enemydata)
    {
        return Mathf.RoundToInt(enemydata.Atk);
    }

    public bool TryApplyDamage(Collider hitCollider)
    {
        var damageable = hitCollider.GetComponent<IDamageable>();
        if (damageable == null) return false;

        if (damageable.TeamType != TeamType.Player
            && damageable.TeamType != TeamType.Boss) return false;

        damageable.TakeDamage(GetDamageAmount(enemydata));
        return true;
    }
    public void Shot(Transform shotpoint, Vector3 direction)
    {
       
        GameObject bullet = Instantiate(
            prefab,
            shotpoint.position,
            Quaternion.LookRotation(direction)
        );
        Debug.Log(bullet);
        var projectile = bullet.GetComponent<ProjectileObject>();
        projectile.Initialize((col) => TryApplyDamage(col), lifetime);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = direction * speed;
    }

}

