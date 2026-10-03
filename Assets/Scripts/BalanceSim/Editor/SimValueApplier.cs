using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BalanceSim.Editor
{
    public sealed class SimValueApplier : IDisposable
    {
        private readonly struct Original
        {
            public readonly UnityEngine.Object Target;
            public readonly string PropertyPath;
            public readonly double Value;
            public readonly bool WasDirty;
            public readonly bool RevertPrefabOverride;

            public Original(UnityEngine.Object target, SerializedProperty property)
            {
                Target = target;
                PropertyPath = property.propertyPath;
                Value = SimValueOverride.Read(property);
                WasDirty = EditorUtility.IsDirty(target);
                RevertPrefabOverride = PrefabUtility.IsPartOfPrefabInstance(target) && !property.prefabOverride;
            }
        }

        private readonly List<Original> _originals = new();

        private SimValueApplier()
        {
        }

        public static SimValueApplier Apply(IEnumerable<SimValueRow> rows, Scene scene, List<string> warnings)
        {
            var applier = new SimValueApplier();
            try
            {
                applier.ApplyRows(rows, scene, warnings);
            }
            catch
            {
                applier.Dispose();
                throw;
            }
            return applier;
        }

        private void ApplyRows(IEnumerable<SimValueRow> rows, Scene scene, List<string> warnings)
        {
            var seen = new Dictionary<(int, string), string>();
            foreach (SimValueRow valueRow in rows)
            {
                SimValueOverride entry = valueRow.Value;
                string row = valueRow.Row;
                if (string.IsNullOrEmpty(entry.TargetId))
                {
                    throw new InvalidOperationException($"{row}: 対象が設定されていません");
                }
                if (string.IsNullOrEmpty(entry.PropertyPath))
                {
                    throw new InvalidOperationException($"{row}（{entry.TargetLabel}）: 項目が選ばれていません");
                }

                UnityEngine.Object target = SimValueOverride.ResolveTarget(entry.TargetId);
                if (target == null)
                {
                    throw new InvalidOperationException($"{row}: 対象 {entry.TargetLabel} が見つかりません。削除されたか、対象シーン以外のシーンのオブジェクトの可能性があります");
                }
                if (target is Component component && component.gameObject.scene.IsValid() && component.gameObject.scene != scene)
                {
                    throw new InvalidOperationException($"{row}: 対象 {entry.TargetLabel} は対象シーン（{scene.name}）のオブジェクトではありません");
                }

                var serialized = new SerializedObject(target);
                SerializedProperty property = serialized.FindProperty(entry.PropertyPath);
                if (property == null)
                {
                    throw new InvalidOperationException($"{row}: {entry.TargetLabel} の {entry.PropertyLabel}（{entry.PropertyPath}）が見つかりません。フィールド名が変わった可能性があります");
                }
                if (!SimValueOverride.IsSupported(property.propertyType))
                {
                    throw new InvalidOperationException($"{row}: {entry.TargetLabel} の {entry.PropertyLabel} は数値でも真偽値でもありません");
                }

                var key = (target.GetInstanceID(), property.propertyPath);
                if (seen.TryGetValue(key, out string previousRow))
                {
                    warnings.Add($"{row}: {entry.TargetLabel} の {entry.PropertyLabel} は {previousRow} でも上書きしています。{row} の値を使います");
                }
                seen[key] = row;

                _originals.Add(new Original(target, property));
                SimValueOverride.Write(property, entry.Value);
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        public void Dispose()
        {
            for (int i = _originals.Count - 1; i >= 0; i--)
            {
                Original original = _originals[i];
                if (original.Target == null)
                {
                    continue;
                }

                var serialized = new SerializedObject(original.Target);
                SerializedProperty property = serialized.FindProperty(original.PropertyPath);
                if (original.RevertPrefabOverride)
                {
                    PrefabUtility.RevertPropertyOverride(property, InteractionMode.AutomatedAction);
                }
                else
                {
                    SimValueOverride.Write(property, original.Value);
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }

                if (!original.WasDirty)
                {
                    EditorUtility.ClearDirty(original.Target);
                }
            }
            _originals.Clear();
        }
    }
}
