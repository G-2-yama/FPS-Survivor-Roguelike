using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BalanceSim.Editor
{
    [CustomPropertyDrawer(typeof(SimSweepGroup))]
    public class SimSweepGroupDrawer : PropertyDrawer
    {
        private const float Spacing = 2f;
        private const float DropAreaHeight = 32f;

        private static readonly GUIContent NameLabel = new("名前", "結果の表の列名に使う。例: 全武器の威力");
        private static readonly GUIContent RangeLabel = new("倍率", "対象の値に掛ける倍率を、最小から最大まで刻みずつ振る。掛ける元の値は、固定の値（Fixed Values）があればその値、無ければゲームの値。整数の項目は四捨五入する");
        private static readonly GUIContent TargetsLabel = new("対象", "倍率を掛ける対象と項目。選べるのは数値の項目だけ");
        private static readonly GUIContent DropLabel = new("ここにアセットやオブジェクトをまとめてドラッグすると対象に足す。\n最後の行と同じ名前の項目があれば、その項目を選ぶ");

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight + Spacing;
            return line * 3 + EditorGUI.GetPropertyHeight(property.FindPropertyRelative(SimSweepGroup.TargetsField), TargetsLabel, true) + Spacing + DropAreaHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty name = property.FindPropertyRelative(SimSweepGroup.NameField);
            SerializedProperty targets = property.FindPropertyRelative(SimSweepGroup.TargetsField);
            SerializedProperty min = property.FindPropertyRelative(SimSweepGroup.MinField);
            SerializedProperty max = property.FindPropertyRelative(SimSweepGroup.MaxField);
            SerializedProperty step = property.FindPropertyRelative(SimSweepGroup.StepField);

            EditorGUI.BeginProperty(position, label, property);
            var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(line, name, NameLabel);
            line.y += line.height + Spacing;

            SimSweepAxisDrawer.DrawRange(line, RangeLabel, SerializedPropertyType.Float, min, max, step);
            line.y += line.height + Spacing;

            EditorGUI.LabelField(line, " ", Info(min.doubleValue, max.doubleValue, step.doubleValue, targets.arraySize), EditorStyles.miniLabel);
            line.y += line.height + Spacing;

            float targetsHeight = EditorGUI.GetPropertyHeight(targets, TargetsLabel, true);
            EditorGUI.PropertyField(new Rect(line.x, line.y, line.width, targetsHeight), targets, TargetsLabel, true);
            line.y += targetsHeight + Spacing;

            DrawDropArea(new Rect(line.x, line.y, line.width, DropAreaHeight), targets);
            EditorGUI.EndProperty();
        }

        private static string Info(double min, double max, double step, int targetCount)
        {
            string error = SimSweepGroup.Validate(min, max, step);
            return error ?? $"{SimSweepAxis.ValuesOf(min, max, step).Count} 通り    対象 {targetCount} 件";
        }

        private static void DrawDropArea(Rect rect, SerializedProperty targets)
        {
            GUI.Box(rect, DropLabel, EditorStyles.helpBox);

            Event current = Event.current;
            if ((current.type != EventType.DragUpdated && current.type != EventType.DragPerform) || !rect.Contains(current.mousePosition))
            {
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (current.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                AddTargets(targets, DragAndDrop.objectReferences);
            }
            current.Use();
        }

        private static void AddTargets(SerializedProperty targets, Object[] dropped)
        {
            string path = targets.arraySize > 0
                ? targets.GetArrayElementAtIndex(targets.arraySize - 1).FindPropertyRelative(SimValueTarget.PropertyPathField).stringValue
                : string.Empty;

            foreach (Object item in dropped)
            {
                Object picked = item is GameObject gameObject ? PickComponent(gameObject, path) : item;
                if (picked == null)
                {
                    continue;
                }

                bool hasPath = HasChoosable(picked, path);
                targets.InsertArrayElementAtIndex(targets.arraySize);
                SerializedProperty element = targets.GetArrayElementAtIndex(targets.arraySize - 1);
                element.FindPropertyRelative(SimValueTarget.TargetIdField).stringValue = SimValueOverride.TargetIdOf(picked);
                element.FindPropertyRelative(SimValueTarget.TargetLabelField).stringValue = SimValueOverride.TargetLabelOf(picked);
                element.FindPropertyRelative(SimValueTarget.PropertyPathField).stringValue = hasPath ? path : string.Empty;
                element.FindPropertyRelative(SimValueTarget.PropertyLabelField).stringValue = hasPath ? SimValueOverrideDrawer.DisplayLabel(picked, path) : string.Empty;
            }
        }

        private static Object PickComponent(GameObject gameObject, string path)
        {
            Component[] components = gameObject.GetComponents<Component>().Where(c => c != null && c is not Transform).ToArray();
            return components.FirstOrDefault(c => HasChoosable(c, path)) ?? components.FirstOrDefault();
        }

        private static bool HasChoosable(Object target, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            SerializedProperty property = new SerializedObject(target).FindProperty(path);
            return property != null && SimValueOverrideDrawer.IsChoosable(property, false);
        }
    }
}
