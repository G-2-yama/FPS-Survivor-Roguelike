using UnityEditor;
using UnityEngine;

namespace BalanceSim.Editor
{
    [CustomEditor(typeof(BalanceSimSettings))]
    public class BalanceSimSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(SimRunner.IsRunning))
            {
                if (GUILayout.Button(SimRunner.IsRunning ? "実行中..." : "実行", GUILayout.Height(30)))
                {
                    SimRunner.Run((BalanceSimSettings)target);
                }
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return SimRunner.IsRunning;
        }
    }
}
