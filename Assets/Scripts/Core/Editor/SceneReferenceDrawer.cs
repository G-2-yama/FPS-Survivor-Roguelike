using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneReferenceAttribute))]
public class SceneReferenceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, $"{nameof(SceneReferenceAttribute)} は string にのみ使えます");
            return;
        }

        SceneAsset currentScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(property.stringValue);

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();
        SceneAsset selectedScene = (SceneAsset)EditorGUI.ObjectField(position, label, currentScene, typeof(SceneAsset), false);
        if (EditorGUI.EndChangeCheck())
        {
            property.stringValue = selectedScene != null ? AssetDatabase.GetAssetPath(selectedScene) : string.Empty;
        }
        EditorGUI.EndProperty();
    }
}
