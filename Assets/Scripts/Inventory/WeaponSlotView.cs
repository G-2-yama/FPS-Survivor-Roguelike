using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponSlotView : MonoBehaviour
{
    [SerializeField] private SlotType slotType;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Image Button;
    [SerializeField] private TMP_Text weaponNameText;

    private Color initialColor;

    private void Awake()
    {
        initialColor = Button.color;
    }

    public SlotType SlotType => slotType;

    // インベントリUIから呼ばれる表示更新
    public void Refresh(Weapon weapon)
    {
        if (weapon == null || !weapon.HasWeapon)
        {
            weaponIcon.sprite = null;
            weaponIcon.gameObject.SetActive(false);
            weaponNameText.text = "Empty";
            return;
        }

        weaponIcon.sprite = weapon.CurrentLevel.Icon;
        weaponIcon.gameObject.SetActive(true);
        weaponNameText.text = weapon.CurrentLevel.DisplayName;
    }

    /// <summary>
    /// 選択状態の表示切り替え
    /// </summary>
    /// <param name="highlight">選択状態の場合はtrue</param>
    public void SetHighlight(bool highlight)
    {
        Button.color = highlight ? Color.yellow : initialColor;
    }
}