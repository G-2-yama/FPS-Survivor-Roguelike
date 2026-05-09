using UnityEngine;
using System;
using Unity.VisualScripting;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Canvas inventoryCanvas;
    public Canvas InventoryCanvas => inventoryCanvas;

    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private SlotView[] slotUIs;
    [SerializeField] private DiscardSlotView discardSlotUI;

    private SlotView selectedSlot = null;

    /// <summary>
    /// スロットがクリックされたとき
    /// 1回目の選択 → 選択状態に
    /// 2回目の選択 → スワップ実行
    /// </summary>
    public void OnSlotClicked(SlotView clickedSlot)
    {
        // 初回のスロット選択
        if (selectedSlot == null)
        {
            selectedSlot = clickedSlot;
            clickedSlot.SetHighlight(true);
            return;
        }

        // 同じスロットを選択時
        if (selectedSlot == clickedSlot)
        {
            selectedSlot.SetHighlight(false);
            selectedSlot = null;
            return;
        }

        // 同じ種類のスロット出ない場合
        if (!selectedSlot.SlotType.IsCompatibleWith(clickedSlot.SlotType))
        {
            selectedSlot.SetHighlight(false);
            selectedSlot = null;
            return;
        }

        // 異なるスロットを選択→スワップ実行
        inventory.Swap(selectedSlot.SlotType, clickedSlot.SlotType);
        selectedSlot.SetHighlight(false);
        selectedSlot = null;
    }

    /// <summary>
    /// 廃棄スロットがクリックされたとき。
    /// 武器スロットが選択済みなら即廃棄。選択がなければ何もしない。
    /// </summary>
    public void OnDiscardSlotClicked()
    {
        if (selectedSlot == null) return;

        inventory.Discard(selectedSlot.SlotType);

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
            var weapon = inventory.Slots[slotUI.SlotType];
            if (weapon == null)
            {
                slotUI.Refresh(null);
                continue;
            }
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
