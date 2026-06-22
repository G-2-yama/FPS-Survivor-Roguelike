using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Weapons/FireMode/HitScan")]
public class HitScanData : FireModeData
{
    [SerializeField] private float maxRange = 60f;
    [SerializeField] private float hitRadius = 0.1f;
    [SerializeField] private GameObject tracerPrefab;
    [SerializeField] private TeamType targetTeam = TeamType.Enemy;

    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Vector3 direction = GetFireDirection(weapon);
        Ray ray = new Ray(Camera.main.transform.position, direction);
        Vector3 endPoint = ray.origin + direction * maxRange;

        Debug.DrawRay(ray.origin, ray.direction * maxRange, Color.red, 1f);
        
        RaycastHit[] hits = Physics.SphereCastAll(ray, hitRadius, maxRange);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent(out IDamageable damageable))
            {
                if ((damageable.TeamType & targetTeam) == 0)
                {
                    continue;
                }
                endPoint = hit.point;

                TryEnableHitEffect(hit.point);

                ApplyDamage(weapon, hit.collider, weaponOwner);

                if(damageable.TeamType != TeamType.EnemyAmmo)
                {
                    break;
                }
            }
            else
            {
                TryEnableHitEffect(hit.point);
                break;
            }
        }
        
        if (tracerPrefab != null)
        {
            GameObject tracer = PoolManager.Instance.Get(tracerPrefab);

            tracer.GetComponent<Tracer>().Initialize(weapon.transform.position, endPoint, 0.1f);
        }
        
    }

    private void ApplyDamage(Weapon weapon, Collider hitCollider, Player weaponOwner)
    {
        if(hitCollider.TryGetComponent(out IDamageable damageable))
        {
            if ((damageable.TeamType & targetTeam) == 0)
            {
                return;
            }
            damageable.TakeDamage(GetDamageAmount(weapon, weaponOwner), GetKnockbackForce(weapon, weaponOwner));
        }
    }
}