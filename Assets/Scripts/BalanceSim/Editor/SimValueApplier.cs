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

        public static SimValueApplier Apply(SimValueSource source, Scene scene, List<string> warnings)
        {
            var applier = new SimValueApplier();
            if (source == null)
            {
                return applier;
            }

            if (source is not SimValueSet set)
            {
                throw new InvalidOperationException($"{source.name}（{source.GetType().Name}）はまだ実行できない種類の数値SO です");
            }

            try
            {
                applier.ApplySet(set, scene, warnings);
            }
            catch
            {
                applier.Dispose();
                throw;
            }
            return applier;
        }

        private void ApplySet(SimValueSet set, Scene scene, List<string> warnings)
        {
            var seen = new HashSet<(int, string)>();
            for (int i = 0; i < set.Overrides.Count; i++)
            {
                SimValueOverride entry = set.Overrides[i];
                string row = $"{set.name} の {i + 1} 行目";
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

                if (!seen.Add((target.GetInstanceID(), property.propertyPath)))
                {
                    warnings.Add($"{row}: {entry.TargetLabel} の {entry.PropertyLabel} は上の行でも上書きしています。下の行の値を使います");
                }

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
