using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponSlotView : MonoBehaviour
{
    [SerializeField] private SlotType slotType;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Image Button;
    [SerializeField] private TMP_Text weaponNameText;
    [Tooltip("右上に出すレベル")]
    [SerializeField] private TMP_Text levelText;
    [Tooltip("Evo などの色")]
    [SerializeField] private UIPalette palette;

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
            levelText.gameObject.SetActive(false);
            return;
        }

        weaponIcon.sprite = weapon.Icon;
        weaponIcon.gameObject.SetActive(true);
        weaponNameText.text = weapon.IsEvo ? palette.ColorizeEvo(weapon.DisplayName) : weapon.DisplayName;
        levelText.gameObject.SetActive(true);
        levelText.text = weapon.IsEvo ? palette.ColorizeEvo("Evo") : $"Lv{weapon.Level + 1}";
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