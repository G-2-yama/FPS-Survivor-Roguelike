using UnityEngine;
using System.Collections;

public class EnemyHealth : PoolableObject, IDamageable
{
    [SerializeField] private EnemyConfig config;
    [SerializeField] private EnemyAttackController attackController;
    [SerializeField] private GameObject prefab;
    [SerializeField] private WhiteFlash whiteFlash;
    [SerializeField]
    private EnemyBrain enemyBrain;
   Transform Target;
      

    [SerializeField] private float deathDelay = 0.1f;
   
    public TeamType TeamType => TeamType.Enemy;
    public Health Health { get; private set; }

    private bool isDead = false;

    
    public void TakeDamage(int damage)
    {
        if (Health == null || isDead)
            return;


        if (Health.CurrentHP > 0)
        {
            whiteFlash?.Flash();
        }
        

        Health.TakeDamage(config.Damagelange * damage);
        /*Target = enemyBrain.Target;
        if (Target != null)
        {
            Vector3 dir =
            transform.position -
            Target.position;

            enemyBrain.KnockbackState.SetKnockback(
                dir,
                8f,
                0.2f);

            enemyBrain.ChangeState(
                enemyBrain.KnockbackState);
        }*/
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

        GameObject expitem = PoolManager.Instance.Get(prefab);
        expitem.transform.position = transform.position;

        Release();
    }
}