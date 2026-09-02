using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class enemyplojectileobject : PoolableObject, IDamageable
{
    [System.Serializable]
    public class DropData
    {
        public GameObject prefab;
        [Min(1)]
        public int weight = 1;
    }
    private DeathType deathType = DeathType.Normal;
    [SerializeField] private List<DropData> dropList = new();
    [SerializeField] protected EnemyConfig config;
    [SerializeField] private WhiteFlash whiteFlash;
    [SerializeField] private float deathDelay = 0.1f;
    [SerializeField] private EnemyBrain enemyBrain;
    protected Health health;
    public TeamType TeamType => TeamType.EnemyAmmo;
    public TeamType targetTeam => TeamType.Player;

    protected bool hasHit = false;
    protected int damage;
    protected float knockbackForce;
    public void Initialize(int damage, float knockbackForce)
    {
        this.damage = damage;
        this.knockbackForce = knockbackForce;
      
    }


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
        if (enemyBrain != null)
        {
            enemyBrain.ResetBrain();
        }

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

        GameObject prefab = GetRandomDrop();
        if (prefab != null)
        {
            GameObject item = PoolManager.Instance.Get(prefab);
            item.transform.position = transform.position;
            item.transform.rotation = transform.rotation;
        }
        Release();
    }

    private GameObject GetRandomDrop()
    {
        if (dropList.Count == 0)
            return null;

        int totalWeight = 0;

        foreach (var drop in dropList)
            totalWeight += drop.weight;

        int rand = Random.Range(0, totalWeight);

        foreach (var drop in dropList)
        {
            if (rand < drop.weight)
                return drop.prefab;

            rand -= drop.weight;
        }

        return null;
    }
}



