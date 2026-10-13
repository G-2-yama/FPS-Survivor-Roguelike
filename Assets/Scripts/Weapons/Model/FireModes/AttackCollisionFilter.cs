using UnityEngine;

/// <summary>攻撃方式に共通する補助Colliderの除外ルールを提供する</summary>
public static class AttackCollisionFilter
{
    /// <summary>命中や弾の解放を行わず通過させるColliderか判定する</summary>
    public static bool ShouldIgnore(Collider collider) =>
        collider != null && collider.TryGetComponent<AttackIgnoredCollider>(out _);
}
