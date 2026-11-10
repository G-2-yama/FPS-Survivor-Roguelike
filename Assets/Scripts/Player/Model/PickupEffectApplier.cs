using UnityEngine;

/// <summary>即時効果と時間付き効果を、それぞれの適用先へ振り分ける。</summary>
public static class PickupEffectApplier
{
    public static bool Apply(Player player, PickupEffectDefinition definition)
    {
        if (player == null || definition == null)
        {
            Debug.LogError("取得効果のプレイヤーまたはSOが未設定です。", player);
            return false;
        }
        if (!definition.TryValidate(out string error))
        {
            Debug.LogError($"{definition.name}: {error}", definition);
            return false;
        }
        if (definition.Mode == PickupEffectMode.Timed)
        {
            if (player.TimedBuffManager == null)
            {
                Debug.LogError("TimedBuffManagerが未設定です。", player);
                return false;
            }
            player.TimedBuffManager.AddBuff(new PickupTimedBuff(player.Stats, definition));
        }
        else foreach (var effect in definition.Effects)
        {
            switch (effect.Kind)
            {
                case PickupEffectKind.Heal: player.Health.Heal(Mathf.RoundToInt(effect.Amount)); break;
                case PickupEffectKind.Experience: player.AddExp(effect.Amount); break;
                default: ApplyStat(player.Stats, effect, 1f); break;
            }
        }
        return true;
    }

    internal static void ApplyStat(PlayerStats stats, PickupEffectEntry effect, float sign)
    {
        float amount = effect.Amount * sign;
        switch (effect.Kind)
        {
            case PickupEffectKind.WalkSpeed: stats.AddWalkSpeed(amount); break;
            case PickupEffectKind.RunSpeed: stats.AddRunSpeed(amount); break;
            case PickupEffectKind.JumpForce: stats.AddJumpForce(amount); break;
        }
    }
}