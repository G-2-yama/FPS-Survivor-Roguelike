using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LegacyTextToTmpConverter
{
    private const string MenuRoot = "Tools/旧Text→TMP変換/";
    private const string BuiltinFontGuid = "0000000000000000e000000000000000";
    private const string BuiltinFontReplacementPath = "Assets/Font/SoukouMincho-Font/SoukouMincho SDF.asset";
    private static readonly string RecordPath = Path.Combine("Library", "LegacyTextToTmpReferences.json");

    [Serializable]
    private class ReferenceRecord
    {
        public string context;
        public string ownerPath;
        public string ownerType;
        public int ownerIndex;
        public string propertyPath;
        public string targetPath;
    }

    [Serializable]
    private class RecordFile
    {
        public List<ReferenceRecord> references = new List<ReferenceRecord>();
    }

    [MenuItem(MenuRoot + "1. シーンとプレハブを変換")]
    private static void Convert()
    {
        if (File.Exists(RecordPath))
        {
            Debug.LogError($"[TMP変換] 前回の参照の記録が残っています。先に「2. 参照を付け直す」を実行するか、{RecordPath} を削除してください");
            return;
        }
        if (!PrepareBatch(out SceneSetup[] setup))
        {
            return;
        }

        RecordFile record = new RecordFile();
        StringBuilder log = new StringBuilder();
        Dictionary<Font, TMP_FontAsset> fontMap = new Dictionary<Font, TMP_FontAsset>();
        int total = 0;
        int converted = 0;

        try
        {
            foreach (string scenePath in GetScenePaths())
            {
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                ConvertRoots(scene.GetRootGameObjects(), scenePath, fontMap, record, log, ref total, ref converted);
                File.WriteAllText(RecordPath, JsonUtility.ToJson(record, true));
                EditorSceneManager.SaveScene(scene);
            }

            foreach (string prefabPath in GetPrefabPathsWithLegacyText())
            {
                GameObject contents = PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    ConvertRoots(new[] { contents }, prefabPath, fontMap, record, log, ref total, ref converted);
                    File.WriteAllText(RecordPath, JsonUtility.ToJson(record, true));
                    PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }
        }
        finally
        {
            EditorSceneManager.RestoreSceneManagerSetup(setup);
        }

        Debug.Log($"[TMP変換] 旧Text {total} 個中 {converted} 個を変換、参照 {record.references.Count} 件を記録\n{log}");
    }

    [MenuItem(MenuRoot + "2. 参照を付け直す")]
    private static void Rebind()
    {
        if (!File.Exists(RecordPath))
        {
            Debug.LogError("[TMP変換] 参照の記録がありません。先に「1. シーンとプレハブを変換」を実行してください");
            return;
        }
        if (!PrepareBatch(out SceneSetup[] setup))
        {
            return;
        }

        RecordFile record = JsonUtility.FromJson<RecordFile>(File.ReadAllText(RecordPath));
        StringBuilder log = new StringBuilder();
        int rebound = 0;

        try
        {
            foreach (IGrouping<string, ReferenceRecord> group in record.references.GroupBy(reference => reference.context))
            {
                if (group.Key.EndsWith(".unity"))
                {
                    Scene scene = EditorSceneManager.OpenScene(group.Key, OpenSceneMode.Single);
                    GameObject[] roots = scene.GetRootGameObjects();
                    rebound += group.Count(reference => RebindReference(roots, reference, log));
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
                else
                {
                    GameObject contents = PrefabUtility.LoadPrefabContents(group.Key);
                    try
                    {
                        GameObject[] roots = { contents };
                        rebound += group.Count(reference => RebindReference(roots, reference, log));
                        PrefabUtility.SaveAsPrefabAsset(contents, group.Key);
                    }
                    finally
                    {
                        PrefabUtility.UnloadPrefabContents(contents);
                    }
                }
            }
        }
        finally
        {
            EditorSceneManager.RestoreSceneManagerSetup(setup);
        }

        if (rebound == record.references.Count)
        {
            File.Delete(RecordPath);
        }
        Debug.Log($"[TMP変換] 参照 {record.references.Count} 件中 {rebound} 件を付け直した\n{log}");
    }

    private static bool PrepareBatch(out SceneSetup[] setup)
    {
        setup = null;
        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            Debug.LogError("[TMP変換] プレハブの編集画面を閉じてから実行してください");
            return false;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return false;
        }
        setup = EditorSceneManager.GetSceneManagerSetup();
        return true;
    }

    private static IEnumerable<string> GetScenePaths()
    {
        return EditorBuildSettings.scenes.Select(scene => scene.path).Where(File.Exists);
    }

    private static IEnumerable<string> GetPrefabPathsWithLegacyText()
    {
        return AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path =>
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                return prefab != null && prefab.GetComponentsInChildren<Text>(true).Any(text => !IsNestedPrefabPart(prefab, text));
            })
            .ToList();
    }

    private static bool IsNestedPrefabPart(GameObject prefabRoot, Component component)
    {
        GameObject nearestRoot = PrefabUtility.GetNearestPrefabInstanceRoot(component);
        return nearestRoot != null && nearestRoot != prefabRoot;
    }

    private static void ConvertRoots(GameObject[] roots, string context, Dictionary<Font, TMP_FontAsset> fontMap, RecordFile record, StringBuilder log, ref int total, ref int converted)
    {
        List<Text> targets = new List<Text>();
        foreach (Text text in roots.SelectMany(root => root.GetComponentsInChildren<Text>(true)))
        {
            total++;
            string name = $"{context} : {GetHierarchyName(text.transform)}";
            if (PrefabUtility.IsPartOfPrefabInstance(text))
            {
                log.AppendLine($"変換しない（プレハブのインスタンスの中。元のプレハブ側で変換する）: {name}");
                continue;
            }
            if (ResolveFontAsset(text.font, fontMap) == null)
            {
                log.AppendLine($"変換しない（対応する TMP フォントアセットがない: {(text.font != null ? text.font.name : "なし")}）: {name}");
                continue;
            }
            targets.Add(text);
        }

        HashSet<Text> targetSet = new HashSet<Text>(targets);
        foreach (Component component in roots.SelectMany(root => root.GetComponentsInChildren<Component>(true)))
        {
            if (component == null || component is Text)
            {
                continue;
            }
            bool isInstance = PrefabUtility.IsPartOfPrefabInstance(component);
            SerializedProperty property = new SerializedObject(component).GetIterator();
            while (property.Next(true))
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference)
                {
                    continue;
                }
                if (!(property.objectReferenceValue is Text referenced) || !targetSet.Contains(referenced))
                {
                    continue;
                }
                if (isInstance && !property.prefabOverride)
                {
                    continue;
                }
                record.references.Add(new ReferenceRecord
                {
                    context = context,
                    ownerPath = GetIndexPath(roots, component.transform),
                    ownerType = component.GetType().FullName,
                    ownerIndex = Array.IndexOf(component.GetComponents(component.GetType()), component),
                    propertyPath = property.propertyPath,
                    targetPath = GetIndexPath(roots, referenced.transform),
                });
                if (property.propertyPath.Contains("m_PersistentCalls"))
                {
                    log.AppendLine($"要確認（UnityEvent から参照）: {context} : {GetHierarchyName(component.transform)} の {component.GetType().Name}.{property.propertyPath}");
                }
            }
        }

        foreach (Text text in targets)
        {
            GameObject go = text.gameObject;
            string name = $"{context} : {GetHierarchyName(go.transform)}";
            Settings settings = Settings.From(text, fontMap[text.font]);
            RectTransform rect = go.GetComponent<RectTransform>();
            Vector2 sizeDelta = rect.sizeDelta;

            foreach (BaseMeshEffect effect in go.GetComponents<BaseMeshEffect>())
            {
                log.AppendLine($"要確認（TMP では効かない {effect.GetType().Name} が付いている）: {name}");
            }
            if (!Mathf.Approximately(text.lineSpacing, 1f))
            {
                log.AppendLine($"要確認（行間 {text.lineSpacing} を換算した）: {name}");
            }

            UnityEngine.Object.DestroyImmediate(text);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            if (tmp == null)
            {
                log.AppendLine($"失敗（TextMeshProUGUI を追加できなかった）: {name}");
                continue;
            }
            settings.ApplyTo(tmp);
            rect.sizeDelta = sizeDelta;
            converted++;
        }
    }

    private static bool RebindReference(GameObject[] roots, ReferenceRecord reference, StringBuilder log)
    {
        string name = $"{reference.context} : {reference.ownerPath} の {reference.ownerType}.{reference.propertyPath}";
        Transform ownerTransform = FindByIndexPath(roots, reference.ownerPath);
        Transform targetTransform = FindByIndexPath(roots, reference.targetPath);
        Component owner = ownerTransform != null
            ? ownerTransform.GetComponents<Component>().Where(component => component != null && component.GetType().FullName == reference.ownerType).ElementAtOrDefault(reference.ownerIndex)
            : null;
        TMP_Text tmp = targetTransform != null ? targetTransform.GetComponent<TMP_Text>() : null;
        if (owner == null || tmp == null)
        {
            log.AppendLine($"失敗（参照元か参照先が見つからない）: {name}");
            return false;
        }

        SerializedObject serialized = new SerializedObject(owner);
        SerializedProperty property = serialized.FindProperty(reference.propertyPath);
        if (property == null)
        {
            log.AppendLine($"失敗（プロパティが見つからない）: {name}");
            return false;
        }
        property.objectReferenceValue = tmp;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        serialized.Update();
        if (serialized.FindProperty(reference.propertyPath).objectReferenceValue != tmp)
        {
            log.AppendLine($"失敗（型が合わず代入できない）: {name}");
            return false;
        }
        return true;
    }

    private static TMP_FontAsset ResolveFontAsset(Font font, Dictionary<Font, TMP_FontAsset> fontMap)
    {
        if (font == null)
        {
            return null;
        }
        if (fontMap.TryGetValue(font, out TMP_FontAsset cached))
        {
            return cached;
        }

        TMP_FontAsset found = null;
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(font, out string fontGuid, out long _);
        if (fontGuid == BuiltinFontGuid)
        {
            found = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BuiltinFontReplacementPath);
        }
        else
        {
            foreach (string guid in AssetDatabase.FindAssets("t:TMP_FontAsset"))
            {
                TMP_FontAsset candidate = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (candidate == null)
                {
                    continue;
                }
                SerializedProperty sourceGuid = new SerializedObject(candidate).FindProperty("m_SourceFontFileGUID");
                if (sourceGuid != null && sourceGuid.stringValue == fontGuid)
                {
                    found = candidate;
                    break;
                }
            }
        }

        if (found != null)
        {
            fontMap[font] = found;
        }
        return found;
    }

    private static string GetIndexPath(GameObject[] roots, Transform transform)
    {
        List<int> indices = new List<int>();
        while (transform.parent != null)
        {
            indices.Add(transform.GetSiblingIndex());
            transform = transform.parent;
        }
        indices.Add(Array.IndexOf(roots, transform.gameObject));
        indices.Reverse();
        return string.Join("/", indices);
    }

    private static Transform FindByIndexPath(GameObject[] roots, string path)
    {
        int[] indices = path.Split('/').Select(int.Parse).ToArray();
        if (indices[0] < 0 || indices[0] >= roots.Length)
        {
            return null;
        }
        Transform current = roots[indices[0]].transform;
        foreach (int index in indices.Skip(1))
        {
            if (index >= current.childCount)
            {
                return null;
            }
            current = current.GetChild(index);
        }
        return current;
    }

    private static string GetHierarchyName(Transform transform)
    {
        return transform.parent == null ? transform.name : GetHierarchyName(transform.parent) + "/" + transform.name;
    }

    private struct Settings
    {
        private string text;
        private TMP_FontAsset fontAsset;
        private float fontSize;
        private FontStyles fontStyle;
        private Color color;
        private TextAlignmentOptions alignment;
        private bool richText;
        private bool raycastTarget;
        private bool maskable;
        private bool autoSize;
        private float autoSizeMin;
        private float autoSizeMax;
        private TextWrappingModes wrapping;
        private TextOverflowModes overflow;
        private float lineSpacing;

        public static Settings From(Text text, TMP_FontAsset fontAsset)
        {
            return new Settings
            {
                text = text.text,
                fontAsset = fontAsset,
                fontSize = text.fontSize,
                fontStyle = ToFontStyles(text.fontStyle),
                color = text.color,
                alignment = ToAlignment(text.alignment),
                richText = text.supportRichText,
                raycastTarget = text.raycastTarget,
                maskable = text.maskable,
                autoSize = text.resizeTextForBestFit,
                autoSizeMin = text.resizeTextMinSize,
                autoSizeMax = text.resizeTextMaxSize,
                wrapping = text.horizontalOverflow == HorizontalWrapMode.Wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap,
                overflow = text.verticalOverflow == VerticalWrapMode.Truncate ? TextOverflowModes.Truncate : TextOverflowModes.Overflow,
                lineSpacing = (text.lineSpacing - 1f) * 100f,
            };
        }

        public void ApplyTo(TextMeshProUGUI tmp)
        {
            tmp.font = fontAsset;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = fontStyle;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.richText = richText;
            tmp.raycastTarget = raycastTarget;
            tmp.maskable = maskable;
            tmp.enableAutoSizing = autoSize;
            tmp.fontSizeMin = autoSizeMin;
            tmp.fontSizeMax = autoSizeMax;
            tmp.textWrappingMode = wrapping;
            tmp.overflowMode = overflow;
            tmp.lineSpacing = lineSpacing;
        }

        private static FontStyles ToFontStyles(FontStyle style)
        {
            switch (style)
            {
                case FontStyle.Bold: return FontStyles.Bold;
                case FontStyle.Italic: return FontStyles.Italic;
                case FontStyle.BoldAndItalic: return FontStyles.Bold | FontStyles.Italic;
                default: return FontStyles.Normal;
            }
        }

        private static TextAlignmentOptions ToAlignment(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                default: return TextAlignmentOptions.Center;
            }
        }
    }
}
