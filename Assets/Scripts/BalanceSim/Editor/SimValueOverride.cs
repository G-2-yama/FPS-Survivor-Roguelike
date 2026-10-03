using System;
using UnityEditor;
using UnityEngine;

namespace BalanceSim.Editor
{
    [Serializable]
    public class SimValueOverride
    {
        public const string TargetIdField = nameof(targetId);
        public const string TargetLabelField = nameof(targetLabel);
        public const string PropertyPathField = nameof(propertyPath);
        public const string PropertyLabelField = nameof(propertyLabel);
        public const string ValueField = nameof(value);

        [SerializeField] private string targetId;
        [SerializeField] private string targetLabel;
        [SerializeField] private string propertyPath;
        [SerializeField] private string propertyLabel;
        [SerializeField] private double value;

        public string TargetId => targetId;
        public string TargetLabel => targetLabel;
        public string PropertyPath => propertyPath;
        public string PropertyLabel => propertyLabel;
        public double Value => value;

        public static UnityEngine.Object ResolveTarget(string id)
        {
            if (string.IsNullOrEmpty(id) || !GlobalObjectId.TryParse(id, out GlobalObjectId globalId))
            {
                return null;
            }
            return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId);
        }

        public static string TargetIdOf(UnityEngine.Object target)
        {
            return GlobalObjectId.GetGlobalObjectIdSlow(target).ToString();
        }

        public static string TargetLabelOf(UnityEngine.Object target)
        {
            if (target is Component component)
            {
                string owner = component.gameObject.scene.IsValid()
                    ? $"{component.gameObject.scene.name}: {component.gameObject.name}"
                    : component.transform.root.name == component.gameObject.name
                        ? component.gameObject.name
                        : $"{component.transform.root.name}/{component.gameObject.name}";
                return $"{owner} ({component.GetType().Name})";
            }
            return target.name;
        }

        public static bool IsSupported(SerializedPropertyType type)
        {
            return type == SerializedPropertyType.Integer
                || type == SerializedPropertyType.Float
                || type == SerializedPropertyType.Boolean;
        }

        public static double Read(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.longValue;
                case SerializedPropertyType.Float:
                    return property.doubleValue;
                case SerializedPropertyType.Boolean:
                    return property.boolValue ? 1 : 0;
                default:
                    throw new InvalidOperationException($"{property.propertyPath} は数値でも真偽値でもありません");
            }
        }

        public static void Write(SerializedProperty property, double newValue)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.longValue = (long)Math.Round(newValue);
                    break;
                case SerializedPropertyType.Float:
                    property.doubleValue = newValue;
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = newValue != 0;
                    break;
                default:
                    throw new InvalidOperationException($"{property.propertyPath} は数値でも真偽値でもありません");
            }
        }

        public static string Format(SerializedPropertyType type, double number)
        {
            switch (type)
            {
                case SerializedPropertyType.Boolean:
                    return number != 0 ? "オン" : "オフ";
                case SerializedPropertyType.Integer:
                    return ((long)Math.Round(number)).ToString();
                default:
                    return number.ToString("0.#####");
            }
        }
    }
}
