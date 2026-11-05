using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>プレイヤー設定SOに数値検証の説明を表示する</summary>
[CustomEditor(typeof(PlayerSettingsAsset), true)]
public sealed class PlayerSettingsAssetEditor : Editor
{
    private readonly PlayerExperienceGraph experienceGraph = new PlayerExperienceGraph();

    /// <summary>設定項目と不正な設定の日本語説明を描画する</summary>
    public override void OnInspectorGUI()
    {
        if (target is PlayerProgressionSettings settings)
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "levelUpRequiredExp", "levelUpRate");
            using (new EditorGUI.DisabledScope(settings.UsesLevelRequirements))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("levelUpRequiredExp"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("levelUpRate"));
            }
            serializedObject.ApplyModifiedProperties();
        }
        else DrawDefaultInspector();
        if (!((PlayerSettingsAsset)target).Validate(out string error))
            EditorGUILayout.HelpBox(error, MessageType.Error);
        else if (target is PlayerProgressionSettings progression)
            experienceGraph.Draw(progression);
    }
}

/// <summary>レベル別の必要EXPを可視化・編集する。表示操作は設定アセットに保存しない</summary>
internal sealed class PlayerExperienceGraph
{
    private int firstLevel;
    private int levelCount = 50;
    private int selectedLevel;
    private int rangeStartLevel;
    private int rangeTargetLevel;
    private double verticalMaximum;
    private double dragMaximum;
    private int dragIndex = -1;
    private int dragUndoGroup;
    private readonly HashSet<int> selectedLevels = new HashSet<int>();
    private readonly Dictionary<int, float> dragValues = new Dictionary<int, float>();
    private float dragStartY;
    private int selectionTarget;
    private static readonly Color CurveColor = new Color(0.25f, 0.8f, 1f);

