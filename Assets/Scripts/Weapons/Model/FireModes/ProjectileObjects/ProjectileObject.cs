using UnityEngine;
using System.Collections;
/// <summary>
/// 発射されたProjectileの衝突を検知し、ヒット時コールバックを実行して自身を破棄するコンポーネント。
/// </summary>
public abstract class ProjectileObject : PoolableObject
{
    [SerializeField] protected TeamType targetTeam = TeamType.None;
    protected System.Action<Collider> onHit;
    private Coroutine lifeRoutine;
    protected Rigidbody rb;

    protected int damage;

    /// <summary>
    /// 二重ヒット防止フラグ
    /// </summary>
    protected bool hasHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// ヒット時に呼び出す処理と生存時間を初期化する
    /// </summary>
    /// <param name="onHitAction">衝突したColliderを受け取るヒット時コールバック</param>
    /// <param name="lifeTime">自動破棄までの秒数</param>
    public void Initialize(int damage, float lifeTime)
    {
        this.damage = damage;
        lifeRoutine = StartCoroutine(LifeTimer(lifeTime));
    }

    IEnumerator LifeTimer(float time)
    {
        yield return new WaitForSeconds(time);
        Release(); 
    }

    /// <summary>
    /// 物理衝突時にヒット処理を行う
    /// </summary>
    /// <param name="collision">衝突情報</param>
    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;

        HandleHit(collision.collider);
    }


    /// <summary>
    /// Trigger侵入時にヒット処理を行う
    /// </summary>
    /// <param name="other">接触したCollider</param>
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        HandleHit(other);
    }

    /// <summary>
    /// ヒットコールバックを実行し、Projectileを破棄する
    /// </summary>
    /// <param name="col">ヒットしたCollider</param>
    protected abstract void HandleHit(Collider col);

    public override void OnGet()
    {
        hasHit = false;
        rb.linearVelocity = Vector3.zero;
    }

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
