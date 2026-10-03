using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BalanceSim.Editor
{
    [CustomEditor(typeof(BalanceSimSettings))]
    public class BalanceSimSettingsEditor : UnityEditor.Editor
    {
        private const string PolicyPropertyName = "choicePolicy";

        private static Type[] _policyTypes;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, PolicyPropertyName);
            DrawPolicy(serializedObject.FindProperty(PolicyPropertyName));
            serializedObject.ApplyModifiedProperties();

            var settings = (BalanceSimSettings)target;
            EditorGUILayout.Space();
            if (settings.Values is SimSweep sweep)
            {
                SimSweepEditor.DrawCaseCount(sweep, settings.RunCount);
            }

            if (!SimRunner.IsRunning)
            {
                if (GUILayout.Button("実行", GUILayout.Height(30)))
                {
                    SimRunner.Run(settings);
                }
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    GUILayout.Button($"実行中... {SimRunner.ProgressText}", GUILayout.Height(30));
                }
                if (GUILayout.Button("中止", GUILayout.Height(30), GUILayout.Width(80)))
                {
                    SimRunner.Cancel();
                }
            }
        }

        private static void DrawPolicy(SerializedProperty property)
        {
            _policyTypes ??= TypeCache.GetTypesDerivedFrom<IUpgradeChoicePolicy>()
                .Where(t => !t.IsAbstract && !t.IsGenericType && t.IsSerializable && t.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(t => t.Name)
                .ToArray();

            Type current = property.managedReferenceValue?.GetType();
            int index = Array.IndexOf(_policyTypes, current);
            string[] labels = _policyTypes.Select(t => t.Name).ToArray();

            int selected = EditorGUILayout.Popup(new GUIContent("選び方の種類", property.tooltip), index, labels);
            if (selected != index && selected >= 0)
            {
                property.managedReferenceValue = Activator.CreateInstance(_policyTypes[selected]);
            }

            if (property.managedReferenceValue != null)
            {
                EditorGUILayout.PropertyField(property, new GUIContent("選び方の設定"), true);
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return SimRunner.IsRunning;
        }
    }
}
