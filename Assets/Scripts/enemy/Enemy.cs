using UnityEngine;

public class EnemyHealth : PoolableObject, IDamageable
{
    [SerializeField] private EnemyConfig config;
    [SerializeField] private EnemyAttackController attackController;


    public TeamType TeamType => TeamType.Enemy;
    public Health Health { get; private set; }

    public void TakeDamage(int damage)
    {
        Health?.TakeDamage(damage);
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
        Release();
    }
}

