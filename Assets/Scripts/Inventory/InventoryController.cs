using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// インベントリの操作を管理するコントローラー
/// </summary>
public class InventoryController : MonoBehaviour
{
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private UIFader uiFader;

    public bool IsOpen { get; private set; } = false;

    public void Open()
    {
        IsOpen = true;
        inventoryUI.InventoryCanvas.gameObject.SetActive(true);
        uiFader.FadeIn();
    }

    public void Close()
    {
        uiFader.FadeOut(() =>
        {
            inventoryUI.InventoryCanvas.gameObject.SetActive(false);
        });
        IsOpen = false;
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void OnOpenInventory(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Toggle();
    }
}