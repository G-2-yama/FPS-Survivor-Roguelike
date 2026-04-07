using UnityEngine;

/// <summary>
/// 発射されたProjectileの衝突判定とダメージ適用を担当するコンポーネント。
/// </summary>
public class ProjectileObject : MonoBehaviour
{
    private Weapon ownerWeapon;
    private FireModeData fireModeData;
    private bool isInitialized;
    private bool hasHit;

    /// <summary>
    /// Projectileのダメージ量と寿命を初期化する。
    /// </summary>
    /// <param name="weapon">Projectileを発射した武器。</param>
    /// <param name="modeData">ダメージ適用に使用するFireModeデータ。</param>
    /// <param name="lifeTime">自動破棄までの秒数。</param>
    public void Initialize(Weapon weapon, FireModeData modeData, float lifeTime)
    {
        ownerWeapon = weapon;
        fireModeData = modeData;
        isInitialized = true;

        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// 物理衝突時にヒット判定を処理する
    /// </summary>
    /// <param name="collision">衝突情報。</param>
    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.collider);
    }

    /// <summary>
    /// Trigger侵入時にヒット判定を処理する
    /// </summary>
    /// <param name="other">接触したコライダー。</param>
    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    /// <summary>
    /// ダメージ対象を探索してダメージを適用し、Projectileを破棄する
    /// </summary>
    /// <param name="hitCollider">ヒットしたコライダー。</param>
    private void HandleHit(Collider hitCollider)
    {
        if (hasHit || !isInitialized || hitCollider == null)
        {
            return;
        }

        hasHit = true;

        fireModeData.TryApplyDamage(ownerWeapon, hitCollider);

        Destroy(gameObject);
    }
}
