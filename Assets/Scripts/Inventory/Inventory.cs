using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private Player player;

    [SerializeField] private Weapon leftWeapon;
    [SerializeField] private Weapon rightWeapon;
    [SerializeField] private Weapon leftAbility;
    [SerializeField] private Weapon rightAbility;
    [SerializeField] private Weapon leftAutoWeapon;
    [SerializeField] private Weapon rightAutoWeapon;

    private Dictionary<SlotType, Weapon> slots;
    public IReadOnlyDictionary<SlotType, Weapon> Slots => slots;
    public event Action<SlotType, WeaponData> OnSlotChanged;

    private List<Item> items = new List<Item>();
    public IReadOnlyList<Item> Items => items;
    public event Action<Item> OnItemAdded;

    private void Awake()
    {
        slots = new Dictionary<SlotType, Weapon>
        {
            { SlotType.LeftMain,       leftWeapon      },
            { SlotType.RightMain,      rightWeapon     },
            { SlotType.LeftAbility,    leftAbility     },
            { SlotType.RightAbility,   rightAbility    },
            { SlotType.LeftAutoWeapon, leftAutoWeapon  },
            { SlotType.RightAutoWeapon,rightAutoWeapon },
        };
    }

    public bool HasWeapon(SlotType slot)
        => slots.TryGetValue(slot, out var w) && w?.WeaponData != null;

    public void EquipWeapon(SlotType slot, WeaponData data, int level = 0, int ammo = -1)
    {
        if (slots.TryGetValue(slot, out var weapon))
            weapon.Equip(data, level, ammo);
    }

    public void EquipItem(Item item)
    {
        items.Add(item);
        item.Initialize(player);
        item.Apply();
        OnItemAdded?.Invoke(item);
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
