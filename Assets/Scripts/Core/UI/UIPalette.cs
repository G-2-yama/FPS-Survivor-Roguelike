using UnityEngine;

[CreateAssetMenu(menuName = "UI/UIPalette")]
public class UIPalette : ScriptableObject
{
    [Tooltip("Evo の文字の色")]
    [SerializeField] private Color evoColor = new Color32(0xC7, 0x7D, 0xFF, 0xFF);
    public Color EvoColor => evoColor;

    [Header("Attributes")]
    [Tooltip("有効化に届いていない属性の文字の色")]
    [SerializeField] private Color attributeIdleColor = new Color32(0xE6, 0xE6, 0xE6, 0xFF);
    [Tooltip("有効化に届いていない属性の枠と、空いたマスの色")]
    [SerializeField] private Color attributeIdleFrameColor = new Color(1f, 1f, 1f, 0.45f);
    [Tooltip("有効化に届いた属性の色")]
    [SerializeField] private Color attributeActiveColor = new Color32(0xFF, 0xD3, 0x4D, 0xFF);
    [Tooltip("基盤が最大に届いたときの色")]
    [SerializeField] private Color attributeMaxColor = new Color32(0xFF, 0x7A, 0x3D, 0xFF);
    public Color AttributeIdleColor => attributeIdleColor;
    public Color AttributeIdleFrameColor => attributeIdleFrameColor;
    public Color AttributeActiveColor => attributeActiveColor;
    public Color AttributeMaxColor => attributeMaxColor;

    public string ColorizeEvo(string text) => $"<color=#{ColorUtility.ToHtmlStringRGBA(evoColor)}>{text}</color>";
}
