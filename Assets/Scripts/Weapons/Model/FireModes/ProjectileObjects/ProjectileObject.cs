using UnityEngine;
using System.Collections;
/// <summary>
/// 発射されたProjectileの衝突を検知し、ヒット時コールバックを実行して自身を破棄するコンポーネント。
/// </summary>
public abstract class ProjectileObject : PoolableObject
{
    protected System.Action<Collider> onHit;
    private Coroutine lifeRoutine;
    protected Rigidbody rb;



    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    /// <summary>
    /// ヒット時に呼び出す処理と生存時間を初期化する
    /// </summary>
    /// <param name="onHitAction">衝突したColliderを受け取るヒット時コールバック</param>
    /// <param name="lifeTime">自動破棄までの秒数</param>
    public void Initialize(System.Action<Collider> onHitAction, float lifeTime)
    {
        onHit = onHitAction;
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
    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.collider);
    }

    /// <summary>
    /// Trigger侵入時にヒット処理を行う
    /// </summary>
    /// <param name="other">接触したCollider</param>
    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    /// <summary>
    /// ヒットコールバックを実行し、Projectileを破棄する
    /// </summary>
    /// <param name="col">ヒットしたCollider</param>
    protected abstract void HandleHit(Collider col);

    public override void OnGet()
    {
        rb.linearVelocity = Vector3.zero;
    }

    public override void OnRelease()
    {
        if (lifeRoutine != null)
        {
            StopCoroutine(lifeRoutine);
            lifeRoutine = null;
        }

        rb.linearVelocity = Vector3.zero;
        onHit = null;
    }
}
