using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BalanceSim.Editor
{
    internal static class ManagedReferencePopup
    {
        private static readonly Dictionary<Type, Type[]> TypesByBase = new();

        public static void Draw<T>(SerializedProperty property, string popupLabel, string fieldLabel)
        {
            if (!TypesByBase.TryGetValue(typeof(T), out Type[] types))
            {
                types = TypeCache.GetTypesDerivedFrom<T>()
                    .Where(t => !t.IsAbstract && !t.IsGenericType && t.IsSerializable && t.GetConstructor(Type.EmptyTypes) != null)
                    .OrderBy(t => t.Name)
                    .ToArray();
                TypesByBase[typeof(T)] = types;
            }

            Type current = property.managedReferenceValue?.GetType();
            int index = Array.IndexOf(types, current);
            string[] labels = types.Select(t => t.Name).ToArray();

            int selected = EditorGUILayout.Popup(new GUIContent(popupLabel, property.tooltip), index, labels);
            if (selected != index && selected >= 0)
            {
                property.managedReferenceValue = Activator.CreateInstance(types[selected]);
            }

            if (property.managedReferenceValue != null)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(fieldLabel), true);
            }
        }
    }
}
