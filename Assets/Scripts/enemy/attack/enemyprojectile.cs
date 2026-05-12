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
    protected override void OnCollisionEnter(Collision collision)
    {
        var selfDamageable = GetComponent<IDamageable>();
        var damageable = collision.collider.GetComponentInParent<IDamageable>();
        if (damageable == selfDamageable)
            return;
        if (selfDamageable != null && damageable != null)
        {
            if ((selfDamageable.TeamType & damageable.TeamType) != 0)
            { return; }
        }
        else if (selfDamageable != null && damageable == null)
        { return; }
        else if (selfDamageable == null && damageable != null)
        { return; }

        base.OnCollisionEnter(collision);
    }
    protected override void OnTriggerEnter(Collider other)
    {
        var selfDamageable = GetComponent<IDamageable>();
        var damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == selfDamageable) 
            return; 
        if (selfDamageable != null && damageable != null)
        { if ((selfDamageable.TeamType & damageable.TeamType) != 0) 
            { return; } } 
        else if (selfDamageable != null && damageable == null) 
        { return; } 
        else if (selfDamageable == null && damageable != null) 
        { return; }
        base.OnTriggerEnter(other);
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


