using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Items/syncWeapon")]
public class SyncWeaponItem : Item
{
    public override int MaxLevel => 1;

    public override void Apply(Player player, int level)
    {
        player.SetWeaponSync(true);
    }

    public override void Revert(Player player, int level)
    {
        player.SetWeaponSync(false);
    }

    public override IReadOnlyList<LevelChange> GetLevelChanges(int level)
    {
        return Array.Empty<LevelChange>();
    }
}
