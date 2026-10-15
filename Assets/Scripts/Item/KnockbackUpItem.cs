using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Items/knockbackUp")]
public class KnockbackUpItem : Item
{
    [Tooltip("レベル順のノックバック倍率の加算量（合計値）")]
    [SerializeField] private float[] knockbackIncreasePerLevel = { 0.1f };

    public override int MaxLevel => knockbackIncreasePerLevel.Length;

    public override void Apply(Player player, int level)
    {
        player.Stats.AddKnockbackForceMultiplier(ValueAt(knockbackIncreasePerLevel, level));
    }

    public override void Revert(Player player, int level)
    {
        player.Stats.AddKnockbackForceMultiplier(-ValueAt(knockbackIncreasePerLevel, level));
    }

    public override IReadOnlyList<LevelChange> GetLevelChanges(int level)
    {
        var changes = new List<LevelChange>();
        if (level >= 0 && level + 1 < knockbackIncreasePerLevel.Length)
        {
            changes.Add(new LevelChange("ノックバック",
                LevelChange.SignedPercent(knockbackIncreasePerLevel[level]),
                LevelChange.SignedPercent(knockbackIncreasePerLevel[level + 1])));
        }
        return changes;
    }
}
