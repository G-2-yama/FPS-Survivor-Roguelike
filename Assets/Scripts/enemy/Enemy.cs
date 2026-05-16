using UnityEngine;

public class EnemyHealth : PoolableObject, IDamageable
{
    [SerializeField] private EnemyConfig config;
    [SerializeField] private EnemyAttackController attackController;
    [SerializeField] private GameObject prefab;


    public TeamType TeamType => TeamType.Enemy;
    public Health Health { get; private set; }
  
    

    public void TakeDamage(int damage)
    {
        Health?.TakeDamage(config.Damagelange*damage);
    }

    public override void OnGet()
    {
        Health = new Health(config.MaxHp);
        Health.OnDeath += HandleDeath;
    }

    public override void OnRelease()
    {
        attackController?.CancelAttack();

        if (Health != null)
        {
            Health.OnDeath -= HandleDeath;
            Health = null;
        }
        
      

    }

    private void HandleDeath()

    {
        GameObject expitem= PoolManager.Instance.Get(prefab);
        expitem.transform.position = transform.position;
        Debug.Log("death");
        Release();
    }
}

