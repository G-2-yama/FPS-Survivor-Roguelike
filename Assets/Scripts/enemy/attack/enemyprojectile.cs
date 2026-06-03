using UnityEngine;
using System.Collections;

public class enemyplojectileobject : ProjectileObject,IDamageable

{
    private DeathType deathType = DeathType.Normal;
    [SerializeField] private EnemyConfig config;
    [SerializeField] private GameObject exp;
    [SerializeField] private WhiteFlash whiteFlash;
    [SerializeField] private float deathDelay = 0.1f;
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
        deathType= DeathType.Normal;
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


        if (deathType == DeathType.Normal)
        {

            StartCoroutine(DeathRoutine());

        }
        else
        {

            Release();
        }
    }
    protected override void HandleHit(Collider col)
    {
      
        onHit?.Invoke(col);
        deathType = DeathType.SelfDestruct;
        Health?.TakeDamage(9999);
    }
    private IEnumerator DeathRoutine()
    {

        yield return new WaitForSeconds(deathDelay);

        if (exp != null)
        {
            GameObject expitem = PoolManager.Instance.Get(exp);
            expitem.transform.position = transform.position;
        }

        Release();
    }
}



