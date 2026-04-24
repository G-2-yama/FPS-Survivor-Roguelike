using System;
using System.Collections.Generic;

public enum SlotType
{
    MainLeft,
    MainRight,
    LeftAbility,
    RightAbility,
    LeftAutoWeapon,
    RightAutoWeapon,
}

public enum SlotGroup
{
    Main,
    Ability,
    AutoWeapon
}

public static class SlotTypeExtensions
{
    private static readonly Dictionary<SlotType, SlotGroup> GroupMap = new()
    {
        { SlotType.MainLeft,     SlotGroup.Main    },
        { SlotType.MainRight,    SlotGroup.Main    },
        { SlotType.LeftAbility,  SlotGroup.Ability },
        { SlotType.RightAbility, SlotGroup.Ability },
        { SlotType.LeftAutoWeapon, SlotGroup.AutoWeapon },
        { SlotType.RightAutoWeapon, SlotGroup.AutoWeapon }
    };

    /// <summary>
    /// スロットが属するグループを返す
    /// </summary>
    /// <param name="slot">スロット</param>
    /// <returns>スロットが属するグループ</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static SlotGroup GetGroup(this SlotType slot)
    {
        if (GroupMap.TryGetValue(slot, out var group))
            return group;

        throw new ArgumentOutOfRangeException(nameof(slot), slot, "未定義のSlotTypeです");
    }

    /// <summary>
    /// 2つのスロットが互換性があるか返す
    /// </summary>
    /// <param name="a">スロットA</param>
    /// <param name="b">スロットB</param>
    /// <returns>true: 同じ種類のスロットタイプ / false: 異なる種類のスロットタイプ</returns>
    public static bool IsCompatibleWith(this SlotType a, SlotType b)
        => a.GetGroup() == b.GetGroup();
}