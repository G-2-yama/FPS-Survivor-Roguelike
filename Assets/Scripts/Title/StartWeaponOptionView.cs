using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StartWeaponOptionView : MonoBehaviour
{
    [Tooltip("選択状態を切り替えるトグル")]
    [SerializeField] private Toggle toggle;

    [Tooltip("武器名を表示するテキスト 未設定でも動作します")]
    [SerializeField] private TMP_Text nameText;

    [Tooltip("武器のアイコンを表示する画像 未設定でも動作します")]
    [SerializeField] private Image iconImage;

    public void Setup(WeaponData weaponData, ToggleGroup group, bool isOn, Action onSelected)
    {
        if (nameText != null)
            nameText.text = weaponData.DisplayName;

        if (iconImage != null)
            iconImage.sprite = weaponData.Icon;

        toggle.SetIsOnWithoutNotify(isOn);
        toggle.group = group;
        toggle.onValueChanged.AddListener(selected =>
        {
            if (selected)
                onSelected();
        });
    }
}
