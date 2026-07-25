using UnityEngine;

public class basicenemyprojectile : enemyplojectileobject
{
    protected override void HandleHit(Collider collider)
    { 
        if (collider.TryGetComponent(out IDamageable damageable))
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
