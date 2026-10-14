using System.Collections.Generic;
using UnityEngine;

/// <summary>ヒットスキャンを行い、近くの敵へ連鎖してダメージを与える</summary>
[CreateAssetMenu(menuName = "Weapons/FireMode/ChainFire")]
public class ChainFire : FireModeData
{
    [SerializeField] private float maxRange = 60f;
    [SerializeField] private float hitRadius = 0.1f;
    [SerializeField] private TeamType targetTeam = TeamType.Enemy;

    [Header("Chain Settings")]
    [SerializeField] private float chainRange = 10f;
    [SerializeField] private int chainCount = 3;

    /// <summary>最初の有効な対象を探し、連鎖ダメージを適用する</summary>
    /// <param name="weapon">ダメージ値を持つ武器</param>
    /// <param name="weaponOwner">武器の能力補正を受けるプレイヤー</param>
    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        List<Collider> hitTargets = new();

        Vector3 direction = GetFireDirection(weapon);
        Ray ray = new Ray(weapon.AimTransform.position, direction);

        RaycastHit[] hits = Physics.SphereCastAll(ray, hitRadius, maxRange);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            // プレイヤー自身の接触判定を飛ばし、その先のヒット候補を調べる
            if (AttackCollisionFilter.ShouldIgnore(hit.collider))
                continue;

            if (!DamageTargetRegistry.TryResolve(hit.collider, out IDamageable damageable))
            {
                TryEnableHitEffect(hit.point);
                break;
            }

            if ((damageable.TeamType & targetTeam) == 0)
                continue;

            TryEnableHitEffect(hit.collider.transform.position);
            ApplyDamage(weapon, damageable, weaponOwner);

            if (damageable.TeamType == TeamType.EnemyAmmo)
                continue;

            hitTargets.Add(hit.collider);
            ChainDamage(hit.collider, weapon, weaponOwner, hitTargets);

            break;
        }
    }

    /// <summary>現在の対象の近くにいる敵を探し、追加でダメージを与える</summary>
    /// <param name="startTarget">連鎖の起点となる対象</param>
    /// <param name="weapon">ダメージ値を持つ武器</param>
    /// <param name="weaponOwner">強化効果を適用するプレイヤー</param>
    /// <param name="hitTargets">連鎖中に処理済みのCollider</param>
    private void ChainDamage(Collider startTarget, Weapon weapon, Player weaponOwner, List<Collider> hitTargets)
    {
        Collider currentTarget = startTarget;

        for (int i = 0; i < chainCount; i++)
        {
            Collider nextTarget = DamageTargetSearch.FindNearest(
                currentTarget.transform.position, chainRange, targetTeam & ~TeamType.EnemyAmmo, out IDamageable damageable, hitTargets);

            if (nextTarget == null)
                break;

            hitTargets.Add(nextTarget);

            TryEnableHitEffect(nextTarget.transform.position);

            ApplyDamage(weapon, damageable, weaponOwner);

            currentTarget = nextTarget;
        }
    }

    /// <summary>武器能力を反映したダメージを連鎖対象へ適用する</summary>
    /// <param name="weapon">基礎ダメージとノックバック量を持つ武器</param>
    /// <param name="damageable">連鎖先として選択されたダメージ対象</param>
    /// <param name="weaponOwner">強化効果を適用するプレイヤー</param>
    private void ApplyDamage(Weapon weapon, IDamageable damageable, Player weaponOwner)
    {
        damageable.TakeDamage(GetDamageAmount(weapon, weaponOwner));
    }
}
