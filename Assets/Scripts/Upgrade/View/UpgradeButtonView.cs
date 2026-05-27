using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButtonView : MonoBehaviour
{
    [SerializeField] Image ButtonImage;
    [SerializeField] private Text buttonText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Image iconImage;

    public void Setup(UpgradeBase upgrade)
    {
        buttonText.text = upgrade.DisplayName;
        descriptionText.text = upgrade.Description;
        iconImage.sprite = upgrade.Icon;

        Color rarityColor = GetColorByRarity(upgrade.Rarity);
        ButtonImage.color = rarityColor;
    }

    private Color GetColorByRarity(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return new Color(1f, 1f, 1f);
            case Rarity.Uncommon:
                return new Color(0.5f, 1f, 0.5f);
            case Rarity.Rare:
                return new Color(0, 0.5f, 1f);
            case Rarity.Epic:
                return new Color(0.6f, 0, 0.6f);
            case Rarity.Legendary:
                return new Color(1f, 0.5f, 0);
            default:
                return Color.white;
        }
    }
}
