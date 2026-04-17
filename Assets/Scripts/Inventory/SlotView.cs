using UnityEngine;
using UnityEngine.UI;

public class SlotView : MonoBehaviour
{
    [SerializeField] private SlotType slotType;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Text weaponNameText;

    public SlotType SlotType => slotType;

    // インベントリUIから呼ばれる表示更新
    public void Refresh(WeaponData data)
    {
        if (data == null)
        {
            weaponIcon.sprite = null;
            weaponNameText.text = "Empty";
            return;
        }

        weaponIcon.sprite = data.Icon;
        weaponNameText.text = data.DisplayName;
    }

    /// <summary>
    /// 選択状態の表示切り替え
    /// </summary>
    /// <param name="highlight">選択状態の場合はtrue</param>
    public void SetHighlight(bool highlight)
    {
        weaponIcon.color = highlight ? Color.yellow : Color.white;
    }
}