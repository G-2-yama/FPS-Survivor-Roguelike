using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private Player player;

    private Dictionary<SlotType, Weapon> slots;

    public event Action<SlotType, WeaponData> OnSlotChanged;

    private void Awake()
    {
        slots = new Dictionary<SlotType, Weapon>
        {
            { SlotType.LeftMain,  player.LeftWeapon  },
            { SlotType.RightMain, player.RightWeapon },
            { SlotType.LeftAbility, player.LeftAbility },   
            { SlotType.RightAbility, player.RightAbility },
            { SlotType.LeftAutoWeapon, player.LeftAutoWeapon },
            { SlotType.RightAutoWeapon, player.RightAutoWeapon }
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
    /// 指定したスロット同士の装備を入れ替える
    /// </summary>
    /// <param name="slotA"></param>
    /// <param name="slotB"></param>
    /// <returns></returns>
    public bool Swap(SlotType slotA, SlotType slotB)
    {
        if (!slots.ContainsKey(slotA) || !slots.ContainsKey(slotB))
            return false;

        if (!slotA.IsCompatibleWith(slotB))
            return false;

        slots[slotA].SwapLoadoutWith(slots[slotB]);

        // 両スロットの変化を通知
        OnSlotChanged?.Invoke(slotA, slots[slotA].WeaponData);
        OnSlotChanged?.Invoke(slotB, slots[slotB].WeaponData);
        return true;
    }

    public bool Discard(SlotType slot)
    {
        if (!slots.TryGetValue(slot, out var weapon)) return false;

        weapon.Equip(null);
        OnSlotChanged?.Invoke(slot, null);
        return true;
    }
}
