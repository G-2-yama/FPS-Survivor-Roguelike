using UnityEngine;
using System.Collections;

public class enemyplojectileobject : PoolableObject, IDamageable
{
    private DeathType deathType = DeathType.Normal;
    [SerializeField] protected EnemyConfig config;
    [SerializeField] private GameObject exp;
    [SerializeField] private WhiteFlash whiteFlash;
    [SerializeField] private float deathDelay = 0.1f;
    protected Health health;
    public TeamType TeamType => TeamType.EnemyAmmo;
    public TeamType targetTeam => TeamType.Player;

    protected bool hasHit = false;

    public void TakeDamage(int damage, float knockbackForce)
    {
        if (health == null)
            return;

        if (health.CurrentHP > 0)
        {
            whiteFlash?.Flash();
        }
       
        health?.TakeDamage(config.Damagelange*damage);
    }



    protected void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        HandleHit(other);
    }

    public override void OnGet()
    {
        hasHit = false;
        deathType= DeathType.Normal;
        health = new Health(config.MaxHp);
        health.OnDeath += HandleDeath;
    }
    
    public override void OnRelease()
    {
        hasHit = true;
        
        if (health != null)
        {
            health.OnDeath -= HandleDeath;
            health = null;
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

    protected virtual void HandleHit(Collider collider)
    {
        hasHit = true;
        deathType = DeathType.SelfDestruct;
        health?.TakeDamage(9999);
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



