using UnityEngine;

/// <summary>プレイヤーの共有設定に共通する検証契約を定義する</summary>
public abstract class PlayerSettingsAsset : ScriptableObject
{
    /// <summary>設定が利用可能か確認し、不正な場合は理由を返す</summary>
    public abstract bool Validate(out string error);

    /// <summary>数値が無限大や非数ではないか確認する</summary>
    protected static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}

