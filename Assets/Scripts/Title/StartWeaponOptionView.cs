using System;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// タイトル画面の初期武器選択肢を表示する
/// </summary>
public class StartWeaponOptionView : MonoBehaviour, IPointerEnterHandler
{
    [Tooltip("選択状態を切り替えるトグル")]
    [SerializeField] private Toggle toggle;

    [Tooltip("武器名を表示するテキスト 未設定でも動作します")]
    [SerializeField] private TMP_Text nameText;

    [Tooltip("武器のアイコンを表示する画像 未設定でも動作します")]
    [SerializeField] private Image iconImage;

    [Tooltip("選択中だけ表示するもの 未設定でも動作します")]
    [SerializeField] private GameObject selectedMark;

    [Tooltip("選択中は色を保つホバー演出 未設定でも動作します")]
    [SerializeField] private TitleHoverFx hoverFx;

    public void Setup(WeaponData weaponData, ToggleGroup group, bool isOn, Action onSelected)
    {
        if (nameText != null)
            nameText.text = weaponData.DisplayName;

        if (iconImage != null)
            iconImage.sprite = weaponData.Icon;

        toggle.SetIsOnWithoutNotify(isOn);
        toggle.group = group;
        ShowSelected(isOn);
        toggle.onValueChanged.AddListener(selected =>
        {
            ShowSelected(selected);
            if (selected)
                onSelected();
        });
    }

    /// <summary>
    /// ポインターが武器選択肢に入ったときにゲームパッド決定の対象にする
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null && eventSystem.currentSelectedGameObject != gameObject)
            eventSystem.SetSelectedGameObject(gameObject);
    }

    private void ShowSelected(bool isSelected)
    {
        if (selectedMark != null)
            selectedMark.SetActive(isSelected);

        if (hoverFx != null)
            hoverFx.Highlighted = isSelected;
    }
}
