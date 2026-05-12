using UnityEngine;

public class enemyplojectileobject : ProjectileObject,IDamageable
{
    [SerializeField] private EnemyConfig config;
    public TeamType TeamType => TeamType.Enemy;
    public Health Health { get; private set; }
    public void TakeDamage(int damage)
    {
        Health?.TakeDamage(config.Damagelange*damage);
    }
    public override void OnGet()
    {
        base.OnGet();
        Health = new Health(config.MaxHp);
        Health.OnDeath += HandleDeath;

    }
    public override void OnRelease()
    {
        base.OnRelease();
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
    protected override void HandleHit(Collider col)
    {
        onHit?.Invoke(col);
        Health?.TakeDamage(9999);
    }
}