    public void Draw(PlayerProgressionSettings settings, int currentLevel = -1, bool editable = true)
    {
        if (settings == null || !settings.Validate(out _)) return;
        if (selectionTarget != settings.GetInstanceID())
        {
            selectedLevels.Clear();
            selectionTarget = settings.GetInstanceID();
        }
        selectedLevels.RemoveWhere(level => level < settings.InitialLevel
            || (long)level - settings.InitialLevel >= settings.EditableLevelCount);
        if (firstLevel == 0) firstLevel = settings.InitialLevel;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("必要経験値の推移", EditorStyles.boldLabel);
        if (settings.InitialLevel == int.MaxValue)
        {
            EditorGUILayout.HelpBox("初期レベルが整数の上限のため、次のレベルはありません。", MessageType.Info);
            return;
        }
        EditorGUILayout.LabelField("横軸：レベル / 縦軸：次のレベルに必要な経験値", EditorStyles.miniLabel);
        firstLevel = Mathf.Clamp(EditorGUILayout.IntField("表示開始レベル", firstLevel),
            settings.InitialLevel, int.MaxValue - 1);
        int maximumCount = (int)System.Math.Min(200L, (long)int.MaxValue - firstLevel);
        levelCount = EditorGUILayout.IntSlider("表示レベル数", Mathf.Min(levelCount, maximumCount),
            Mathf.Min(2, maximumCount), maximumCount);
        int lastLevel = firstLevel + levelCount - 1;
        DrawEditingControls(settings, lastLevel, editable);
        if (settings.UsesLevelRequirements)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("表示範囲を全選択"))
                {
                    selectedLevels.Clear();
                    for (int level = firstLevel; level <= lastLevel; level++)
                        if ((long)level - settings.InitialLevel < settings.EditableLevelCount) selectedLevels.Add(level);
                }
                if (GUILayout.Button("選択を解除")) selectedLevels.Clear();
            }
            EditorGUILayout.LabelField($"選択中：{selectedLevels.Count}点", EditorStyles.miniLabel);
        }
        double maxExperience = 1d;
        for (int i = 0; i < levelCount; i++)
            maxExperience = System.Math.Max(maxExperience, RequiredExperience(settings, firstLevel + i));
        verticalMaximum = EditorGUILayout.DoubleField("縦軸上限（0で自動）", verticalMaximum);
        if (double.IsNaN(verticalMaximum) || double.IsInfinity(verticalMaximum) || verticalMaximum < 0d)
            verticalMaximum = 0d;
        maxExperience = dragIndex >= 0 ? dragMaximum
            : verticalMaximum > 0d ? verticalMaximum : maxExperience * 1.15d;

        Rect area = GUILayoutUtility.GetRect(0f, 190f, GUILayout.ExpandWidth(true));
        Rect plot = new Rect(area.x + 66f, area.y + 8f,
            Mathf.Max(1f, area.width - 78f), area.height - 34f);
        selectedLevel = Mathf.Clamp(selectedLevel, firstLevel, lastLevel);
        HandleVertices(settings, plot, maxExperience, editable);

        if (Event.current.type == EventType.Repaint)
        {
            EditorGUI.DrawRect(plot, EditorGUIUtility.isProSkin
                ? new Color(0.12f, 0.12f, 0.12f) : new Color(0.9f, 0.9f, 0.9f));
            Handles.BeginGUI();
            Color previousColor = Handles.color;
            try
            {
                Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.35f);
                for (int i = 0; i <= 4; i++)
                {
                    float y = plot.yMax - plot.height * i / 4f;
                    Handles.DrawLine(new Vector3(plot.x, y), new Vector3(plot.xMax, y));
                    GUI.Label(new Rect(area.x, y - 8f, 63f, 18f),
                        Format(maxExperience * i / 4d), EditorStyles.miniLabel);
                }
                var points = new Vector3[levelCount];
                for (int i = 0; i < levelCount; i++)
                    points[i] = Point(plot, i, levelCount, System.Math.Min(1d,
                        RequiredExperience(settings, firstLevel + i) / maxExperience));
                Handles.color = CurveColor;
                if (points.Length > 1) Handles.DrawAAPolyLine(2f, points);
                for (int i = 0; i < points.Length; i++)
                {
                    bool selected = selectedLevels.Contains(firstLevel + i);
                    float size = selected ? 7f : 4f;
                    EditorGUI.DrawRect(new Rect(points[i].x - size / 2f, points[i].y - size / 2f, size, size),
                        selected ? new Color(1f, 0.55f, 0.2f) : CurveColor);
                }

                if (currentLevel >= firstLevel && currentLevel <= lastLevel)
                {
                    float x = points[currentLevel - firstLevel].x;
                    Handles.color = new Color(1f, 0.75f, 0.25f);
                    Handles.DrawLine(new Vector3(x, plot.y), new Vector3(x, plot.yMax));
                }
                Vector3 selectedPoint = points[selectedLevel - firstLevel];
                EditorGUI.DrawRect(new Rect(selectedPoint.x - 3f, selectedPoint.y - 3f, 6f, 6f), CurveColor);
            }
            finally
            {
                Handles.color = previousColor;
                Handles.EndGUI();
            }
            GUI.Label(new Rect(plot.x, plot.yMax + 3f, 100f, 18f), $"Lv {firstLevel}", EditorStyles.miniLabel);
            GUI.Label(new Rect(plot.xMax - 100f, plot.yMax + 3f, 100f, 18f),
                $"Lv {lastLevel}", new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.UpperRight });
        }

        selectedLevel = EditorGUILayout.IntSlider("確認するレベル", selectedLevel, firstLevel, lastLevel);
        EditorGUILayout.LabelField($"Lv {selectedLevel} → {selectedLevel + 1}",
            $"{Format(RequiredExperience(settings, selectedLevel))} EXP");
        if (settings.UsesLevelRequirements && (long)selectedLevel - settings.InitialLevel < settings.EditableLevelCount)
        {
            using (new EditorGUI.DisabledScope(!editable))
            {
                EditorGUI.BeginChangeCheck();
                float value = EditorGUILayout.FloatField("選択レベルの必要EXP", (float)RequiredExperience(settings, selectedLevel));
                if (EditorGUI.EndChangeCheck() && !float.IsNaN(value) && !float.IsInfinity(value))
                    SetRequirement(settings, selectedLevel, Mathf.Max(1f, value));
            }
        }
        DrawRangeTotal(settings);
        if (currentLevel >= 0)
            EditorGUILayout.LabelField("現在のレベル（黄色線）", $"Lv {currentLevel}");
        else if (Application.isPlaying)
            EditorGUILayout.HelpBox("設定アセットのプレビューです。変更は次回のプレイ開始時に反映されます。", MessageType.Info);
        EditorGUILayout.HelpBox(settings.UsesLevelRequirements
            ? editable ? "点を上下にドラッグして編集。Shift+クリックで複数選択・解除し、選択済みの点をドラッグすると一括移動します。Ctrl+Zで元に戻せます。"
                : "Play開始時の設定を表示しています。実行中のグラフは読み取り専用です。"
            : "現在は倍率式です。編集用データを作成すると各レベルの点を動かせます。", MessageType.None);
    }

    private void DrawEditingControls(PlayerProgressionSettings settings, int lastLevel, bool editable)
    {
        using (new EditorGUI.DisabledScope(!editable))
        {
            if (!settings.UsesLevelRequirements)
            {
                if (GUILayout.Button("倍率式から編集用データを作成"))
                {
                    int count = (int)System.Math.Min(10000L, (long)lastLevel - settings.InitialLevel + 1);
                    var values = new float[count];
                    for (int i = 0; i < count; i++)
                        values[i] = (float)RequiredExperience(settings, settings.InitialLevel + i);
                    SetTable(settings, values);
                }
            }
            else
            {
                int maxCount = (int)System.Math.Min(10000L, (long)int.MaxValue - settings.InitialLevel);
                int count = Mathf.Clamp(EditorGUILayout.DelayedIntField("編集するレベル数", settings.EditableLevelCount), 1, maxCount);
                if (count != settings.EditableLevelCount)
                {
                    float[] values = settings.CopyLevelRequirements();
                    float last = values[values.Length - 1];
                    int previousCount = values.Length;
                    System.Array.Resize(ref values, count);
                    for (int i = previousCount; i < count; i++) values[i] = last;
                    SetTable(settings, values);
                }
                if (GUILayout.Button("倍率式に戻す（編集データを解除）")) SetTable(settings, new float[0]);
            }
        }
        if (settings.UsesLevelRequirements)
            EditorGUILayout.HelpBox($"Lv {(long)settings.InitialLevel + settings.EditableLevelCount - 1}以降は最後の点の必要EXPで一定です。"
                + "先のレベルも編集する場合は編集レベル数を増やしてください。", MessageType.None);
    }

    private void HandleVertices(PlayerProgressionSettings settings, Rect plot, double maximum, bool editable)
    {
        int control = GUIUtility.GetControlID(FocusType.Passive);
        Event evt = Event.current;
        if (evt.type == EventType.MouseDown && evt.button == 0
            && new Rect(plot.x - 6f, plot.y - 6f, plot.width + 12f, plot.height + 12f).Contains(evt.mousePosition))
        {
            float nearestDistance = 100f;
            int nearest = -1;
            for (int i = 0; i < levelCount; i++)
            {
                Vector3 point = Point(plot, i, levelCount, System.Math.Min(1d,
                    RequiredExperience(settings, firstLevel + i) / maximum));
                float distance = ((Vector2)point - evt.mousePosition).sqrMagnitude;
                if (distance < nearestDistance) { nearestDistance = distance; nearest = i; }
            }
            if (nearest < 0) return;
            selectedLevel = firstLevel + nearest;
            if (editable && settings.UsesLevelRequirements
                && (long)selectedLevel - settings.InitialLevel < settings.EditableLevelCount)
            {
                if (evt.shift)
                {
                    if (!selectedLevels.Add(selectedLevel))
                    {
                        selectedLevels.Remove(selectedLevel);
                        GUI.changed = true;
                        evt.Use();
                        return;
                    }
                }
                else if (!selectedLevels.Contains(selectedLevel))
                {
                    selectedLevels.Clear();
                    selectedLevels.Add(selectedLevel);
                }
                GUIUtility.hotControl = control;
                dragIndex = selectedLevel;
                dragMaximum = maximum;
                dragStartY = evt.mousePosition.y;
                dragValues.Clear();
                foreach (int level in selectedLevels)
                    dragValues.Add(level, (float)settings.RequiredAtLevel(level));
                Undo.IncrementCurrentGroup();
                dragUndoGroup = Undo.GetCurrentGroup();
                Undo.SetCurrentGroupName("選択した必要EXPの点を移動");
            }
            GUI.changed = true;
            evt.Use();
        }
        else if (GUIUtility.hotControl == control && dragIndex >= 0)
        {
            if (evt.type == EventType.MouseDrag)
            {
                double delta = (dragStartY - evt.mousePosition.y) / plot.height * dragMaximum;
                double minimum = float.MaxValue;
                double maximumValue = 1d;
                foreach (float value in dragValues.Values)
                {
                    minimum = System.Math.Min(minimum, value);
                    maximumValue = System.Math.Max(maximumValue, value);
                }
                delta = System.Math.Max(1d - minimum, System.Math.Min(float.MaxValue - maximumValue, delta));
                var serialized = new SerializedObject(settings);
                serialized.Update();
                SerializedProperty table = serialized.FindProperty("requiredExpByLevel");
                foreach (var pair in dragValues)
                    table.GetArrayElementAtIndex(pair.Key - settings.InitialLevel).floatValue = (float)(pair.Value + delta);
                serialized.ApplyModifiedProperties();
                GUI.changed = true;
                evt.Use();
            }
            else if (evt.type == EventType.MouseUp)
            {
                GUIUtility.hotControl = 0;
                dragIndex = -1;
                dragValues.Clear();
                Undo.CollapseUndoOperations(dragUndoGroup);
                evt.Use();
            }
        }
    }

    private static void SetRequirement(PlayerProgressionSettings settings, int level, float value)
    {
        var serialized = new SerializedObject(settings);
        serialized.Update();
        serialized.FindProperty("requiredExpByLevel").GetArrayElementAtIndex(level - settings.InitialLevel).floatValue = value;
        serialized.ApplyModifiedProperties();
    }

    private static void SetTable(PlayerProgressionSettings settings, float[] values)
    {
        var serialized = new SerializedObject(settings);
        serialized.Update();
        SerializedProperty table = serialized.FindProperty("requiredExpByLevel");
        table.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) table.GetArrayElementAtIndex(i).floatValue = values[i];
        serialized.ApplyModifiedProperties();
    }

    /// <summary>開始レベルの経験値0から到達レベルまでの合計を表示する</summary>
    private void DrawRangeTotal(PlayerProgressionSettings settings)
    {
        if (rangeStartLevel == 0)
        {
            rangeStartLevel = settings.InitialLevel;
            rangeTargetLevel = (int)System.Math.Min(int.MaxValue, (long)rangeStartLevel + 49);
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("区間の累計必要EXP", EditorStyles.boldLabel);
        rangeStartLevel = Mathf.Clamp(EditorGUILayout.IntField("開始レベル", rangeStartLevel),
            settings.InitialLevel, int.MaxValue);
        rangeTargetLevel = Mathf.Clamp(EditorGUILayout.IntField("到達レベル", rangeTargetLevel),
            rangeStartLevel, int.MaxValue);
        double total = RequiredForRange(settings, rangeStartLevel, rangeTargetLevel);
        string totalText = double.IsInfinity(total) ? "計算可能な範囲を超えています" : total.ToString("G10") + " EXP";
        EditorGUILayout.LabelField($"Lv {rangeStartLevel} → {rangeTargetLevel} の合計", totalText);
        EditorGUILayout.LabelField("開始レベルの経験値0から計算（現在の所持EXPは差し引きません）", EditorStyles.wordWrappedMiniLabel);
    }

    // 実際のレベルアップと同じ設定で、到達レベル直前までを合計する。
    internal static double RequiredForRange(PlayerProgressionSettings settings, int startLevel, int targetLevel)
    {
        return settings.RequiredForRange(startLevel, targetLevel);
    }

    private static Vector3 Point(Rect plot, int index, int count, double normalizedExperience) =>
        new Vector3(plot.x + plot.width * index / Mathf.Max(1, count - 1),
            plot.yMax - plot.height * (float)normalizedExperience);

    // ゲーム本体と同じ設定を評価し、描画可能なfloat範囲に収める。
    private static double RequiredExperience(PlayerProgressionSettings settings, int level) =>
        System.Math.Min(float.MaxValue, settings.RequiredAtLevel(level));

    private static string Format(double value) => value >= 1000000d ? value.ToString("0.##E+0") : value.ToString("0.##");
}
