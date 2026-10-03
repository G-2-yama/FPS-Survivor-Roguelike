using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace BalanceSim.Editor
{
    [CustomPropertyDrawer(typeof(SimValueOverride))]
    public class SimValueOverrideDrawer : PropertyDrawer
    {
        private const float ComponentPopupWidth = 140f;
        private const float Spacing = 2f;

        private static readonly Regex PathToken = new(@"Array\.data\[\d+\]|[^.]+");

        private static readonly GUIContent TargetLabel = new("対象", "値を上書きするアセット、プレハブ、またはシーン上のオブジェクト。GameObject を入れたときは右の一覧でコンポーネントを選ぶ。シーン上のオブジェクトは、対象シーンを開いているときだけ選べる");
        private static readonly GUIContent PropertyLabel = new("項目", "上書きする項目。Inspector と同じ名前で並ぶ。選べるのは数値と真偽値だけ");
        private static readonly GUIContent ValueLabel = new("値", "シミュレーションで使う値");

        private static readonly Dictionary<string, Object> TargetCache = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lines = NeedsMissingNote(property) ? 4 : 3;
            return lines * EditorGUIUtility.singleLineHeight + (lines - 1) * Spacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty targetId = property.FindPropertyRelative(SimValueOverride.TargetIdField);
            SerializedProperty targetLabel = property.FindPropertyRelative(SimValueOverride.TargetLabelField);
            SerializedProperty propertyPath = property.FindPropertyRelative(SimValueOverride.PropertyPathField);
            SerializedProperty propertyLabel = property.FindPropertyRelative(SimValueOverride.PropertyLabelField);
            SerializedProperty value = property.FindPropertyRelative(SimValueOverride.ValueField);

            EditorGUI.BeginProperty(position, label, property);
            var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Object target = Resolve(targetId.stringValue);

            DrawTarget(line, target, targetId, targetLabel, propertyPath, propertyLabel);
            line.y += line.height + Spacing;

            if (target == null && !string.IsNullOrEmpty(targetId.stringValue))
            {
                EditorGUI.LabelField(line, " ", $"{targetLabel.stringValue}（見つかりません。対象シーンを開くと表示されます）", EditorStyles.miniLabel);
                line.y += line.height + Spacing;
            }

            SerializedProperty source = target != null && !string.IsNullOrEmpty(propertyPath.stringValue)
                ? new SerializedObject(target).FindProperty(propertyPath.stringValue)
                : null;

            DrawPropertyChoice(line, target, propertyPath, propertyLabel, value);
            line.y += line.height + Spacing;

            DrawValue(line, source, propertyPath.stringValue, value);
            EditorGUI.EndProperty();
        }

        private static bool NeedsMissingNote(SerializedProperty property)
        {
            string id = property.FindPropertyRelative(SimValueOverride.TargetIdField).stringValue;
            return !string.IsNullOrEmpty(id) && Resolve(id) == null;
        }

        private static Object Resolve(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            if (TargetCache.TryGetValue(id, out Object cached) && cached != null)
            {
                return cached;
            }

            Object resolved = SimValueOverride.ResolveTarget(id);
            if (resolved != null)
            {
                TargetCache[id] = resolved;
            }
            return resolved;
        }

        private static void DrawTarget(Rect line, Object target, SerializedProperty targetId, SerializedProperty targetLabel,
            SerializedProperty propertyPath, SerializedProperty propertyLabel)
        {
            Component[] components = target is Component current
                ? current.GetComponents<Component>().Where(c => c != null && c is not Transform).ToArray()
                : null;

            Rect fieldRect = line;
            if (components != null)
            {
                fieldRect.width -= ComponentPopupWidth + Spacing;
            }

            EditorGUI.BeginChangeCheck();
            Object picked = EditorGUI.ObjectField(fieldRect, TargetLabel, target, typeof(Object), true);
            if (EditorGUI.EndChangeCheck())
            {
                if (picked is GameObject gameObject)
                {
                    picked = gameObject.GetComponents<Component>().FirstOrDefault(c => c != null && c is not Transform);
                }
                SetTarget(picked, targetId, targetLabel, propertyPath, propertyLabel);
            }

            if (components == null)
            {
                return;
            }

            var popupRect = new Rect(fieldRect.xMax + Spacing, line.y, ComponentPopupWidth, line.height);
            int index = System.Array.IndexOf(components, target);
            string[] names = components.Select(c => c.GetType().Name).ToArray();
            int selected = EditorGUI.Popup(popupRect, index, names);
            if (selected != index && selected >= 0)
            {
                SetTarget(components[selected], targetId, targetLabel, propertyPath, propertyLabel);
            }
        }

        private static void SetTarget(Object picked, SerializedProperty targetId, SerializedProperty targetLabel,
            SerializedProperty propertyPath, SerializedProperty propertyLabel)
        {
            targetId.stringValue = picked != null ? SimValueOverride.TargetIdOf(picked) : string.Empty;
            targetLabel.stringValue = picked != null ? SimValueOverride.TargetLabelOf(picked) : string.Empty;
            propertyPath.stringValue = string.Empty;
            propertyLabel.stringValue = string.Empty;
        }

        private static void DrawPropertyChoice(Rect line, Object target, SerializedProperty propertyPath,
            SerializedProperty propertyLabel, SerializedProperty value)
        {
            Rect buttonRect = EditorGUI.PrefixLabel(line, PropertyLabel);
            string caption = string.IsNullOrEmpty(propertyLabel.stringValue) ? "（選ぶ）" : propertyLabel.stringValue;
            using (new EditorGUI.DisabledScope(target == null))
            {
                if (!EditorGUI.DropdownButton(buttonRect, new GUIContent(caption, propertyPath.stringValue), FocusType.Keyboard))
                {
                    return;
                }
            }

            SerializedObject serializedObject = propertyPath.serializedObject;
            string pathName = propertyPath.propertyPath;
            string labelName = propertyLabel.propertyPath;
            string valueName = value.propertyPath;

            var menu = new GenericMenu();
            foreach ((string path, string menuLabel, string displayLabel, double current) in ListProperties(target))
            {
                menu.AddItem(new GUIContent(menuLabel), path == propertyPath.stringValue, () =>
                {
                    serializedObject.Update();
                    serializedObject.FindProperty(pathName).stringValue = path;
                    serializedObject.FindProperty(labelName).stringValue = displayLabel;
                    serializedObject.FindProperty(valueName).doubleValue = current;
                    serializedObject.ApplyModifiedProperties();
                });
            }
            if (menu.GetItemCount() == 0)
            {
                menu.AddDisabledItem(new GUIContent("数値・真偽値の項目がありません"));
            }
            menu.DropDown(buttonRect);
        }

        private static IEnumerable<(string path, string menuLabel, string displayLabel, double current)> ListProperties(Object target)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty iterator = serialized.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = iterator.propertyType == SerializedPropertyType.Generic;
                if (!SimValueOverride.IsSupported(iterator.propertyType) || iterator.propertyPath.EndsWith(".Array.size"))
                {
                    continue;
                }

                string[] names = DisplayNames(serialized, iterator.propertyPath);
                double current = SimValueOverride.Read(iterator);
                string leaf = $"{names[^1]}  ({SimValueOverride.Format(iterator.propertyType, current)})";
                string menuLabel = string.Join("/", names.Take(names.Length - 1).Append(leaf).Select(n => n.Replace('/', '／')));
                yield return (iterator.propertyPath, menuLabel, string.Join(" > ", names), current);
            }
        }

        private static string[] DisplayNames(SerializedObject serialized, string path)
        {
            var names = new List<string>();
            string prefix = null;
            foreach (Match token in PathToken.Matches(path))
            {
                prefix = prefix == null ? token.Value : $"{prefix}.{token.Value}";
                SerializedProperty part = serialized.FindProperty(prefix);
                if (part == null)
                {
                    names.Add(token.Value);
                    continue;
                }

                if (token.Value.StartsWith("Array.data["))
                {
                    names[^1] = $"{names[^1]} > {ElementName(part)}";
                }
                else
                {
                    names.Add(part.displayName);
                }
            }
            return names.ToArray();
        }

        private static string ElementName(SerializedProperty element)
        {
            string name = element.displayName;
            if (element.propertyType == SerializedPropertyType.ObjectReference)
            {
                return element.objectReferenceValue != null ? $"{name} ({element.objectReferenceValue.name})" : name;
            }
            if (element.propertyType != SerializedPropertyType.Generic)
            {
                return name;
            }

            SerializedProperty child = element.Copy();
            SerializedProperty end = element.GetEndProperty();
            if (!child.NextVisible(true))
            {
                return name;
            }
            while (!SerializedProperty.EqualContents(child, end))
            {
                if (child.propertyType == SerializedPropertyType.ObjectReference && child.objectReferenceValue != null)
                {
                    return $"{name} ({child.objectReferenceValue.name})";
                }
                if (child.propertyType == SerializedPropertyType.String && !string.IsNullOrEmpty(child.stringValue))
                {
                    return $"{name} ({child.stringValue})";
                }
                if (!child.NextVisible(false))
                {
                    break;
                }
            }
            return name;
        }

        private static void DrawValue(Rect line, SerializedProperty source, string path, SerializedProperty value)
        {
            SerializedPropertyType type = source?.propertyType ?? SerializedPropertyType.Float;
            Rect valueRect = line;
            if (source != null)
            {
                valueRect.width = EditorGUIUtility.labelWidth + (line.width - EditorGUIUtility.labelWidth) * 0.5f;
                var gameRect = new Rect(valueRect.xMax + Spacing, line.y, line.xMax - valueRect.xMax - Spacing, line.height);
                EditorGUI.LabelField(gameRect, $"ゲーム: {SimValueOverride.Format(type, SimValueOverride.Read(source))}", EditorStyles.miniLabel);
            }
            else if (!string.IsNullOrEmpty(path))
            {
                valueRect.width = EditorGUIUtility.labelWidth + (line.width - EditorGUIUtility.labelWidth) * 0.5f;
            }

            EditorGUI.BeginChangeCheck();
            double edited;
            switch (type)
            {
                case SerializedPropertyType.Integer:
                    edited = EditorGUI.LongField(valueRect, ValueLabel, (long)System.Math.Round(value.doubleValue));
                    break;
                case SerializedPropertyType.Boolean:
                    edited = EditorGUI.Toggle(valueRect, ValueLabel, value.doubleValue != 0) ? 1 : 0;
                    break;
                default:
                    edited = EditorGUI.DoubleField(valueRect, ValueLabel, value.doubleValue);
                    break;
            }
            if (EditorGUI.EndChangeCheck())
            {
                value.doubleValue = edited;
            }
        }
    }
}
