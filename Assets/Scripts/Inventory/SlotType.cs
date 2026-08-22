using System;

public enum WeaponSlotGroup
{
    Main,
    Ability,
    AutoWeapon
}

public enum SlotType
{
    LeftMain,
    RightMain,

    LeftAbility,
    RightAbility,

    LeftAutoWeapon,
    RightAutoWeapon
}

public static class SlotTypeExtensions
{
    /// <summary>
    /// スロットが属するグループを取得する
    /// </summary>
    public static WeaponSlotGroup GetGroup(this SlotType slot)
    {
        switch (slot)
        {
            case SlotType.LeftMain:
            case SlotType.RightMain:
                return WeaponSlotGroup.Main;

            case SlotType.LeftAbility:
            case SlotType.RightAbility:
                return WeaponSlotGroup.Ability;

            case SlotType.LeftAutoWeapon:
            case SlotType.RightAutoWeapon:
                return WeaponSlotGroup.AutoWeapon;

            default:
                throw new ArgumentOutOfRangeException(nameof(slot), slot, "未定義のSlotTypeです");
        }
    }

    /// <summary>
    /// 2つのスロットが互換性があるか
    /// </summary>
    public static bool IsCompatibleWith(this SlotType a, SlotType b)
    {
        return a.GetGroup() == b.GetGroup();
    }
}