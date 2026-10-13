using UnityEngine;
/// <summary>プレイヤーへの命中ダメージを処理する</summary>

public class basicenemyprojectile : enemyplojectileobject
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

            damageable.TakeDamage(config.Damagelange*damage);
        }
        hasHit = true;
        Release();
    }
}
