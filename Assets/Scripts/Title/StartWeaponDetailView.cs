using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartWeaponDetailView : MonoBehaviour
{
    [Tooltip("武器名を表示するテキスト")]
    [SerializeField] private TMP_Text nameText;

    [Tooltip("武器の説明を表示するテキスト")]
    [SerializeField] private TMP_Text descriptionText;

    [Tooltip("武器の画像（WeaponData の Artwork）を表示する画像")]
    [SerializeField] private Image artworkImage;

    [Tooltip("武器を切り替えたときに動かす要素")]
    [SerializeField] private List<TitleSlideIn> swapEffects = new List<TitleSlideIn>();

    [Header("Attributes")]
    [Tooltip("武器の基盤を並べるバッジの列")]
    [SerializeField] private AttributeBadgeListView baseBadges;

    public event Action<AttributeDefinition> AttributeClicked;

    private void Awake()
    {
        baseBadges.BadgeClicked += attribute => AttributeClicked?.Invoke(attribute);
    }

    public void ShowAttributes(WeaponData weaponData, AttributeSnapshot snapshot)
    {
        baseBadges.Show(weaponData.Attributes.Where(attribute => attribute != null && attribute.Kind == AttributeKind.Base), snapshot);
    }

    public void Show(WeaponData weaponData)
    {
        nameText.text = weaponData.DisplayName;
        descriptionText.text = weaponData.Description;

        artworkImage.sprite = weaponData.Artwork;
        artworkImage.enabled = weaponData.Artwork != null;

        foreach (var effect in swapEffects)
            effect.Play();
    }
}
