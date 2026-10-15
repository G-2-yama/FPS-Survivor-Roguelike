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
    public event Action<SlotType, Weapon> OnSlotChanged;

    public event Action<Weapon> WeaponFired;
    public event Action<Weapon> WeaponReloadCompleted;


    private OwnedItem[] items = new OwnedItem[6];

    public IReadOnlyList<OwnedItem> Items => items;

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

        foreach (Weapon weapon in weaponSlots.Values)
        {
            if (weapon == null) continue;
            weapon.Fired += HandleWeaponFired;
            weapon.ReloadCompleted += HandleWeaponReloadCompleted;
        }
    }

    private void OnDestroy()
    {
        if (weaponSlots == null) return;

        foreach (Weapon weapon in weaponSlots.Values)
        {
            if (weapon == null) continue;
            weapon.Fired -= HandleWeaponFired;
            weapon.ReloadCompleted -= HandleWeaponReloadCompleted;
        }
    }

    private void HandleWeaponFired(Weapon weapon) => WeaponFired?.Invoke(weapon);

    private void HandleWeaponReloadCompleted(Weapon weapon) => WeaponReloadCompleted?.Invoke(weapon);


    // =====================================================
    // Weapon
    // =====================================================

    public bool HasWeapon(SlotType slot)
    {
        return weaponSlots.TryGetValue(slot, out var weapon)
            && weapon != null
            && weapon.HasWeapon;
    }


    public Weapon GetWeapon(SlotType slot)
    {
        if (!weaponSlots.TryGetValue(slot, out var weapon))
            return null;

        return weapon;
    }


    public void EquipWeapon(SlotType slot, WeaponData data, int level = 0, int ammo = -1)
    {
        if (!weaponSlots.TryGetValue(slot, out var weapon))
            return;

        weapon.Equip(data, level, ammo);

        OnSlotChanged?.Invoke(slot, weapon);
    }


    public bool LevelUpWeapon(SlotType slot)
    {
        if (!weaponSlots.TryGetValue(slot, out var weapon) || weapon == null || !weapon.CanLevelUp)
            return false;

        weapon.LevelUp();

        OnSlotChanged?.Invoke(slot, weapon);

        return true;
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
        int levelA = weaponA.Level;
        int ammoA = weaponA.CurrentAmmo;
        WeaponRuntime runtimeA = weaponA.Runtime;
        WeaponData dataB = weaponB.WeaponData;
        int levelB = weaponB.Level;
        int ammoB = weaponB.CurrentAmmo;
        WeaponRuntime runtimeB = weaponB.Runtime;

        weaponA.Equip(dataB, levelB, ammoB, runtimeB);
        weaponB.Equip(dataA, levelA, ammoA, runtimeA);

        OnSlotChanged?.Invoke(slotA, weaponA);
        OnSlotChanged?.Invoke(slotB, weaponB);

        return true;
    }


    public bool DiscardWeapon(SlotType slot)
    {
        if (!weaponSlots.TryGetValue(slot, out var weapon))
            return false;

        if (weapon == null)
            return false;

        weapon.Equip(null);

        OnSlotChanged?.Invoke(slot, weapon);

        return true;
    }


    // =====================================================
    // Item
    // =====================================================

    public bool EquipItem(Item item, int level = 0)
    {
        if (item == null)
            return false;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
                continue;

            var owned = new OwnedItem(item, Mathf.Clamp(level, 0, Mathf.Max(0, item.MaxLevel - 1)));
            items[i] = owned;

            item.Apply(player, owned.Level);

            OnItemsChanged?.Invoke();

            return true;
        }

        // インベントリが満杯
        return false;
    }


    public bool LevelUpItem(int index)
    {
        if (index < 0 || index >= items.Length)
            return false;

        OwnedItem owned = items[index];
        if (owned == null || !owned.CanLevelUp)
            return false;

        owned.Item.Revert(player, owned.Level);
        owned.Level++;
        owned.Item.Apply(player, owned.Level);

        OnItemsChanged?.Invoke();

        return true;
    }


    public bool HasItemInGroup(ItemGroup group)
    {
        if (group == null)
            return false;

        foreach (OwnedItem owned in items)
        {
            if (owned != null && owned.Item.ExclusiveGroup == group)
                return true;
        }
        return false;
    }


    public bool DiscardItem(int index)
    {
        if (index < 0 || index >= items.Length)
            return false;

        if (items[index] == null)
            return false;

        items[index].Item.Revert(player, items[index].Level);

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