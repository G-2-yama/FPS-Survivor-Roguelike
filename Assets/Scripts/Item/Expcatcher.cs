using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Items/Expcatcher")]
public class Expcatcher : Item
{
    [Tooltip("レベル順の取得範囲の加算量（合計値）")]
    [SerializeField] private float[] increasePerLevel = { 5f };

    public override int MaxLevel => increasePerLevel.Length;

    public override void Apply(Player player, int level)
    {
        player.Stats.AddPickupRadius(ValueAt(increasePerLevel, level));
    }

    public override void Revert(Player player, int level)
    {
       player.Stats.AddPickupRadius(-ValueAt(increasePerLevel, level));
    }

    public override IReadOnlyList<LevelChange> GetLevelChanges(int level)
    {
        var changes = new List<LevelChange>();
        if (level >= 0 && level + 1 < increasePerLevel.Length)
        {
            changes.Add(new LevelChange("取得範囲",
                LevelChange.Signed(increasePerLevel[level]),
                LevelChange.Signed(increasePerLevel[level + 1])));
        }
        return changes;
    }
}
