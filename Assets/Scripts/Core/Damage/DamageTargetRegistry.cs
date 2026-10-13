using System.Collections.Generic;
using UnityEngine;

/// <summary>命中したColliderから初期化済みのダメージ対象を解決する</summary>
public static class DamageTargetRegistry
{
    private static readonly Dictionary<Collider, DamageTargetBinding> targets = new();

    /// <summary>プレイ開始時に前回の登録を破棄する</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => targets.Clear();

    /// <summary>対象のColliderを登録する</summary>
    internal static void Register(Collider collider, DamageTargetBinding binding)
    {
        if (collider != null) targets[collider] = binding;
    }

    /// <summary>自身が所有する登録だけを解除する</summary>
    internal static void Unregister(Collider collider, DamageTargetBinding binding)
    {
        if (collider != null && targets.TryGetValue(collider, out var current) && current == binding)
            targets.Remove(collider);
    }

    /// <summary>登録済みで有効なColliderの対象を返す</summary>
    public static bool TryResolve(Collider collider, out IDamageable target)
    {
        target = null;
        if (!TryResolveBinding(collider, out var binding)) return false;
        target = binding.Target;
        return target != null;
    }

    /// <summary>接触終了の通知元を保持する処理へ登録情報を返す</summary>
    public static bool TryResolveBinding(Collider collider, out DamageTargetBinding binding)
    {
        binding = null;
        if (collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy
            || !targets.TryGetValue(collider, out binding) || binding == null || !binding.IsRegistered
            || binding.Target == null)
            return false;
        return true;
    }
}
