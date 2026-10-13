using UnityEngine;
/// <summary>命中した登録済み対象へのダメージと貫通を処理する</summary>

public class NormalProjectile : ProjectileObject
{
    /// <summary>保持済みの登録情報を使って対象への命中を処理する</summary>
    protected override void HandleHit(Collider collider)
    {

        if (DamageTargetRegistry.TryResolve(collider, out IDamageable damageable))
        {
            if ((damageable.TeamType & targetTeam) == 0)
            {
                return;
            }

            damageable.TakeDamage(damage, knockbackForce);

            // 敵の弾丸の場合は貫通
            if(damageable.TeamType != TeamType.EnemyAmmo)
            {
                Release();
                hasHit = true;
            }
        }
        else
        {
            // 壁などに当たった場合
            hasHit = true;
            Release();
        }
    }
}