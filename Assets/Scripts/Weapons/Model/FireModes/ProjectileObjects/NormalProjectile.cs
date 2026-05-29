using UnityEngine;

public class NormalProjectile : ProjectileObject
{
    [SerializeField] private TeamType targetTeam = TeamType.None;
    protected override void HandleHit(Collider col)
    {
        if (col.TryGetComponent(out IDamageable damageable))
        {
            if ((damageable.TeamType & targetTeam) == 0) return;
            damageable.TakeDamage(damage);
        }

        Release();
    }
}