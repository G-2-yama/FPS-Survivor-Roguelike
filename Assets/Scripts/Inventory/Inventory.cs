using UnityEngine;
using System;
using System.Collections.Generic;

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

    /// <summary>
    /// Weaponスロットの内容が変更されたときに通知する。
    /// </summary>
    public event Action<SlotType, WeaponData> OnSlotChanged;


    private Item[] items = new Item[6];

    public IReadOnlyList<Item> Items => items;

    /// <summary>
    /// Itemの状態が変更されたときに通知する。
    /// </summary>
    public event Action OnItemsChanged;


    private void Awake()
    {
        weaponSlots = new Dictionary<SlotType, Weapon>
        {
            { SlotType.LeftMain,        leftWeapon },
            { SlotType.RightMain,       rightWeapon },
            { SlotType.LeftAbility,     leftAbility },
            { SlotType.RightAbility,    rightAbility },
            { SlotType.LeftAutoWeapon,  leftAutoWeapon },
            { SlotType.RightAutoWeapon, rightAutoWeapon },
        };
    }


    // =====================================================
    // Weapon
    // =====================================================

    public bool HasWeapon(SlotType slot)
    {
        return weaponSlots.TryGetValue(slot, out var weapon)
            && weapon != null
            && weapon.HasWeapon;
    }


    public WeaponData GetWeaponData(SlotType slot)
    {
        if (!weaponSlots.TryGetValue(slot, out var weapon))
            return null;

        return weapon?.WeaponData;
    }


    public void EquipWeapon(SlotType slot, WeaponData data, int ammo = -1)
    {
        if (!weaponSlots.TryGetValue(slot, out var weapon))
            return;

        weapon.Equip(data, ammo);

        OnSlotChanged?.Invoke(slot, weapon.WeaponData);
    }


    /// <summary>
    /// 指定したスロット同士の装備を入れ替える。
    /// </summary>
    public bool Swap(SlotType slotA, SlotType slotB)
    {
        if (!weaponSlots.ContainsKey(slotA) || !weaponSlots.ContainsKey(slotB))
        {
            return false;
        }

        if (!slotA.IsCompatibleWith(slotB))
            return false;

        // スロットの武器を入れ替える
        Weapon weaponA = weaponSlots[slotA];
        Weapon weaponB = weaponSlots[slotB];

        WeaponData dataA = weaponA.WeaponData;
        int ammoA = weaponA.CurrentAmmo;
        WeaponData dataB = weaponB.WeaponData;
        int ammoB = weaponB.CurrentAmmo;

        weaponA.Equip(dataB, ammoB);
        weaponB.Equip(dataA, ammoA);

        OnSlotChanged?.Invoke(slotA, weaponA.WeaponData);
        OnSlotChanged?.Invoke(slotB, weaponB.WeaponData);

        return true;
    }


    public bool DiscardWeapon(SlotType slot)
    {
        if (!weaponSlots.TryGetValue(slot, out var weapon))
            return false;

        if (weapon == null)
            return false;

        weapon.Equip(null);

        OnSlotChanged?.Invoke(slot, weapon.WeaponData);

        return true;
    }


    // =====================================================
    // Item
    // =====================================================

    public bool EquipItem(Item item)
    {
        if (item == null)
            return false;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
                continue;

            items[i] = item;

            item.Initialize(player);
            item.Apply();

            OnItemsChanged?.Invoke();

            return true;
        }

        // インベントリが満杯
        return false;
    }


    public bool DiscardItem(int index)
    {
        if (index < 0 || index >= items.Length)
            return false;

        if (items[index] == null)
            return false;

        items[index].Revert();

        // アイテムを削除
        items[index] = null;

        // 後ろのアイテムを前に詰める
        for (int i = index; i < items.Length - 1; i++)
        {
            items[i] = items[i + 1];
        }

        // 最後を空にする
        items[^1] = null;

        OnItemsChanged?.Invoke();

        return true;
    }
}