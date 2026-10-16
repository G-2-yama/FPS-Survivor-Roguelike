using System;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private SlotType slotType;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Image Button;
    [SerializeField] private TMP_Text weaponNameText;
    [Tooltip("右上に出すレベル")]
    [SerializeField] private TMP_Text levelText;
    [Tooltip("Evo などの色")]
    [SerializeField] private UIPalette palette;
    [Tooltip("左上に並べる属性のバッジ")]
    [SerializeField] private AttributeBadgeListView attributeBadges;
    [Tooltip("Evo したメイン武器の名前の後ろに付ける文字")]
    [SerializeField] private string evoSuffix = " (+2)";

    private Color initialColor;

    public event Action<AttributeDefinition> AttributeClicked;
    public event Action<WeaponSlotView> PointerEntered;
    public event Action<WeaponSlotView> PointerExited;

    public void OnPointerEnter(PointerEventData eventData) => PointerEntered?.Invoke(this);

    public void OnPointerExit(PointerEventData eventData) => PointerExited?.Invoke(this);

    private void Awake()
    {
        initialColor = Button.color;
        attributeBadges.BadgeClicked += attribute => AttributeClicked?.Invoke(attribute);
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
        weaponNameText.text = weapon.IsEvo ? palette.ColorizeEvo(weapon.DisplayName + EvoSuffixOf(weapon)) : weapon.DisplayName;
        levelText.gameObject.SetActive(true);
        levelText.text = weapon.IsEvo ? palette.ColorizeEvo("Evo") : $"Lv{weapon.Level + 1}";
    }

    public void ShowAttributes(Weapon weapon, AttributeSnapshot snapshot)
    {
        bool hasWeapon = weapon != null && weapon.HasWeapon;
        attributeBadges.Show(hasWeapon ? weapon.WeaponData.Attributes : Array.Empty<AttributeDefinition>(), snapshot);
    }

    private string EvoSuffixOf(Weapon weapon) => weapon.WeaponData.WeaponType == WeaponType.Main ? evoSuffix : "";

    /// <summary>
    /// 選択状態の表示切り替え
    /// </summary>
    /// <param name="highlight">選択状態の場合はtrue</param>
    public void SetHighlight(bool highlight)
    {
        Button.color = highlight ? Color.yellow : initialColor;
    }
}