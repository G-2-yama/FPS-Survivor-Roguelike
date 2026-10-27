using UnityEngine;

/// <summary>通常の強化候補を取り切った後に、繰り返し選べる回復候補。</summary>
[CreateAssetMenu(fileName = "RecoveryUpgrade", menuName = "Upgrade/Recovery")]
public class RecoveryUpgrade : UpgradeBase
{
    public enum RecoveryMode
    {
        FixedAmount,
        MaxHealthPercentage
    }

    [Header("Recovery")]
    [SerializeField] private RecoveryMode recoveryMode = RecoveryMode.MaxHealthPercentage;
    [SerializeField, Min(1)] private int healAmount = 300;
    [SerializeField, Range(1f, 100f)] private float maxHealthPercentage = 30f;

    [Header("Availability")]
    [Tooltip("候補プールにある回復以外のアップグレードを登録する。すべて選択不可になると回復を表示する。")]
    [SerializeField] private UpgradeBase[] normalUpgrades;

    public override bool IsAvailable()
    {
        if (player == null || player.Health == null || player.Health.IsDead
            || normalUpgrades == null || normalUpgrades.Length == 0)
            return false;

        foreach (var upgrade in normalUpgrades)
        {
            // 回復候補同士を参照しても、再帰的な判定を行わない。
            if (upgrade == null || upgrade is RecoveryUpgrade)
                continue;

            upgrade.Initialize(player);
            if (upgrade.IsAvailable())
                return false;
        }

        int amount = GetHealAmount();
        description = recoveryMode == RecoveryMode.MaxHealthPercentage
            ? $"HPを{amount}回復する（最大HPの{Mathf.Clamp(maxHealthPercentage, 1f, 100f):0.##}％）"
            : $"HPを{amount}回復する";

        // 満タンでも選択できるようにして、強化画面が進行不能になることを防ぐ。
        return true;
    }

    public override void Apply()
    {
        if (player == null || player.Health == null || player.Health.IsDead)
            return;

        player.Health.Heal(GetHealAmount());
    }

    private int GetHealAmount()
    {
        return recoveryMode == RecoveryMode.MaxHealthPercentage
            ? Mathf.Max(1, Mathf.CeilToInt(player.Health.MaxHP * (Mathf.Clamp(maxHealthPercentage, 1f, 100f) / 100f)))
            : Mathf.Max(1, healAmount);
    }
}
