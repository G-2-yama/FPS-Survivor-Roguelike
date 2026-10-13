using UnityEditor;

/// <summary>プレイヤー設定SOに数値検証の説明を表示する</summary>
[CustomEditor(typeof(PlayerSettingsAsset), true)]
public sealed class PlayerSettingsAssetEditor : Editor
{
    /// <summary>設定項目と不正な設定の日本語説明を描画する</summary>
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (!((PlayerSettingsAsset)target).Validate(out string error))
            EditorGUILayout.HelpBox(error, MessageType.Error);
    }
}
