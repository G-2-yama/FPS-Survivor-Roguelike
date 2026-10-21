using UnityEngine;
/// <summary>攻撃個体のダメージと体力と寿命を管理する</summary>

[RequireComponent(typeof(DamageTargetBinding))]
public abstract class DamageBase : PoolableObject, IDamageable, ILifetimeOwner, IInitialize
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
    /// <summary>取得した個体へ今回の攻撃設定を渡す</summary>

    public virtual void Initialize(int damage, float knockback)
    {
        this.damage = damage;
        this.knockbackForce = knockback;
    }
    /// <summary>受けたダメージを個体の体力へ適用する</summary>

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
    /// <summary>登録済みの対象へチーム条件を確認してダメージを適用する</summary>

    protected bool TryDamage(Collider other, out IDamageable target)
    {
        target = null;

        if (!DamageTargetRegistry.TryResolve(other, out target))
            return false;

        if ((target.TeamType & targetTeam) == 0)
        {
            target = null;
            return false;
        }

        target.TakeDamage(damage, knockbackForce);
        return true;
    }
    /// <summary>新しい活動期間の状態を準備する</summary>

    public override void OnGet()
    {
        isReleased = false;

        health = new Health(HP);

        if (lifetime > 0)
            LifetimeRunner.Register(this, lifetime);
    }
    /// <summary>返却する個体の一時状態と購読を解除する</summary>

    public override void OnRelease()
    {
        isReleased = true;
    }
    /// <summary>寿命が終了した個体をプールへ返却する</summary>

    public void OnLifetimeExpired()
    {
        if (!isReleased)
            Release();
    }
    /// <summary>無効化された個体の登録と一時状態を解除する</summary>

    protected virtual void OnDisable()
    {
        LifetimeRunner.Unregister(this);
    }
}