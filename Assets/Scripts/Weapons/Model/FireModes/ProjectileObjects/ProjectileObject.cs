using UnityEngine;
using System.Collections;
/// <summary>
/// 弾の体力、寿命、衝突検知とプール管理の共通処理を提供する
/// </summary>
public abstract class ProjectileObject : PoolableObject, IDamageable
{
    [SerializeField] protected TeamType myTeam = TeamType.None;
    public virtual TeamType TeamType => myTeam;

    [SerializeField] protected TeamType targetTeam = TeamType.None;
    [SerializeField] protected int HP = 1;
    protected System.Action<Collider> onHit;
    private Coroutine lifeRoutine;
    protected Rigidbody rb;

    protected int damage;
    protected float knockbackForce;

    protected Health health;

    /// <summary>Projectileの体力にダメージを適用する</summary>
    public void TakeDamage(int damage, float knockbackForce)
    {
        health.TakeDamage(damage);
    }

    /// <summary>
    /// 二重ヒット防止フラグ
    /// </summary>
    protected bool hasHit = false;

    /// <summary>ProjectileのRigidbodyを取得する</summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>ダメージ値と生存時間を初期化する</summary>
    /// <param name="damage">有効な対象に与えるダメージ</param>
    /// <param name="knockbackForce">有効な対象へ渡すノックバック量</param>
    /// <param name="lifeTime">Projectileを解放するまでの秒数</param>
    public void Initialize(int damage, float knockbackForce, float lifeTime)
    {
        this.damage = damage;
        this.knockbackForce = knockbackForce;
        health = new Health(HP);
        health.OnDeath += OnRelease;
        lifeRoutine = StartCoroutine(LifeTimer(lifeTime));
    }

    /// <summary>指定時間後にProjectileを解放する</summary>
    /// <param name="time">解放まで待機する秒数</param>
    private IEnumerator LifeTimer(float time)
    {
        yield return new WaitForSeconds(time);
        Release();
    }

    /// <summary>
    /// 物理衝突時にヒット処理を行う
    /// </summary>
    /// <param name="collision">衝突情報</param>
    protected void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        // プレイヤー所有の押し出し領域は敵との距離調整用のため、弾の対象から除外する
        if (AttackCollisionFilter.ShouldIgnore(collision.collider))
            return;

        HandleHit(collision.collider);
    }


    /// <summary>
    /// Trigger侵入時にヒット処理を行う
    /// </summary>
    /// <param name="other">接触したCollider</param>
    protected void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        // プレイヤー自身の接触判定だけを除外し、Projectileは解放しない
        if (AttackCollisionFilter.ShouldIgnore(other))
            return;

        HandleHit(other);
    }

    /// <summary>
    /// 具体的な弾の種類に応じた命中処理を実行する
    /// </summary>
    /// <param name="collider">ヒットしたCollider</param>
    protected abstract void HandleHit(Collider collider);

    /// <summary>プールから取得された際にヒット状態を初期化する</summary>
    public override void OnGet()
    {
        hasHit = false;
        rb.linearVelocity = Vector3.zero;
    }

    /// <summary>プールへ戻す前に生存時間処理を停止し、一時状態を消去する</summary>
    public override void OnRelease()
    {
        hasHit = true;

        if (lifeRoutine != null)
        {
            StopCoroutine(lifeRoutine);
            lifeRoutine = null;
        }

        rb.linearVelocity = Vector3.zero;
        onHit = null;
    }
}
