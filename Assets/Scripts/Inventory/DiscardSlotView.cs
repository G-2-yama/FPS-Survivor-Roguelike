using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 武器廃棄スロット。
/// 通常のSlotViewとは独立した「捨て場」UIコンポーネント。
/// </summary>
public class DiscardSlotView : MonoBehaviour
{
    [SerializeField] private Image highlightImage;

    public void SetHighlight(bool active)
    {
        if (highlightImage != null)
            highlightImage.enabled = active;
    }
}