using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class Enemy : PoolableObject, IDamageable
{
    [System.Serializable]
    public class DropData
    {
        public GameObject prefab;
        [Min(1)]
        public int weight = 1;
    }
    [SerializeField] private EnemyConfig config;
    [SerializeField] private EnemyAttackController attackController;
    private GameObject item;
    [SerializeField] private WhiteFlash whiteFlash;
    [SerializeField] private EnemyBrain enemyBrain;
    Transform Target;


    [SerializeField] private float deathDelay = 0.1f;

    [SerializeField] private List<DropData> dropList = new();

    public TeamType TeamType => TeamType.Enemy;
    public Health Health { get; private set; }

    private bool isDead = false;


    public void TakeDamage(int damage, float knockbackForce)
    {
        // Damage処理
        if (Health == null || isDead)
            return;


        if (Health.CurrentHP > 0)
        {
            whiteFlash?.Flash();
        }

        Health.TakeDamage(config.Damagelange * damage);

        // Knockback処理
        Target = enemyBrain.Target;
        if (Target != null)
        {
            Vector3 dir = transform.position - Target.position;

            enemyBrain.KnockbackState.SetKnockback(dir, knockbackForce);

            enemyBrain.ChangeState(enemyBrain.KnockbackState);
        }
    }

    public override void OnGet()
    {
        isDead = false;

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
        if (isDead) return;

        isDead = true;

        StartCoroutine(DeathRoutine());
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


