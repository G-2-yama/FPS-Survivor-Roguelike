using UnityEditor;
using UnityEngine;

namespace BalanceSim.Editor
{
    [CustomPropertyDrawer(typeof(SimSweepAxis))]
    public class SimSweepAxisDrawer : PropertyDrawer
    {
        private const float Spacing = 2f;
        private const float RangeLabelWidth = 28f;

        private static readonly GUIContent RangeLabel = new("範囲", "最小から最大まで、刻みずつ値を振る。最小と最大が同じなら 1 通り。真偽値は最小をオフ、最大をオンにすると両方を試す");
        private static readonly GUIContent MinLabel = new("最小");
        private static readonly GUIContent MaxLabel = new("最大");
        private static readonly GUIContent StepLabel = new("刻み");

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lines = SimValueOverrideDrawer.IsMissing(property.FindPropertyRelative(SimSweepAxis.TargetIdField).stringValue) ? 5 : 4;
            return lines * EditorGUIUtility.singleLineHeight + (lines - 1) * Spacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty targetId = property.FindPropertyRelative(SimSweepAxis.TargetIdField);
            SerializedProperty targetLabel = property.FindPropertyRelative(SimSweepAxis.TargetLabelField);
            SerializedProperty propertyPath = property.FindPropertyRelative(SimSweepAxis.PropertyPathField);
            SerializedProperty propertyLabel = property.FindPropertyRelative(SimSweepAxis.PropertyLabelField);
            SerializedProperty min = property.FindPropertyRelative(SimSweepAxis.MinField);
            SerializedProperty max = property.FindPropertyRelative(SimSweepAxis.MaxField);
            SerializedProperty step = property.FindPropertyRelative(SimSweepAxis.StepField);

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
            SerializedPropertyType type = source?.propertyType ?? SerializedPropertyType.Float;

            SimValueOverrideDrawer.DrawPropertyChoice(line, target, propertyPath, propertyLabel, true, min, max);
            line.y += line.height + Spacing;

            DrawRange(line, RangeLabel, type, min, max, step);
            line.y += line.height + Spacing;

            EditorGUI.LabelField(line, " ", Info(source, type, min.doubleValue, max.doubleValue, step.doubleValue), EditorStyles.miniLabel);
            EditorGUI.EndProperty();
        }

        internal static void DrawRange(Rect line, GUIContent label, SerializedPropertyType type, SerializedProperty min, SerializedProperty max, SerializedProperty step)
        {
            Rect field = EditorGUI.PrefixLabel(line, label);
            int indent = EditorGUI.indentLevel;
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth = RangeLabelWidth;

            float width = (field.width - Spacing * 2f) / 3f;
            var rect = new Rect(field.x, field.y, width, field.height);
            SimValueOverrideDrawer.NumberField(rect, MinLabel, type, min);
            rect.x += width + Spacing;
            SimValueOverrideDrawer.NumberField(rect, MaxLabel, type, max);
            rect.x += width + Spacing;
            SimValueOverrideDrawer.NumberField(rect, StepLabel, type == SerializedPropertyType.Boolean ? SerializedPropertyType.Integer : type, step);

            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.indentLevel = indent;
        }

        private static string Info(SerializedProperty source, SerializedPropertyType type, double min, double max, double step)
        {
            string game = source != null ? $"ゲーム: {SimValueOverride.Format(type, SimValueOverride.Read(source))}    " : string.Empty;
            string error = SimSweepAxis.Validate(min, max, step);
            return error != null ? $"{game}{error}" : $"{game}{SimSweepAxis.ValuesOf(min, max, step).Count} 通り";
        }
    }
}
