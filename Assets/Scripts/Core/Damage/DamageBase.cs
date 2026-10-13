using UnityEngine;

public abstract class DamageBase : PoolableObject, IDamageable, ILifetimeOwner
{
    [SerializeField] protected TeamType myTeam;
    public TeamType MyTeam => myTeam;
    [SerializeField] protected TeamType targetTeam;
    public TeamType TargetTeam => targetTeam;

    [SerializeField] protected int damage = 10;
    [SerializeField] protected float knockbackForce = 5f;

    [SerializeField] protected float lifetime = 0f;
    public float Lifetime => lifetime;

    [SerializeField] protected int HP = 1;

    protected Health health;

    protected bool isReleased;

    public int LifetimeIndex { get; set; } = -1;

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
            LifetimeRunner.Register(this, lifetime);
    }

    public override void OnRelease()
    {
        isReleased = true;
    }

    public void OnLifetimeExpired()
    {
        if (!isReleased)
            Release();
    }

    private void OnDisable()
    {
        LifetimeRunner.Unregister(this);
    }
}