using System.Collections.Generic;
using UnityEngine;


/// <summary>プレイヤーの最大HPを強化し、必要に応じて解除するアイテム</summary>
[CreateAssetMenu(menuName = "Items/healthUp")]
public class HealthUpItem : Item
{
    [SerializeField, Min(0), Tooltip("レベル順の最大HPと現在HPへの加算量（合計値）")]
    private int[] healthIncreasePerLevel = { 200 };

    public override int MaxLevel => healthIncreasePerLevel.Length;

    /// <summary>最大HPを強化する</summary>
    public override void Apply(Player player, int level)
    {
        player.Stats.AddMaxHP(ValueAt(healthIncreasePerLevel, level));
    }

    /// <summary>適用した最大HP強化を解除する</summary>
    public override void Revert(Player player, int level)
    {
        player.Stats.AddMaxHP(-ValueAt(healthIncreasePerLevel, level));
    }

    public override IReadOnlyList<LevelChange> GetLevelChanges(int level)
    {
        var changes = new List<LevelChange>();
        if (level >= 0 && level + 1 < healthIncreasePerLevel.Length)
        {
            changes.Add(new LevelChange("最大体力",
                LevelChange.Signed(healthIncreasePerLevel[level]),
                LevelChange.Signed(healthIncreasePerLevel[level + 1])));
        }
        return changes;
    }
}
