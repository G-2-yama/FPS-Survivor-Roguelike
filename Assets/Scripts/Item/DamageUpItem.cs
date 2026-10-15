using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Items/damageUp")]
public class DamageUpItem : Item
{
    [Tooltip("レベル順のダメージ倍率の加算量（合計値）")]
    [SerializeField] private float[] damageIncreasePerLevel = { 0.1f };

    public override int MaxLevel => damageIncreasePerLevel.Length;

    public override void Apply(Player player, int level)
    {
        player.Stats.AddDamageMultiplier(ValueAt(damageIncreasePerLevel, level));
    }

    public override void Revert(Player player, int level)
    {
        player.Stats.AddDamageMultiplier(-ValueAt(damageIncreasePerLevel, level));
    }

    public override IReadOnlyList<LevelChange> GetLevelChanges(int level)
    {
        var changes = new List<LevelChange>();
        if (level >= 0 && level + 1 < damageIncreasePerLevel.Length)
        {
            changes.Add(new LevelChange("ダメージ",
                LevelChange.SignedPercent(damageIncreasePerLevel[level]),
                LevelChange.SignedPercent(damageIncreasePerLevel[level + 1])));
        }
        return changes;
    }
}
