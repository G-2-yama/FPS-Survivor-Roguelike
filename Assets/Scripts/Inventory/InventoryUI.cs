using UnityEngine;
using System;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Canvas inventoryCanvas;
    public Canvas InventoryCanvas => inventoryCanvas;

    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private SlotView[] slotUIs;

    private SlotView selectedSlot = null;

    /// <summary>
    /// スロットがクリックされたとき
    /// 1回目の選択 → 選択状態に
    /// 2回目の選択 → スワップ実行
    /// </summary>
    public void OnSlotClicked(SlotView clickedSlot)
    {
        if (selectedSlot == null)
        {
            selectedSlot = clickedSlot;
            clickedSlot.SetHighlight(true);
            return;
        }

        if (selectedSlot == clickedSlot)
        {
            // 同じスロットをもう一度押したら選択解除
            selectedSlot.SetHighlight(false);
            selectedSlot = null;
            return;
        }

        // 異なるスロットを選択→スワップ実行
        inventory.Swap(selectedSlot.SlotType, clickedSlot.SlotType);
        selectedSlot.SetHighlight(false);
        selectedSlot = null;
    }

    private void OnEnable()
    {
        inventory.OnSlotChanged += OnSlotChanged;
        RefreshAll();
    }

    private void OnDisable()
    {
        inventory.OnSlotChanged -= OnSlotChanged;
    }

    /// <summary>
    /// 全スロットの表示を初期化
    /// </summary>
    private void RefreshAll()
    {
        foreach (var slotUI in slotUIs)
        {
            var weapon = inventory.GetWeapon(slotUI.SlotType);
            slotUI.Refresh(weapon?.WeaponData);
        }
    }

    /// <summary>
    /// 変化したスロットだけ更新
    /// </summary>
    /// <param name="slotType">変化したスロットのタイプ</param>
    /// <param name="data">新しい武器データ</param>
    private void OnSlotChanged(SlotType slotType, WeaponData data)
    {
        var slotUI = Array.Find(slotUIs, s => s.SlotType == slotType);
        slotUI?.Refresh(data);
    }
}
