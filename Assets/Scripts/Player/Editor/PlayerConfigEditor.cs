using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>5分類の設定を入口から作成、選択、展開して編集できるInspector</summary>
[CustomEditor(typeof(PlayerConfig))]
public sealed class PlayerConfigEditor : Editor
{
    private static readonly string[] Fields = { "stats", "control", "collision", "cameraSettings", "progression" };
    private static readonly string[] Labels = { "ステータス", "操作", "衝突判定", "カメラ演出", "レベル成長" };
    private static readonly System.Type[] Types = {
        typeof(PlayerStatsSettings), typeof(PlayerControlSettings), typeof(PlayerCollisionSettings),
        typeof(PlayerCameraSettings), typeof(PlayerProgressionSettings)
    };
    private readonly Dictionary<int, Editor> assetEditors = new();
    private readonly bool[] expanded = new bool[5];

    /// <summary>設定参照と作成ボタン、用途別の展開欄を表示する</summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.HelpBox("設定項目にカーソルを合わせると用途と単位を表示します\nSOは複数のプレイヤーで共有できます", MessageType.Info);
        if (Application.isPlaying)
            EditorGUILayout.HelpBox("SOの変更は次回のプレイ開始時に反映されます 実行中の値はPlayerのInspectorで確認できます", MessageType.Info);
        for (int i = 0; i < Fields.Length; i++) DrawSettings(i);
        serializedObject.ApplyModifiedProperties();
        if (!((PlayerConfig)target).Validate(out string error))
            EditorGUILayout.HelpBox(error, MessageType.Error);
    }

    /// <summary>1分類のSO参照と必要に応じたアセット編集欄を表示する</summary>
    private void DrawSettings(int index)
    {
        SerializedProperty property = serializedObject.FindProperty(Fields[index]);
        EditorGUILayout.PropertyField(property);
        using (new EditorGUILayout.HorizontalScope())
        {
            expanded[index] = EditorGUILayout.Foldout(expanded[index], Labels[index] + "を展開", true);
            if (GUILayout.Button("新規作成", GUILayout.Width(80))) CreateAsset(property, index);
            using (new EditorGUI.DisabledScope(property.objectReferenceValue == null))
                if (GUILayout.Button("選択", GUILayout.Width(50))) Selection.activeObject = property.objectReferenceValue;
        }
        if (!expanded[index] || property.objectReferenceValue == null) return;
        int id = property.objectReferenceValue.GetInstanceID();
        if (!assetEditors.TryGetValue(id, out Editor editor))
        {
            editor = CreateEditor(property.objectReferenceValue);
            assetEditors.Add(id, editor);
        }
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) editor.OnInspectorGUI();
    }

    /// <summary>保存先を指定して独立したSOを作り、この設定欄へ割り当てる</summary>
    private void CreateAsset(SerializedProperty property, int index)
    {
        string path = EditorUtility.SaveFilePanelInProject(Labels[index] + "設定の作成",
            Types[index].Name, "asset", "新しい設定SOの保存先を指定してください", "Assets/Datas/Player");
        if (string.IsNullOrEmpty(path)) return;
        var asset = ScriptableObject.CreateInstance(Types[index]);
        AssetDatabase.CreateAsset(asset, path);
        Undo.RecordObject(target, "プレイヤー設定SOを割り当てる");
        property.objectReferenceValue = asset;
        serializedObject.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(asset);
    }

    /// <summary>展開したSO用のEditorを解放する</summary>
    private void OnDisable()
    {
        foreach (Editor editor in assetEditors.Values) DestroyImmediate(editor);
        assetEditors.Clear();
    }
}
