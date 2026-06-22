using UnityEngine;

public class NormalProjectile : ProjectileObject
{
    protected override void HandleHit(Collider collider)
    {

        if (collider.TryGetComponent(out IDamageable damageable))
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