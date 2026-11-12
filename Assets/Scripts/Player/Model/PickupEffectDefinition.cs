using System;
using System.Collections.Generic;
using UnityEngine;

public enum PickupEffectMode { Instant, Timed }
public enum BuffReacquireMode { Extend, Refresh, Stack }
public enum PickupEffectKind { Heal, Experience, WalkSpeed, RunSpeed, JumpForce }

[Serializable]
public struct PickupEffectEntry
{
    [SerializeField] private PickupEffectKind kind;
    [SerializeField, Min(0f)] private float amount;
    public PickupEffectKind Kind => kind;
    public float Amount => amount;
}

/// <summary>共有する取得効果の設定。適用中の残り時間は保持しない。</summary>
[CreateAssetMenu(menuName = "Items/Pickup Effect Definition")]
public sealed class PickupEffectDefinition : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private PickupEffectMode mode;
    [SerializeField] private PickupEffectEntry[] effects = Array.Empty<PickupEffectEntry>();
    [SerializeField, Min(0.01f)] private float duration = 3f;
    [SerializeField] private BuffReacquireMode reacquireMode = BuffReacquireMode.Extend;

    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public PickupEffectMode Mode => mode;
    public IReadOnlyList<PickupEffectEntry> Effects => effects;
    public float Duration => duration;
    public BuffReacquireMode ReacquireMode => reacquireMode;

    public float GetAmount(PickupEffectKind kind)
    {
        float total = 0f;
        foreach (var effect in effects) if (effect.Kind == kind) total += effect.Amount;
        return total;
    }

    public bool TryValidate(out string error)
    {
        error = null;
        if (!Enum.IsDefined(typeof(PickupEffectMode), mode) || !Enum.IsDefined(typeof(BuffReacquireMode), reacquireMode))
            error = "効果方式または再取得設定が不正です。";
        else if (effects == null || effects.Length == 0) error = "効果を1つ以上設定してください。";
        else if (mode == PickupEffectMode.Timed && (float.IsNaN(duration) || float.IsInfinity(duration) || duration <= 0f))
            error = "持続時間は0より大きい有限値にしてください。";
        else foreach (var effect in effects)
        {
            if (!Enum.IsDefined(typeof(PickupEffectKind), effect.Kind) || float.IsNaN(effect.Amount) || float.IsInfinity(effect.Amount) || effect.Amount < 0f)
                error = "効果の種類または効果量が不正です。";
            else if (mode == PickupEffectMode.Timed && (effect.Kind == PickupEffectKind.Heal || effect.Kind == PickupEffectKind.Experience))
                error = "回復・経験値は即時効果として設定してください。";
            if (error != null) break;
        }
        return error == null;
    }
}