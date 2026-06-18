using UnityEngine;

public class ballprojectile : enemyplojectileobject
{
    [SerializeField] private GameObject prefab;
    
    protected override void HandleHit(Collider collider)
    {
        if (collider.TryGetComponent(out IDamageable damageable))
        {
            if ((damageable.TeamType & targetTeam) == 0)
            {
                return;
            }
            hasHit = true;
            GameObject area = PoolManager.Instance.Get(prefab);
            area.transform.position = transform.position;
            Release();
        }
    }
}
