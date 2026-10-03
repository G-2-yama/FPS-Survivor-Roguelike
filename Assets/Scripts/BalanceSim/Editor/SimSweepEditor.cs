using System;
using UnityEditor;

namespace BalanceSim.Editor
{
    [CustomEditor(typeof(SimSweep))]
    public class SimSweepEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, SimSweep.GeneratorField);
            ManagedReferencePopup.Draw<ISimSweepGenerator>(serializedObject.FindProperty(SimSweep.GeneratorField), "作り方の種類", "作り方の設定");
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            DrawCaseCount((SimSweep)target, null);
        }

        internal static void DrawCaseCount(SimSweep sweep, int? runCount)
        {
            int cases;
            try
            {
                cases = sweep.CountCases();
            }
            catch (InvalidOperationException e)
            {
                EditorGUILayout.HelpBox(e.Message, MessageType.Warning);
                return;
            }

            if (runCount == null)
            {
                EditorGUILayout.HelpBox($"{cases} 通り。実行すると、1通りごとに設定SO の回数ずつ回す", MessageType.Info);
                return;
            }

            long runs = (long)cases * runCount.Value;
            string text = $"探索: {cases} 通り × {runCount} 回 = {runs} 回";
            float secondsPerRun = SimRunner.SecondsPerRun;
            if (secondsPerRun > 0f)
            {
                text += $"\n前回の速度で約 {FormatDuration(runs * secondsPerRun)}（値の読み出しの時間は含まない）";
            }
            EditorGUILayout.HelpBox(text, MessageType.Info);
        }

        private static string FormatDuration(double seconds)
        {
            if (seconds < 60)
            {
                return $"{seconds:0}秒";
            }
            return seconds < 3600 ? $"{seconds / 60:0}分" : $"{seconds / 3600:0.#}時間";
        }
    }
}
