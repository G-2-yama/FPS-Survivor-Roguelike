using System;
using System.Collections.Generic;

public enum SlotType
{
    LeftMain,
    RightMain,
    LeftAbility,
    RightAbility,
    LeftAutoWeapon,
    RightAutoWeapon,
}

public static class SlotTypeExtensions
{
    private static readonly Dictionary<SlotType, WeaponType> GroupMap = new()
    {
        { SlotType.LeftMain,     WeaponType.Main    },
        { SlotType.RightMain,    WeaponType.Main    },
        { SlotType.LeftAbility,  WeaponType.Ability },
        { SlotType.RightAbility, WeaponType.Ability },
        { SlotType.LeftAutoWeapon, WeaponType.AutoWeapon },
        { SlotType.RightAutoWeapon, WeaponType.AutoWeapon }
    };

    /// <summary>
    /// スロットが属するグループを返す
    /// </summary>
    /// <param name="slot">スロット</param>
    /// <returns>スロットが属するグループ</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static WeaponType GetGroup(this SlotType slot)
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