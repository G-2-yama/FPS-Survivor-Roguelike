using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum AttributeDisplayState
{
    Idle,
    Active,
    Max,
}

public static class AttributeDisplayStates
{
    public static AttributeDisplayState Of(AttributeDefinition attribute, int count)
    {
        if (attribute.IsMaxAt(count))
            return AttributeDisplayState.Max;
        return attribute.IsActiveAt(count) ? AttributeDisplayState.Active : AttributeDisplayState.Idle;
    }

    public static Color ToColor(this AttributeDisplayState state, UIPalette palette, Color idle)
    {
        return state switch
        {
            AttributeDisplayState.Max => palette.AttributeMaxColor,
            AttributeDisplayState.Active => palette.AttributeActiveColor,
            _ => idle,
        };
    }
}

public class AttributeBadgeView : MonoBehaviour
{
    [Tooltip("属性名")]
    [SerializeField] private TMP_Text nameText;
    [Tooltip("枠。状態の色で塗る")]
    [SerializeField] private Image frameImage;
    [Tooltip("枠の内側の背景")]
    [SerializeField] private Image fillImage;
    [Tooltip("属性の色")]
    [SerializeField] private UIPalette palette;

    [Header("Shapes")]
    [Tooltip("基盤の枠")]
    [SerializeField] private Sprite baseFrameSprite;
    [Tooltip("基盤の背景。空なら四角")]
    [SerializeField] private Sprite baseFillSprite;
    [Tooltip("モジュールの枠")]
    [SerializeField] private Sprite moduleFrameSprite;
    [Tooltip("モジュールの背景")]
    [SerializeField] private Sprite moduleFillSprite;

    public void Setup(AttributeDefinition attribute, AttributeDisplayState state)
    {
        bool isBase = attribute.Kind == AttributeKind.Base;
        frameImage.sprite = isBase ? baseFrameSprite : moduleFrameSprite;
        fillImage.sprite = isBase ? baseFillSprite : moduleFillSprite;

        nameText.text = attribute.DisplayName;
        nameText.color = state.ToColor(palette, palette.AttributeIdleColor);
        frameImage.color = state.ToColor(palette, palette.AttributeIdleFrameColor);
    }
}
