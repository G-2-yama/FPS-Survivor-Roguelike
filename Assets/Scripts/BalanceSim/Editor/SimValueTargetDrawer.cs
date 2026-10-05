using UnityEditor;
using UnityEngine;

namespace BalanceSim.Editor
{
    [CustomPropertyDrawer(typeof(SimValueTarget))]
    public class SimValueTargetDrawer : PropertyDrawer
    {
        private const float Spacing = 2f;
        private const float GameValueWidth = 90f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lines = SimValueOverrideDrawer.IsMissing(property.FindPropertyRelative(SimValueTarget.TargetIdField).stringValue) ? 3 : 2;
            return lines * EditorGUIUtility.singleLineHeight + (lines - 1) * Spacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty targetId = property.FindPropertyRelative(SimValueTarget.TargetIdField);
            SerializedProperty targetLabel = property.FindPropertyRelative(SimValueTarget.TargetLabelField);
            SerializedProperty propertyPath = property.FindPropertyRelative(SimValueTarget.PropertyPathField);
            SerializedProperty propertyLabel = property.FindPropertyRelative(SimValueTarget.PropertyLabelField);

            EditorGUI.BeginProperty(position, label, property);
            var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Object target = SimValueOverrideDrawer.Resolve(targetId.stringValue);

            SimValueOverrideDrawer.DrawTarget(line, target, targetId, targetLabel, propertyPath, propertyLabel);
            line.y += line.height + Spacing;

            if (target == null && !string.IsNullOrEmpty(targetId.stringValue))
            {
                EditorGUI.LabelField(line, " ", $"{targetLabel.stringValue}（見つかりません。対象シーンを開くと表示されます）", EditorStyles.miniLabel);
                line.y += line.height + Spacing;
            }

            SerializedProperty source = SimValueOverrideDrawer.SourceProperty(target, propertyPath.stringValue);
            Rect choiceRect = line;
            if (source != null)
            {
                choiceRect.width -= GameValueWidth + Spacing;
                var gameRect = new Rect(choiceRect.xMax + Spacing, line.y, GameValueWidth, line.height);
                EditorGUI.LabelField(gameRect, $"ゲーム: {SimValueOverride.Format(source.propertyType, SimValueOverride.Read(source))}", EditorStyles.miniLabel);
            }
            SimValueOverrideDrawer.DrawPropertyChoice(choiceRect, target, propertyPath, propertyLabel, false);
            EditorGUI.EndProperty();
        }
    }
}
