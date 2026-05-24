using UnityEngine;

public class EnemyHealth : PoolableObject, IDamageable
{
    [SerializeField] private EnemyConfig config;
    [SerializeField] private EnemyAttackController attackController;
    [SerializeField] private GameObject prefab;
    [SerializeField] private WhiteFlash whiteFlash;


    public TeamType TeamType => TeamType.Enemy;
    public Health Health { get; private set; }
  
    

    public void TakeDamage(int damage)
    {

        if (Health == null)
            return;
        if (Health.CurrentHP > 0)
        {
            whiteFlash?.Flash();
        }
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
        Release();
    }
}

