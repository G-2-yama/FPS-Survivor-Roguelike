using UnityEngine;

[CreateAssetMenu(menuName = "UI/UIPalette")]
public class UIPalette : ScriptableObject
{
    [Tooltip("Evo の文字の色")]
    [SerializeField] private Color evoColor = new Color32(0xC7, 0x7D, 0xFF, 0xFF);
    public Color EvoColor => evoColor;

    public string ColorizeEvo(string text) => $"<color=#{ColorUtility.ToHtmlStringRGBA(evoColor)}>{text}</color>";
}
