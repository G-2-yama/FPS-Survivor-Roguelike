using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("デバッグ", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("プレイ中のみ使えます", MessageType.Info);
        }

        GameManager gameManager = (GameManager)target;

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("クリアを再現"))
            {
                gameManager.DebugClear();
            }

            if (GUILayout.Button("ゲームオーバーを再現"))
            {
                gameManager.DebugGameOver();
            }
        }
    }
}
