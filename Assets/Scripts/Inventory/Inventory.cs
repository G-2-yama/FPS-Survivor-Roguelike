using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private Weapon mainLeftWeapon;
    [SerializeField] private Weapon mainRightWeapon;

    private Dictionary<SlotType, Weapon> slots;

    public event Action<SlotType, WeaponData> OnSlotChanged;

    private void Awake()
    {
        slots = new Dictionary<SlotType, Weapon>
        {
            { SlotType.MainLeft,  mainLeftWeapon  },
            { SlotType.MainRight, mainRightWeapon },
        };
    }

    public Weapon GetWeapon(SlotType slot) => slots[slot];

    public bool HasWeapon(SlotType slot)
        => slots.TryGetValue(slot, out var w) && w?.WeaponData != null;

    public void Equip(SlotType slot, WeaponData data, int level = 0, int ammo = -1)
    {
        if (slots.TryGetValue(slot, out var weapon))
            weapon.Equip(data, level, ammo);
    }

    /// <summary>
    /// メイン左右の入れ替え
    /// </summary>
    public bool SwapMainWeapons()
    {
        if (!HasWeapon(SlotType.MainLeft) && !HasWeapon(SlotType.MainRight))
            return false;

        mainLeftWeapon.SwapLoadoutWith(mainRightWeapon);

        OnSlotChanged?.Invoke(SlotType.MainLeft, mainLeftWeapon.WeaponData);
        OnSlotChanged?.Invoke(SlotType.MainRight, mainRightWeapon.WeaponData);
        return true;
    }

    /// <summary>
    /// 指定したスロット同士の装備を入れ替える
    /// </summary>
    /// <param name="slotA"></param>
    /// <param name="slotB"></param>
    /// <returns></returns>
    public bool Swap(SlotType slotA, SlotType slotB)
    {
        if (!slots.ContainsKey(slotA) || !slots.ContainsKey(slotB))
            return false;

        slots[slotA].SwapLoadoutWith(slots[slotB]);

        // 両スロットの変化を通知
        OnSlotChanged?.Invoke(slotA, slots[slotA].WeaponData);
        OnSlotChanged?.Invoke(slotB, slots[slotB].WeaponData);
        return true;
    }
}
