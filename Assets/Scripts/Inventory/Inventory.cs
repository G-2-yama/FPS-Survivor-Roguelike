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

    private Dictionary<SlotType, Weapon> weaponSlots;
    public IReadOnlyDictionary<SlotType, Weapon> WeaponSlots => weaponSlots;
    public event Action<SlotType, WeaponData> OnSlotChanged;

    private Item[] items = new Item[6];
    public IReadOnlyList<Item> Items => items;
    public event Action<Item>    OnItemAdded;

    /// <summary>廃棄されたアイテムのインデックスを通知する</summary>
    public event Action<int> OnItemRemoved;

    private void Awake()
    {
        weaponSlots = new Dictionary<SlotType, Weapon>
        {
            { SlotType.LeftMain,        leftWeapon      },
            { SlotType.RightMain,       rightWeapon     },
            { SlotType.LeftAbility,     leftAbility     },
            { SlotType.RightAbility,    rightAbility    },
            { SlotType.LeftAutoWeapon,  leftAutoWeapon  },
            { SlotType.RightAutoWeapon, rightAutoWeapon },
        };
    }

    // -------------------------------------------------------
    // Weapon
    // -------------------------------------------------------

    public bool HasWeapon(SlotType slot)
        => weaponSlots.TryGetValue(slot, out var w) && w?.HasWeapon == true;

    public void EquipWeapon(SlotType slot, WeaponData data, int level = 0, int ammo = -1)
    {
        if (weaponSlots.TryGetValue(slot, out var weapon))
            weapon.Equip(data, level, ammo);
    }

    /// <summary>
    /// 指定したスロット同士の装備を入れ替える
    /// </summary>
    public bool Swap(SlotType slotA, SlotType slotB)
    {
        if (!weaponSlots.ContainsKey(slotA) || !weaponSlots.ContainsKey(slotB))
            return false;

        if (!slotA.IsCompatibleWith(slotB))
            return false;

        SwapLoadout(weaponSlots[slotA], weaponSlots[slotB]);

        OnSlotChanged?.Invoke(slotA, weaponSlots[slotA].WeaponData);
        OnSlotChanged?.Invoke(slotB, weaponSlots[slotB].WeaponData);
        return true;
    }

    private void SwapLoadout(Weapon weaponA, Weapon weaponB)
    {
        WeaponData dataA = weaponA.WeaponData;
        int ammoA = weaponA.CurrentAmmo;

        WeaponData dataB = weaponB.WeaponData;
        int ammoB = weaponB.CurrentAmmo;

        weaponA.Equip(dataB, 0, ammoB);
        weaponB.Equip(dataA, 0, ammoA);
    }

    public bool DiscardWeapon(SlotType slot)
    {
        if (!weaponSlots.TryGetValue(slot, out var weapon)) return false;

        weapon.Equip(null);
        OnSlotChanged?.Invoke(slot, weapon.WeaponData);
        return true;
    }

    // -------------------------------------------------------
    // Item
    // -------------------------------------------------------

    public void EquipItem(Item item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                item.Initialize(player);
                item.Apply();
                OnItemAdded?.Invoke(item);
                return;
            }
        }
    }

    /// <summary>
    /// 指定インデックスのアイテムを廃棄する。
    /// Item.Revert() を呼んだあとリストから除去し OnItemRemoved を発火する。
    /// </summary>
    /// <param name="index">Items リスト上のインデックス</param>
    /// <returns>廃棄に成功した場合 true</returns>
    public bool DiscardItem(int index)
    {
        if (index < 0 || index >= items.Length) return false;

        items[index].Revert();
        items[index] = null;
        OnItemRemoved?.Invoke(index);
        return true;
    }
}