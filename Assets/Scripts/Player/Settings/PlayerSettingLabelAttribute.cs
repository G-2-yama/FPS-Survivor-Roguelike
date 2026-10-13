using UnityEngine;

/// <summary>Inspectorに表示する項目名を指定する</summary>
public sealed class PlayerSettingLabelAttribute : PropertyAttribute
{
    public string Label { get; }
    public bool IsLayer { get; }

    /// <summary>項目の表示名を設定する</summary>
    public PlayerSettingLabelAttribute(string label, bool isLayer = false)
    {
        Label = label;
        IsLayer = isLayer;
    }
}

