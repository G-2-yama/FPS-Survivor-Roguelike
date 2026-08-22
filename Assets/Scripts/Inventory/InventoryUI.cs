using UnityEngine;
using System;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Canvas inventoryCanvas;
    public Canvas InventoryCanvas => inventoryCanvas;

    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private WeaponSlotView[] weaponSlotUIs;
    [SerializeField] private ItemSlotView[] itemSlotUIs;
    [SerializeField] private DiscardSlotView discardSlotUI;

    // 選択中のWeaponスロット
    private WeaponSlotView selectedWeaponSlot;

    // 選択中のItemスロット
    private ItemSlotView selectedItemSlot;

    private void OnEnable()
    {
        inventory.OnSlotChanged += RefreshWeaponSlot;
        inventory.OnItemsChanged += RefreshItems;

        RefreshAll();
    }

    private void OnDisable()
    {
        inventory.OnSlotChanged -= RefreshWeaponSlot;
        inventory.OnItemsChanged -= RefreshItems;
    }

    /// <summary>
    /// インベントリ全体を更新する。
    /// </summary>
    private void RefreshAll()
    {
        RefreshWeapons();
        RefreshItems();
    }

    /// <summary>
    /// Weaponスロットをすべて更新する。
    /// </summary>
    private void RefreshWeapons()
    {
        foreach (var slotUI in weaponSlotUIs)
        {
            WeaponData data =
                inventory.GetWeaponData(slotUI.SlotType);

            slotUI.Refresh(data);
        }
    }

    /// <summary>
    /// Itemスロットをすべて更新する。
    /// </summary>
    private void RefreshItems()
    {
        for (int i = 0; i < itemSlotUIs.Length; i++)
        {
            Item item = inventory.Items[i];

            itemSlotUIs[i].Refresh(item, i);
        }
    }

    /// <summary>
    /// Weaponスロット1つを更新する。
    /// </summary>
    private void RefreshWeaponSlot(SlotType slotType, WeaponData data)
    {
        foreach (var slotUI in weaponSlotUIs)
        {
            if (slotUI.SlotType != slotType)
                continue;

            slotUI.Refresh(data);
            return;
        }
    }


    // =====================================================
    // Weapon Selection
    // =====================================================

    public void OnWeaponSlotClicked(WeaponSlotView clickedSlot)
    {
        // Itemの選択を解除
        ClearItemSelection();

        // まだ選択されていない
        if (selectedWeaponSlot == null)
        {
            SelectWeaponSlot(clickedSlot);
            return;
        }

        // 同じスロットをクリック
        if (selectedWeaponSlot == clickedSlot)
        {
            ClearWeaponSelection();
            return;
        }

        // 互換性がない
        if (!selectedWeaponSlot.SlotType.IsCompatibleWith(clickedSlot.SlotType))
        {
            ClearWeaponSelection();

            // 今クリックしたスロットを新しく選択
            SelectWeaponSlot(clickedSlot);

            return;
        }

        // スワップ
        inventory.Swap(selectedWeaponSlot.SlotType, clickedSlot.SlotType);

        ClearWeaponSelection();
    }


    private void SelectWeaponSlot(WeaponSlotView slot)
    {
        selectedWeaponSlot = slot;
        selectedWeaponSlot.SetHighlight(true);
    }


    private void ClearWeaponSelection()
    {
        if (selectedWeaponSlot == null)
            return;

        selectedWeaponSlot.SetHighlight(false);
        selectedWeaponSlot = null;
    }


    // =====================================================
    // Item Selection
    // =====================================================

    public void OnItemSlotClicked(ItemSlotView clickedSlot)
    {
        // Weaponの選択を解除
        ClearWeaponSelection();

        // まだ選択されていない
        if (selectedItemSlot == null)
        {
            SelectItemSlot(clickedSlot);
            return;
        }

        // 同じスロットをクリック
        if (selectedItemSlot == clickedSlot)
        {
            ClearItemSelection();
            return;
        }

        // Item同士の交換はしない
        ClearItemSelection();

        // 新しいスロットを選択
        SelectItemSlot(clickedSlot);
    }


    private void SelectItemSlot(ItemSlotView slot)
    {
        selectedItemSlot = slot;
        selectedItemSlot.SetHighlight(true);
    }


    private void ClearItemSelection()
    {
        if (selectedItemSlot == null)
            return;

        selectedItemSlot.SetHighlight(false);
        selectedItemSlot = null;
    }


    // =====================================================
    // Discard
    // =====================================================

    public void OnDiscardSlotClicked()
    {
        // Weaponを選択中
        if (selectedWeaponSlot != null)
        {
            inventory.DiscardWeapon(selectedWeaponSlot.SlotType);

            ClearWeaponSelection();
            return;
        }


        // Itemを選択中
        if (selectedItemSlot != null)
        {
            inventory.DiscardItem(selectedItemSlot.ItemIndex);

            ClearItemSelection();
        }
    }
}