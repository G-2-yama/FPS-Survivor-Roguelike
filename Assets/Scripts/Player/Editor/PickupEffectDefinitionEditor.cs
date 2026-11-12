using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PickupEffectDefinition))]
public sealed class PickupEffectDefinitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mode"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("effects"), true);
        if (serializedObject.FindProperty("mode").enumValueIndex == (int)PickupEffectMode.Timed)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("duration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("reacquireMode"));
        }
        serializedObject.ApplyModifiedProperties();
        if (!((PickupEffectDefinition)target).TryValidate(out string error)) EditorGUILayout.HelpBox(error, MessageType.Error);
    }
}