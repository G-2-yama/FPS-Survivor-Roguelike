using UnityEngine;
using System.Collections;

public abstract class DamageBase : PoolableObject, IDamageable
{
    [SerializeField] protected TeamType myTeam;
    public TeamType MyTeam => myTeam;
    [SerializeField] protected TeamType targetTeam;
    public TeamType TargetTeam => targetTeam;

    [SerializeField] protected int damage = 10;
    [SerializeField] protected float knockbackForce = 5f;

    [SerializeField] protected float lifetime = 0f;

    [SerializeField] protected int HP = 1;

    protected Health health;

    private Coroutine lifeRoutine;

    protected bool isReleased;

    public TeamType TeamType => myTeam;

    public virtual void Initialize(int damage, float knockback)
    {
        this.damage = damage;
        this.knockbackForce = knockback;
    }

    public void TakeDamage(int damage, float knockback)
    {
        if (isReleased)
            return;
            
        health.TakeDamage(damage);

        if (health.IsDead)
        {
            isReleased = true;
            Release();
        }

    }

    protected bool TryDamage(Collider other, out IDamageable target)
    {
        target = null;

        if (!other.TryGetComponent(out target))
            return false;

        if ((target.TeamType & targetTeam) == 0)
        {
            target = null;
            return false;
        }

        target.TakeDamage(damage, knockbackForce);
        return true;
    }

    public override void OnGet()
    {
        isReleased = false;

        health = new Health(HP);

        if (lifetime > 0)
            lifeRoutine = StartCoroutine(LifeTimer(lifetime));
    }

    public override void OnRelease()
    {
        isReleased = true;

        if (lifeRoutine != null)
        {
            StopCoroutine(lifeRoutine);
            lifeRoutine = null;
        }
    }

    IEnumerator LifeTimer(float time)
    {
        yield return new WaitForSeconds(time);

        if (!isReleased)
            Release();
    }
}