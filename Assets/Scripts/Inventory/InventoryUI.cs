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

    [Header("Attributes")]
    [Tooltip("属性の数を持つプレイヤー")]
    [SerializeField] private Player player;
    [Tooltip("見出しの右に並べる、持っている属性のまとめ")]
    [SerializeField] private AttributeSummaryView attributeSummary;
    [Tooltip("属性のバッジを押したときに開く詳細パネル")]
    [SerializeField] private AttributeDetailPanel attributeDetailPanel;

    // 選択中のWeaponスロット
    private WeaponSlotView selectedWeaponSlot;

    // 選択中のItemスロット
    private ItemSlotView selectedItemSlot;

    private AttributeTracker attributeTracker;
    private AttributeSnapshot attributeSnapshot;

    private void Awake()
    {
        foreach (var slotUI in weaponSlotUIs)
            slotUI.AttributeClicked += OpenAttributeDetail;
        foreach (var slotUI in itemSlotUIs)
            slotUI.AttributeClicked += OpenAttributeDetail;
        attributeSummary.BadgeClicked += OpenAttributeDetail;
    }

    private void OnEnable()
    {
        inventory.OnSlotChanged += RefreshWeaponSlot;
        inventory.OnItemsChanged += RefreshItems;

        attributeTracker = player.Attributes;
        if (attributeTracker != null)
            attributeTracker.Changed += RefreshAttributes;

        RefreshAll();
    }

    private void OnDisable()
    {
        inventory.OnSlotChanged -= RefreshWeaponSlot;
        inventory.OnItemsChanged -= RefreshItems;

        if (attributeTracker != null)
            attributeTracker.Changed -= RefreshAttributes;
        attributeTracker = null;

        attributeDetailPanel.Close();
    }

    /// <summary>
    /// インベントリ全体を更新する。
    /// </summary>
    private void RefreshAll()
    {
        RefreshWeapons();
        RefreshItems();
        RefreshAttributes();
    }

    private void RefreshAttributes()
    {
        if (attributeTracker == null)
            return;

        attributeSnapshot = new AttributeSnapshot(
            AttributeTracker.OwnedWeapons(inventory), AttributeTracker.OwnedItems(inventory), attributeTracker.Catalog);

        foreach (var slotUI in weaponSlotUIs)
            slotUI.ShowAttributes(inventory.GetWeapon(slotUI.SlotType), attributeSnapshot);

        for (int i = 0; i < itemSlotUIs.Length; i++)
            itemSlotUIs[i].ShowAttributes(inventory.Items[i], attributeSnapshot);

        attributeSummary.Show(attributeSnapshot);
    }

    private void OpenAttributeDetail(AttributeDefinition attribute)
    {
        if (attributeSnapshot != null)
            attributeDetailPanel.Show(attribute, attributeSnapshot);
    }

    /// <summary>
    /// Weaponスロットをすべて更新する。
    /// </summary>
    private void RefreshWeapons()
    {
        foreach (var slotUI in weaponSlotUIs)
        {
            Weapon weapon =
                inventory.GetWeapon(slotUI.SlotType);

            slotUI.Refresh(weapon);
        }
    }

    /// <summary>
    /// Itemスロットをすべて更新する。
    /// </summary>
    private void RefreshItems()
    {
        for (int i = 0; i < itemSlotUIs.Length; i++)
        {
            itemSlotUIs[i].Refresh(inventory.Items[i], i);
        }
    }

    /// <summary>
    /// Weaponスロット1つを更新する。
    /// </summary>
    private void RefreshWeaponSlot(SlotType slotType, Weapon weapon)
    {
        foreach (var slotUI in weaponSlotUIs)
        {
            if (slotUI.SlotType != slotType)
                continue;

            slotUI.Refresh(weapon);
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