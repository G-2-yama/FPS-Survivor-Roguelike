#if UNITY_EDITOR
using System;
using UnityEngine;

/// <summary>攻撃対象の登録とプール再利用を検証する専用対象</summary>
[RequireComponent(typeof(DamageTargetBinding))]
public sealed class CombatDamageProbe : PoolableObject, IDamageable
{
    public TeamType TeamType => TeamType.Enemy;
    public int ReceivedDamage { get; private set; }
    public int GetCount { get; private set; }
    public int ReleaseCount { get; private set; }
    public Action OnDamageApplied { get; set; }

    /// <summary>検証中に適用されたダメージを記録する</summary>
    public void TakeDamage(int damage, float knockbackForce = 0f)
    {
        ReceivedDamage += damage;
        OnDamageApplied?.Invoke();
    }

    /// <summary>新しい活動期間と取得回数を記録する</summary>
    public override void OnGet()
    {
        GetCount++;
        ReceivedDamage = 0;
        OnDamageApplied = null;
    }

    /// <summary>返却コールバックの回数を記録する</summary>
    public override void OnRelease() => ReleaseCount++;
}
#endif
