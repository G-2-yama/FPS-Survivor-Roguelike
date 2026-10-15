using UnityEngine;
using System.Collections;

/// <summary>ヒットスキャンを行い、最初の有効な対象にダメージを与える</summary>
[CreateAssetMenu(menuName = "Weapons/FireMode/HitScan")]
public class HitScanData : FireModeData
{
    [SerializeField] private float maxRange = 60f;
    [SerializeField] private float hitRadius = 0.1f;
    [SerializeField] private GameObject tracerPrefab;
    [SerializeField] private TeamType targetTeam = TeamType.Enemy;

    /// <summary>射線上を検索し、プレイヤー自身の接触判定を除外する</summary>
    /// <param name="weapon">ダメージ値とトレーサー設定を持つ武器</param>
    /// <param name="weaponOwner">武器の能力補正を受けるプレイヤー</param>
    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Vector3 direction = GetFireDirection(weapon);
        Ray ray = new Ray(weapon.AimTransform.position, direction);
        Vector3 endPoint = ray.origin + direction * maxRange;

        Debug.DrawRay(ray.origin, ray.direction * maxRange, Color.red, 1f);

        RaycastHit[] hits = Physics.SphereCastAll(ray, hitRadius, maxRange);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        int damage = weapon.GetDamageAmount();
        float knockback = weapon.GetKnockbackForce();

        foreach (var hit in hits)
        {
            // プレイヤー所有の押し出し判定を飛ばし、その先の対象も検索する
            if (AttackCollisionFilter.ShouldIgnore(hit.collider))
                continue;

            if (DamageTargetRegistry.TryResolve(hit.collider, out IDamageable damageable))
            {
                if ((damageable.TeamType & targetTeam) == 0)
                {
                    continue;
                }
                endPoint = hit.point;

                TryEnableHitEffect(hit.point);

                damageable.TakeDamage(damage, knockback);

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
            PooledInstance tracer = PoolManager.Instance.Rent(tracerPrefab);

            tracer.RequireTracer().Initialize(weapon.transform.position, endPoint, 0.1f);
        }

    }
}
