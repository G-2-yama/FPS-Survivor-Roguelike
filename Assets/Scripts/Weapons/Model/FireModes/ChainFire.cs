using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/ChainFire")]
public class ChainFire : FireModeData
{
    [SerializeField] private float maxRange = 60f;

    [Header("連鎖設定")]
    [SerializeField] private float chainRange = 10f;
    [SerializeField] private int chainCount = 3;

    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Vector3 direction = GetFireDirection(weapon);
        Ray ray = new Ray(Camera.main.transform.position, direction);

        Debug.DrawRay(ray.origin, ray.direction * maxRange, Color.red, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, maxRange))
        {
            Collider currentTarget = hit.collider;

            // 既に当たった対象を記録
            HashSet<Collider> hitTargets = new();

            for (int i = 0; i < chainCount; i++)
            {
                if (currentTarget == null)
                    break;

                // ダメージ可能か確認
                IDamageable damageable =
                    currentTarget.GetComponent<IDamageable>();

                if (damageable == null || damageable.TeamType == TeamType.Player)
                    break;

                // ダメージ
                TryApplyDamage(weapon, currentTarget, weaponOwner);

                hitTargets.Add(currentTarget);

                // エフェクト
                TryEnableHitEffect(out GameObject hitEffect);

                if (hitEffect != null)
                {
                    hitEffect.transform.position =
                        currentTarget.transform.position;
                }

                // 次の対象を探す
                currentTarget = FindNearestEnemy(
                    currentTarget,
                    hitTargets);
            }
        }
    }

    /// <summary>
    /// まだヒットしていない最も近いダメージ可能対象を探す
    /// </summary>
    private Collider FindNearestEnemy(Collider originTarget, HashSet<Collider> hitTargets)
    {
        Collider[] colliders =Physics.OverlapSphere( originTarget.transform.position, chainRange);

        Collider nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            // 自分自身やヒット済みを除外
            if (hitTargets.Contains(col))
                continue;

            // ダメージ可能対象のみ
            if (col.GetComponent<IDamageable>() == null)
                continue;

            float distance = Vector3.Distance(originTarget.transform.position, col.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = col;
            }
        }

        return nearest;
    }
}