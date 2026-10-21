using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMenuItem : MonoBehaviour, IPointerEnterHandler
{
    [Tooltip("選ばれているときの見た目を出す演出")]
    [SerializeField] private TitleHoverFx hoverFx;

    [Tooltip("決定で押すボタン 左右で値を変える行では空にします")]
    [SerializeField] private Button button;

    public event Action<SettingsMenuItem> PointerEntered;

    public virtual void SetFocused(bool focused)
    {
        if (hoverFx != null)
            hoverFx.Focused = focused;
    }

    public virtual void Step(int direction)
    {
    }

    public void Submit()
    {
        if (button != null && button.IsInteractable())
            button.onClick.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEntered?.Invoke(this);
    }
}
