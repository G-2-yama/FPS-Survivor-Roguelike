using UnityEngine;

/// <summary>
/// 発射されたProjectileの衝突を検知し、ヒット時コールバックを実行して自身を破棄するコンポーネント。
/// </summary>
public class ProjectileObject : MonoBehaviour
{
    private System.Action<Collider> onHit;

    /// <summary>
    /// ヒット時に呼び出す処理と生存時間を初期化する
    /// </summary>
    /// <param name="onHitAction">衝突したColliderを受け取るヒット時コールバック</param>
    /// <param name="lifeTime">自動破棄までの秒数</param>
    public void Initialize(System.Action<Collider> onHitAction, float lifeTime)
    {
        onHit = onHitAction;
        Destroy(gameObject, lifeTime);
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
    private void HandleHit(Collider col)
    {
        if (onHit == null) return;

        onHit.Invoke(col);

        Destroy(gameObject);
    }
}
