using UnityEngine;
using System;
 
/// <summary>
/// インベントリUI全体の管理クラス。
/// WeaponスロットとItemスロットの両方に対応する。
///
/// 選択フロー（Weapon / Item 共通）:
///   1回目クリック → 選択状態（ハイライト ON）
///   同じスロットを再クリック → 選択解除
///   廃棄スロットをクリック → 選択中のスロットを廃棄
///   Weaponスロット同士を選択 → 互換性があればスワップ
///   Itemスロットは廃棄のみ（スワップなし）
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Canvas inventoryCanvas;
    public Canvas InventoryCanvas => inventoryCanvas;
 
    [SerializeField] private PlayerInventory inventory;
 
    [SerializeField] private WeaponSlotView[] weaponSlotUIs;
    [SerializeField] private ItemSlotView[] itemSlotUIs;

    [SerializeField] private DiscardSlotView discardSlotUI;
 
    // 選択中のWeaponスロット
    private WeaponSlotView selectedWeaponSlot = null;
 
    // 選択中のItemスロット
    private ItemSlotView selectedItemSlot = null;
 
    // -------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------
 
    private void OnEnable()
    {
        inventory.OnSlotChanged += OnWeaponSlotChanged;
        inventory.OnItemAdded   += OnItemAdded;
        inventory.OnItemRemoved += OnItemRemoved;
        RefreshAll();
    }
 
    private void OnDisable()
    {
        inventory.OnSlotChanged -= OnWeaponSlotChanged;
        inventory.OnItemAdded   -= OnItemAdded;
        inventory.OnItemRemoved -= OnItemRemoved;
    }
 
    // -------------------------------------------------------
    // クリックハンドラー（各ViewのButton.OnClickから呼ぶ）
    // -------------------------------------------------------
 
    /// <summary>
    /// Weaponスロットがクリックされたとき
    /// </summary>
    public void OnWeaponSlotClicked(WeaponSlotView clickedSlot)
    {
        // Itemスロットの選択は解除する
        ClearItemSelection();
 
        // 初回選択
        if (selectedWeaponSlot == null)
        {
            selectedWeaponSlot = clickedSlot;
            clickedSlot.SetHighlight(true);
            return;
        }
 
        // 同じスロットを再クリック → 解除
        if (selectedWeaponSlot == clickedSlot)
        {
            ClearWeaponSelection();
            return;
        }
 
        // 互換性なし → 選択し直し
        if (!selectedWeaponSlot.SlotType.IsCompatibleWith(clickedSlot.SlotType))
        {
            ClearWeaponSelection();
            return;
        }
 
        // スワップ実行
        inventory.Swap(selectedWeaponSlot.SlotType, clickedSlot.SlotType);
        ClearWeaponSelection();
    }
 
    /// <summary>
    /// Itemスロットがクリックされたとき（廃棄のみ。廃棄スロットへ誘導する）
    /// </summary>
    public void OnItemSlotClicked(ItemSlotView clickedSlot)
    {
        // Weaponスロットの選択は解除する
        ClearWeaponSelection();
 
        // 初回選択
        if (selectedItemSlot == null)
        {
            selectedItemSlot = clickedSlot;
            clickedSlot.SetHighlight(true);
            return;
        }
 
        // 同じスロットを再クリック → 解除
        if (selectedItemSlot == clickedSlot)
        {
            ClearItemSelection();
            return;
        }
 
        // Itemスロット同士はスワップ不可。選択し直し。
        ClearItemSelection();
        selectedItemSlot = clickedSlot;
        clickedSlot.SetHighlight(true);
    }
 
    /// <summary>
    /// 廃棄スロットがクリックされたとき。
    /// WeaponまたはItemが選択済みなら廃棄する。
    /// </summary>
    public void OnDiscardSlotClicked()
    {
        if (selectedWeaponSlot != null)
        {
            inventory.DiscardWeapon(selectedWeaponSlot.SlotType);
            ClearWeaponSelection();
            return;
        }
 
        if (selectedItemSlot != null)
        {
            inventory.DiscardItem(selectedItemSlot.ItemIndex);
            ClearItemSelection();
        }
    }
 
    // -------------------------------------------------------
    // 表示更新
    // -------------------------------------------------------
 
    private void RefreshAll()
    {
        RefreshAllWeaponSlots();
        RefreshAllItemSlots();
    }
 
    private void RefreshAllWeaponSlots()
    {
        foreach (var slotUI in weaponSlotUIs)
        {
            var weapon = inventory.WeaponSlots[slotUI.SlotType];
            slotUI.Refresh(weapon?.WeaponData);
        }
    }
 
    private void RefreshAllItemSlots()
    {
        for (int i = 0; i < itemSlotUIs.Length; i++)
        {
            var item = i < inventory.Items.Count ? inventory.Items[i] : null;
            itemSlotUIs[i].Refresh(item, i);
        }
    }
 
    // -------------------------------------------------------
    // イベントハンドラー
    // -------------------------------------------------------
 
    private void OnWeaponSlotChanged(SlotType slotType, WeaponData data)
    {
        var slotUI = Array.Find(weaponSlotUIs, s => s.SlotType == slotType);
        slotUI?.Refresh(data);
    }
 
    /// <summary>
    /// アイテムが追加されたとき、追加されたスロットだけ更新する
    /// </summary>
    private void OnItemAdded(Item item)
    {
        int index = inventory.Items.Count - 1;
        if (index < itemSlotUIs.Length)
            itemSlotUIs[index].Refresh(item, index);
    }
 
    /// <summary>
    /// アイテムが廃棄されたとき、そのインデックス以降を詰めて再描画する
    /// </summary>
    private void OnItemRemoved(int removedIndex)
    {
        // 削除されたインデックス以降をすべて再描画（リストが詰まるため）
        for (int i = removedIndex; i < itemSlotUIs.Length; i++)
        {
            var item = i < inventory.Items.Count ? inventory.Items[i] : null;
            itemSlotUIs[i].Refresh(item, i);
        }
    }
 
    // -------------------------------------------------------
    // 選択状態クリア
    // -------------------------------------------------------
 
    private void ClearWeaponSelection()
    {
        selectedWeaponSlot?.SetHighlight(false);
        selectedWeaponSlot = null;
    }
 
    private void ClearItemSelection()
    {
        selectedItemSlot?.SetHighlight(false);
        selectedItemSlot = null;
    }
}