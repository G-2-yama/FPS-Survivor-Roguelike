using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>設定項目を指定された項目名と説明付きで描画する</summary>
[CustomPropertyDrawer(typeof(PlayerSettingLabelAttribute))]
public sealed class PlayerSettingLabelDrawer : PropertyDrawer
{
    /// <summary>項目の型に合わせた入力欄と数値制限を表示する</summary>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var settingLabel = (PlayerSettingLabelAttribute)attribute;
        string tooltip = fieldInfo.GetCustomAttribute<TooltipAttribute>()?.tooltip ?? property.tooltip;
        GUIContent content = new GUIContent(settingLabel.Label, tooltip);
        EditorGUI.BeginProperty(position, content, property);
        var range = fieldInfo.GetCustomAttribute<RangeAttribute>();
        var min = fieldInfo.GetCustomAttribute<MinAttribute>();
        switch (property.propertyType)
        {
            case SerializedPropertyType.Integer:
                if (settingLabel.IsLayer)
                    property.intValue = EditorGUI.LayerField(position, content, property.intValue);
                else if (range != null)
                    property.intValue = EditorGUI.IntSlider(position, content, property.intValue, (int)range.min, (int)range.max);
                else
                {
                    property.intValue = EditorGUI.IntField(position, content, property.intValue);
                    if (min != null) property.intValue = Mathf.Max((int)min.min, property.intValue);
                }
                break;
            case SerializedPropertyType.Float:
                property.floatValue = range != null
                    ? EditorGUI.Slider(position, content, property.floatValue, range.min, range.max)
                    : EditorGUI.FloatField(position, content, property.floatValue);
                if (min != null) property.floatValue = Mathf.Max(min.min, property.floatValue);
                break;
            case SerializedPropertyType.Boolean:
                property.boolValue = EditorGUI.Toggle(position, content, property.boolValue);
                break;
            case SerializedPropertyType.Vector3:
                property.vector3Value = EditorGUI.Vector3Field(position, content, property.vector3Value);
                break;
            case SerializedPropertyType.ObjectReference:
                property.objectReferenceValue = EditorGUI.ObjectField(position, content,
                    property.objectReferenceValue, fieldInfo.FieldType, true);
                break;
            case SerializedPropertyType.LayerMask:
                string[] names = new string[32];
                for (int i = 0; i < names.Length; i++)
                {
                    string name = LayerMask.LayerToName(i);
                    names[i] = string.IsNullOrEmpty(name) ? "Unassigned " + i : name;
                }
                property.intValue = EditorGUI.MaskField(position, content, property.intValue, names);
                break;
            default:
                EditorGUI.PropertyField(position, property, content, true);
                break;
        }
        EditorGUI.EndProperty();
    }

    /// <summary>複数行となる項目も収まる高さを返す</summary>
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
        property.propertyType == SerializedPropertyType.Vector3 && !EditorGUIUtility.wideMode
            ? EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing
            : EditorGUIUtility.singleLineHeight;
}
