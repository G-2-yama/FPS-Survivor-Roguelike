using UnityEngine;

public class basicenemyprojectile : enemyplojectileobject
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private TeamType targetTeam = TeamType.None;
    protected override void HandleHit(Collider col)
    {
        
        if (col.TryGetComponent(out IDamageable damageable))
        {

            if ((damageable.TeamType & targetTeam) == 0)
            {
                base.HandleHit(col);
                return; }
            damageable.TakeDamage(damage);
        }
        base.HandleHit(col);

        
    }
}
