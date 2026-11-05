using UnityEditor;

/// <summary>敵専用サイクルの設定条件と旧Phaseとの併用方法を表示する</summary>
[CustomEditor(typeof(EnemyCyclePhase))]
public sealed class EnemyCyclePhaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("敵専用：通常の雑魚 → 中ボスのインターバル → 大群。"
            + "既存Generatorのphases欄に旧Phaseと混在できます。Start Timeは並び順と最早開始時刻です。"
            + "撃破時刻ではなく区間の設定秒数で切り替わり、未撃破の中ボスは残ります。", MessageType.Info);
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "isExPhase");
        serializedObject.ApplyModifiedProperties();
        var phase = (EnemyCyclePhase)target;
        EditorGUILayout.LabelField("1Phaseの長さ", $"{phase.TotalDuration:0.##} 秒");
        EditorGUILayout.LabelField("終了・次Phaseの開始時刻", $"{phase.StartTime + phase.TotalDuration:0.##} 秒");
        EditorGUILayout.HelpBox("中ボスHP = 基礎HP × clamp(√(通常区間の実測DPS / Reference DPS), Min HP Multiplier, Max HP Multiplier)。"
            + "通常区間の終了時に確定し、雑魚には適用しません。", MessageType.None);
        if (!((EnemyCyclePhase)target).Validate(out string error))
            EditorGUILayout.HelpBox(error, MessageType.Error);
    }
}
