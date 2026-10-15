using System;
using TMPro;
using UnityEngine;

public class AttributeCountView : MonoBehaviour
{
    [Tooltip("先頭に置くバッジのプレハブ")]
    [SerializeField] private AttributeBadgeView badgePrefab;
    [Tooltip("数（6/7）")]
    [SerializeField] private TMP_Text countText;
    [Tooltip("属性の色")]
    [SerializeField] private UIPalette palette;
    [Tooltip("バッジの幅を属性名に合わせる。オフならプレハブの最小幅のまま")]
    [SerializeField] private bool fitBadgeToName = true;

    private AttributeBadgeView badge;

    public event Action<AttributeDefinition> BadgeClicked;

    public void Setup(AttributeDefinition attribute, int count)
    {
        if (badge == null)
        {
            badge = Instantiate(badgePrefab, transform);
            badge.transform.SetAsFirstSibling();
            if (fitBadgeToName)
                badge.FitWidthToName();
            badge.Clicked += clicked => BadgeClicked?.Invoke(clicked);
        }

        AttributeDisplayState state = AttributeDisplayStates.Of(attribute, count);
        badge.Setup(attribute, state);
        badge.SetClickable(true);

        int total = attribute.Kind == AttributeKind.Base ? attribute.MaxThreshold : attribute.ActivateThreshold;
        countText.text = $"{count}/{total}";
        countText.color = state.ToColor(palette, palette.AttributeIdleColor);
    }
}
