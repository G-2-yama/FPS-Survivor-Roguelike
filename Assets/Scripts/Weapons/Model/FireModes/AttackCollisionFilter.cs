using System.Collections.Generic;
using UnityEngine;

/// <summary>攻撃方式に共通する補助Colliderの除外ルールを提供する</summary>
public static class AttackCollisionFilter
{
    private static readonly HashSet<Collider> ignoredColliders = new();

    /// <summary>プレイ開始時に前回の攻撃除外対象を破棄する</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => ignoredColliders.Clear();

    /// <summary>攻撃判定から除外するColliderを登録する</summary>
    public static void Register(Collider collider)
    {
        if (collider != null) ignoredColliders.Add(collider);
    }

    /// <summary>攻撃判定から除外するColliderの登録を解除する</summary>
    public static void Unregister(Collider collider)
    {
        if (collider != null) ignoredColliders.Remove(collider);
    }

    /// <summary>登録済みのColliderを攻撃判定から除外するか判定する</summary>
    public static bool ShouldIgnore(Collider collider) =>
        collider != null && ignoredColliders.Contains(collider);
}
