using System;
using System.Collections.Generic;
using System.Linq;
using EnemyBalance;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>任意個数のPhaseをGeneratorから読み取り、時間軸上で編集・試算する。</summary>
public sealed class EnemySpawnTimelineWindow : EditorWindow
{
    [SerializeField] private EnemyGenerator generator;
    [SerializeField] private PhaseData selected;
    [SerializeField] private bool autoDuration = true;
    [SerializeField] private float duration = 900;
    [SerializeField] private float killsPerSecond;
    [SerializeField] private float zoom = 1;
    private readonly List<PhaseData> phases = new List<PhaseData>();
    private EnemySpawnPreview.Result result;
    private Vector2 bodyScroll, graphScroll, listScroll, editorScroll;
    private int capacity, signature;
    private bool dirty = true;
    private string sourceError, calculationError;
    private double yMaximum = 10;
    private enum DragKind { None, Boundary, Minimum, Rate }
    private DragKind drag;
    private PhaseData dragPhase;
    private int dragControl, undoGroup;
    private float dragY, dragRate;
    private Rect minimumHandle, rateHandle;
    private static readonly Color CurveColor = new Color(1f, 0.86f, 0.28f);
    private static readonly Color MinimumColor = new Color(0.3f, 0.9f, 0.75f);

    [MenuItem("Tools/Enemy/出現量タイムライン")]
    public static void Open() => GetWindow<EnemySpawnTimelineWindow>("敵出現量タイムライン");

    private void OnEnable()
    {
        minSize = new Vector2(900, 540);
        Undo.undoRedoPerformed += Invalidate;
        EditorApplication.projectChanged += Invalidate;
        EditorApplication.hierarchyChanged += Invalidate;
        if (generator == null) FindGenerator();
        dirty = true;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= Invalidate;
        EditorApplication.projectChanged -= Invalidate;
        EditorApplication.hierarchyChanged -= Invalidate;
        EndDrag();
    }

    private void OnInspectorUpdate() => Repaint();
    private void Invalidate() { dirty = true; Repaint(); }

    private void FindGenerator()
    {
        generator = FindObjectsByType<EnemyGenerator>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OrderByDescending(g => new SerializedObject(g).FindProperty("phases").arraySize).FirstOrDefault();
        selected = null;
        dirty = true;
    }

    private void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            EditorGUI.BeginChangeCheck();
            generator = (EnemyGenerator)EditorGUILayout.ObjectField("対象Generator", generator, typeof(EnemyGenerator), true);
            if (EditorGUI.EndChangeCheck()) { EndDrag(); selected = null; dirty = true; }
            if (GUILayout.Button("検索", EditorStyles.toolbarButton, GUILayout.Width(50))) FindGenerator();
            if (GUILayout.Button("再計算", EditorStyles.toolbarButton, GUILayout.Width(60))) dirty = true;
        }
        if (generator == null)
        {
            EditorGUILayout.HelpBox("対象Sceneを開き、敵のEnemyGeneratorを指定してください。登録したPhaseの数や名前にかかわらず自動で時間軸を作ります。", MessageType.Info);
            if (GUILayout.Button("GameSceneを開く"))
            {
                const string path = "Assets/Scenes/GameScene.unity";
                if (!SceneManager.GetSceneByPath(path).isLoaded) EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                FindGenerator();
            }
            return;
        }

        using (var scroll = new EditorGUILayout.ScrollViewScope(bodyScroll))
        {
            bodyScroll = scroll.scrollPosition;
            DrawBody();
        }
    }

    private void DrawBody()
    {
        ReadSource();
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUI.BeginChangeCheck();
            autoDuration = GUILayout.Toggle(autoDuration, "表示時間を自動設定", GUILayout.Width(155));
            using (new EditorGUI.DisabledScope(autoDuration))
                duration = EditorGUILayout.FloatField("表示秒数", duration, GUILayout.Width(225));
            killsPerSecond = EditorGUILayout.FloatField(new GUIContent("撃破数 / 秒", "一定の撃破速度を仮定します。0なら撃破なし。"), killsPerSecond, GUILayout.Width(220));
            if (EditorGUI.EndChangeCheck()) dirty = true;
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            zoom = EditorGUILayout.Slider("時間軸の拡大", zoom, 1, 8);
            GUILayout.Label($"{phases.Count} Phase / 生成上限 {capacity} 体", GUILayout.Width(220));
        }
        Recalculate();
        string error = sourceError ?? calculationError;
        if (error != null) EditorGUILayout.HelpBox(error, MessageType.Error);
        DrawGraph();
        if (result != null)
            EditorGUILayout.LabelField($"最大生存数 {result.Peak:0.#} 体　　生成総数（補充・入替含む）{result.Spawned:0.#} 体　　上限による入替 {result.Replaced:0.#} 体");
        EditorGUILayout.HelpBox("縦軸＝推定生存敵数。最低数未満なら不足分を補充、それ以外は出現間隔ごとに1体追加。前Phaseの敵を引き継ぎ、最終Phaseは継続。生成成功率100%・指定した一定の撃破速度で計算し、地形・HP・武器性能は含みません。", MessageType.None);
        using (new EditorGUILayout.HorizontalScope())
        {
            DrawPhaseList();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinWidth(350)))
            {
                editorScroll = EditorGUILayout.BeginScrollView(editorScroll, GUILayout.Height(195));
                DrawSelected();
                EditorGUILayout.EndScrollView();
            }
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            {
                if (GUILayout.Button("Phaseを追加…")) AddPhase();
                using (new EditorGUI.DisabledScope(selected == null))
                    if (GUILayout.Button("選択Phaseをリストから外す")) RemoveSelected();
            }
            if (GUILayout.Button("Phaseアセットを保存"))
                foreach (PhaseData phase in phases.Distinct()) AssetDatabase.SaveAssetIfDirty(phase);
        }
        EditorGUILayout.LabelField("編集は共有Phaseアセットへ反映。Ctrl+Zで取り消し可能。Phase追加・除外後はSceneも保存してください。", EditorStyles.miniLabel);
    }

    private void ReadSource()
    {
        sourceError = null;
        var so = new SerializedObject(generator);
        SerializedProperty list = so.FindProperty("phases");
        phases.Clear();
        int hash = generator.GetInstanceID();
        for (int i = 0; i < list.arraySize; i++)
        {
            var phase = list.GetArrayElementAtIndex(i).objectReferenceValue as PhaseData;
            unchecked { hash = hash * 31 + (phase != null ? phase.GetInstanceID() : 0); }
            if (phase == null) { sourceError = "GeneratorのPhasesに空の参照があります。Inspectorで設定または削除してください。"; continue; }
            phases.Add(phase);
            unchecked
            {
                hash = hash * 31 + phase.StartTime.GetHashCode();
                hash = hash * 31 + phase.MinSpawnCount;
                hash = hash * 31 + phase.SpawnInterval.GetHashCode();
            }
            if (phase.Enemies == null || phase.Enemies.Count == 0 || phase.Enemies.Any(e => e == null || e.Prefab == null || e.SpawnWeight <= 0))
                sourceError = phase.name + "：敵Prefabと正の重みを設定してください。";
        }
        phases.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
        capacity = so.FindProperty("maxEnemyCount").intValue;
        if (autoDuration && drag == DragKind.None)
        {
            var timer = so.FindProperty("timer").objectReferenceValue as Timer;
            duration = Mathf.Max(60, timer != null ? timer.TimeLimit : 0,
                phases.Count > 0 ? phases[phases.Count - 1].StartTime + 60 : 0);
        }
        unchecked { hash = ((hash * 31 + capacity) * 31 + duration.GetHashCode()) * 31 + killsPerSecond.GetHashCode(); }
        if (hash != signature) { signature = hash; dirty = true; }
        if (sourceError != null) dirty = true;
        if (!phases.Contains(selected)) selected = phases.FirstOrDefault();
    }

    private void Recalculate()
    {
        if (sourceError != null) { result = null; return; }
        if (!dirty) return;
        dirty = false;
        calculationError = null;
        result = null;
        try
        {
            result = EnemySpawnPreview.Run(phases.Select(p => new EnemySpawnPreview.Phase
            { Start = p.StartTime, Minimum = p.MinSpawnCount, Interval = p.SpawnInterval }), duration, capacity, killsPerSecond);
            if (phases.Count == 0) calculationError = "GeneratorにPhaseを追加してください。";
            // ドラッグ中は縦軸を固定し、操作に合わせて縮尺が変わるのを防ぐ。
            if (drag == DragKind.None)
                yMaximum = NiceStep(Math.Max(10, result.Peak * 1.15) / 4) * 4;
        }
        catch (ArgumentException exception) { calculationError = exception.Message; }
    }

    private static double NiceStep(double rough)
    {
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rough)));
        double n = rough / magnitude;
        return (n <= 1 ? 1 : n <= 2 ? 2 : n <= 5 ? 5 : 10) * magnitude;
    }
    private static string Clock(double seconds) => $"{(int)(seconds / 60)}:{seconds % 60:00.#}";
    private static Color PhaseColor(PhaseData phase, int index) => phase.IsExPhase
        ? new Color(1f, 0.57f, 0.25f) : index % 2 == 0 ? new Color(0.3f, 0.65f, 0.95f) : new Color(0.45f, 0.75f, 0.55f);
    private double PhaseStart(int index) => index == 0 ? 0 : phases[index].StartTime;
    private double PhaseEnd(int index) => index + 1 < phases.Count ? phases[index + 1].StartTime : duration;

    private void DrawGraph()
    {
        float width = Mathf.Max(820, position.width - 55) * zoom;
        graphScroll = GUILayout.BeginScrollView(graphScroll, true, false, GUILayout.Height(340));
        Rect canvas = GUILayoutUtility.GetRect(width, 312, GUILayout.Width(width), GUILayout.Height(312));
        var plot = new Rect(canvas.x + 65, canvas.y + 78, canvas.width - 90, 200);
        EditorGUI.DrawRect(canvas, new Color(0.12f, 0.13f, 0.15f));
        if (!(duration > 0) || float.IsInfinity(duration) || sourceError != null || calculationError != null)
        {
            GUI.Label(new Rect(canvas.x + 25, canvas.y + 35, canvas.width - 50, 30), "設定を修正すると出現量グラフを表示します。", EditorStyles.whiteLabel);
            GUILayout.EndScrollView();
            return;
        }
        Func<double, float> x = t => plot.x + (float)(t / duration) * plot.width;
        Func<double, float> y = n => plot.yMax - (float)(Math.Min(yMaximum, n) / yMaximum) * plot.height;
        GUI.Label(new Rect(canvas.x + 5, canvas.y + 2, 500, 20), "Phase担当区間 / 推定生存敵数（体）　　緑●：最低出現数　黄●：出現頻度", EditorStyles.whiteMiniLabel);

        var labelStyle = new GUIStyle(EditorStyles.whiteMiniLabel) { clipping = TextClipping.Clip };
        for (int i = 0; i < phases.Count; i++)
        {
            float left = x(PhaseStart(i)), right = x(Math.Min(duration, PhaseEnd(i)));
            if (left >= plot.xMax || right <= left) continue;
            Color color = PhaseColor(phases[i], i);
            EditorGUI.DrawRect(new Rect(left, plot.y, right - left, plot.height), new Color(color.r, color.g, color.b, selected == phases[i] ? 0.16f : 0.055f));
            var band = new Rect(left, canvas.y + 24 + i % 2 * 25, right - left, 23);
            EditorGUI.DrawRect(band, color * (selected == phases[i] ? 1 : 0.65f));
            string tooltip = $"{phases[i].name}\n{Clock(PhaseStart(i))} ～ {(i + 1 < phases.Count ? Clock(PhaseEnd(i)) : "以降（継続）")}\n最低 {phases[i].MinSpawnCount}体 / {1f / phases[i].SpawnInterval:0.##}回/秒";
            GUI.Label(band, new GUIContent(" " + phases[i].name, tooltip), labelStyle);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && band.Contains(Event.current.mousePosition))
            { selected = phases[i]; GUI.FocusControl(null); Repaint(); }
            EditorGUI.DrawRect(new Rect(left, plot.y, 1, plot.height), new Color(color.r, color.g, color.b, 0.3f));
        }

        Handles.BeginGUI();
        for (int i = 0; i <= 4; i++)
        {
            float yy = y(yMaximum * i / 4);
            Handles.color = new Color(1, 1, 1, 0.13f);
            Handles.DrawLine(new Vector3(plot.x, yy), new Vector3(plot.xMax, yy));
            GUI.Label(new Rect(canvas.x + 3, yy - 9, 60, 18), $"{yMaximum * i / 4:0}", labelStyle);
        }
        double step = NiceStep(duration / Math.Max(4, plot.width / 100));
        for (double time = 0; time <= duration; time += step)
        {
            float xx = x(time);
            Handles.color = new Color(1, 1, 1, 0.1f);
            Handles.DrawLine(new Vector3(xx, plot.y), new Vector3(xx, plot.yMax));
            GUI.Label(new Rect(xx - 14, plot.yMax + 5, 65, 20), Clock(time), labelStyle);
        }
        if (result != null && Event.current.type == EventType.Repaint)
        {
            Handles.color = CurveColor;
            // 画素ごとの値を1本の線として描画し、大量の生成イベントでも表示が欠けないようにする。
            int pixels = Mathf.CeilToInt(plot.width);
            var vertices = new Vector3[pixels + 1];
            for (int i = 0; i <= pixels; i++)
            {
                double time = duration * (double)i / pixels;
                vertices[i] = new Vector3(x(time), y(result.At(time).Alive));
            }
            Handles.DrawAAPolyLine(2f, vertices);
        }
        minimumHandle = rateHandle = Rect.zero;
        int selectedIndex = phases.IndexOf(selected);
        if (selectedIndex >= 0 && PhaseStart(selectedIndex) < duration)
        {
            float left = x(PhaseStart(selectedIndex)), right = x(Math.Min(duration, PhaseEnd(selectedIndex)));
            float yy = y(selected.MinSpawnCount);
            Handles.color = MinimumColor;
            Handles.DrawDottedLine(new Vector3(left, yy), new Vector3(right, yy), 4);
            minimumHandle = new Rect(left + (right - left) * 0.18f - 6, yy - 6, 12, 12);
            EditorGUI.DrawRect(minimumHandle, MinimumColor);
            GUI.Label(new Rect(left + 3, Mathf.Clamp(yy - 20, plot.y, plot.yMax - 20), 160, 20), $"最低 {selected.MinSpawnCount}体", labelStyle);
            if (result != null)
            {
                double time = PhaseStart(selectedIndex) + (Math.Min(duration, PhaseEnd(selectedIndex)) - PhaseStart(selectedIndex)) * 0.7;
                rateHandle = new Rect(x(time) - 6, y(result.At(time).Alive) - 6, 12, 12);
                EditorGUI.DrawRect(rateHandle, CurveColor);
            }
        }
        Handles.EndGUI();
        HandleDrag(canvas, plot, x);
        if (drag == DragKind.None && plot.Contains(Event.current.mousePosition) && result != null && result.Points.Count > 0)
        {
            double time = (Event.current.mousePosition.x - plot.x) / plot.width * duration;
            EnemySpawnPreview.Point point = result.At(time);
            string text = $"{Clock(time)} / {point.Alive:0.#}体 / {phases[point.PhaseIndex].name}";
            GUI.Label(new Rect(Mathf.Clamp(Event.current.mousePosition.x + 10, plot.x, plot.xMax - 290), plot.y + 4, 290, 22), text, EditorStyles.helpBox);
        }
        GUILayout.EndScrollView();
    }

    private void HandleDrag(Rect canvas, Rect plot, Func<double, float> x)
    {
        Event e = Event.current;
        int id = GUIUtility.GetControlID(FocusType.Passive);
        EditorGUIUtility.AddCursorRect(minimumHandle, MouseCursor.ResizeVertical);
        EditorGUIUtility.AddCursorRect(rateHandle, MouseCursor.ResizeVertical);
        for (int i = 1; i < phases.Count; i++)
            if (phases[i].StartTime < duration)
            {
                var hit = new Rect(x(phases[i].StartTime) - 4, canvas.y + 24, 8, plot.yMax - canvas.y - 24);
                EditorGUIUtility.AddCursorRect(hit, MouseCursor.ResizeHorizontal);
                if (e.type == EventType.MouseDown && e.button == 0 && hit.Contains(e.mousePosition) && drag == DragKind.None && !EditorApplication.isPlaying)
                    BeginDrag(DragKind.Boundary, phases[i], id);
            }
        if (e.type == EventType.MouseDown && e.button == 0 && drag == DragKind.None && selected != null && !EditorApplication.isPlaying)
        {
            if (minimumHandle.Contains(e.mousePosition)) BeginDrag(DragKind.Minimum, selected, id);
            else if (rateHandle.Contains(e.mousePosition)) BeginDrag(DragKind.Rate, selected, id);
        }
        if (drag == DragKind.None) return;
        if (e.type == EventType.MouseDrag)
        {
            var so = new SerializedObject(dragPhase);
            if (drag == DragKind.Boundary)
            {
                int index = phases.IndexOf(dragPhase);
                if (index <= 0) { EndDrag(); return; }
                float lo = phases[index - 1].StartTime + 0.01f;
                float hi = index + 1 < phases.Count ? phases[index + 1].StartTime - 0.01f : duration - 0.01f;
                if (lo <= hi) so.FindProperty("startTime").floatValue = Mathf.Clamp(Mathf.Round((e.mousePosition.x - plot.x) / plot.width * duration * 10) / 10, lo, hi);
            }
            else if (drag == DragKind.Minimum)
                so.FindProperty("minSpawnCount").intValue = Mathf.Max(0, Mathf.RoundToInt((plot.yMax - e.mousePosition.y) / plot.height * (float)yMaximum));
            else
            {
                float rate = Mathf.Clamp(dragRate * Mathf.Exp((dragY - e.mousePosition.y) / 80), 0.01f, 1000);
                so.FindProperty("spawnInterval").floatValue = 1 / rate;
            }
            if (so.ApplyModifiedProperties()) dirty = true;
            e.Use(); Repaint();
        }
        if (e.type == EventType.MouseUp || e.type == EventType.Ignore) { EndDrag(); e.Use(); }
    }

    private void BeginDrag(DragKind kind, PhaseData phase, int id)
    {
        drag = kind; dragPhase = selected = phase;
        dragY = Event.current.mousePosition.y;
        dragRate = phase.SpawnInterval > 0 ? 1 / phase.SpawnInterval : 1;
        dragControl = GUIUtility.hotControl = id;
        Undo.IncrementCurrentGroup();
        undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("敵出現グラフを調整");
        Event.current.Use();
    }

    private void EndDrag()
    {
        if (drag == DragKind.None) return;
        Undo.CollapseUndoOperations(undoGroup);
        drag = DragKind.None; dragPhase = null;
        if (GUIUtility.hotControl == dragControl) GUIUtility.hotControl = 0;
        dirty = true;
    }

    private void DrawPhaseList()
    {
        using (new EditorGUILayout.VerticalScope(GUILayout.Width(Mathf.Max(310, position.width * 0.42f))))
        {
            EditorGUILayout.LabelField("Phase一覧（クリックで選択）", EditorStyles.boldLabel);
            listScroll = EditorGUILayout.BeginScrollView(listScroll, GUILayout.Height(175));
            for (int i = 0; i < phases.Count; i++)
            {
                string end = i + 1 < phases.Count ? Clock(PhaseEnd(i)) : "以降";
                if (GUILayout.Toggle(selected == phases[i], $"{Clock(PhaseStart(i))} ～ {end}　{phases[i].name}", "Button")) selected = phases[i];
            }
            EditorGUILayout.EndScrollView();
        }
    }

    private void DrawSelected()
    {
        if (selected == null) return;
        EditorGUILayout.LabelField(selected.name, EditorStyles.boldLabel);
        var so = new SerializedObject(selected);
        using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
        {
            EditorGUILayout.PropertyField(so.FindProperty("startTime"), new GUIContent("開始時刻（秒）"));
            EditorGUILayout.PropertyField(so.FindProperty("minSpawnCount"), new GUIContent("最低出現数（維持数）"));
            SerializedProperty interval = so.FindProperty("spawnInterval");
            EditorGUI.BeginChangeCheck();
            float rate = EditorGUILayout.FloatField(new GUIContent("出現頻度（回/秒）", "グラフの黄ハンドルを上下にドラッグしても調整できます。"), interval.floatValue > 0 ? 1 / interval.floatValue : 0);
            if (EditorGUI.EndChangeCheck() && rate > 0 && !float.IsInfinity(rate)) interval.floatValue = 1 / rate;
            EditorGUILayout.PropertyField(interval, new GUIContent("出現間隔（秒）"));
            EditorGUILayout.PropertyField(so.FindProperty("isExPhase"), new GUIContent("EXPhase"));
            EditorGUILayout.PropertyField(so.FindProperty("enemies"), new GUIContent("敵構成"), true);
            if (so.ApplyModifiedProperties()) dirty = true;
        }
        if (phases.Count > 0 && phases[0].StartTime > 0)
            EditorGUILayout.HelpBox("実Generatorに合わせ、先頭Phaseは0秒から計算します。", MessageType.Info);
        if (GUILayout.Button("Inspectorで開く")) Selection.activeObject = selected;
    }

    private void AddPhase()
    {
        string path = EditorUtility.SaveFilePanelInProject("Phaseを追加", "Phase_New", "asset", "選択Phaseを複製し、Generatorの最後に追加します。");
        if (string.IsNullOrEmpty(path)) return;
        if (AssetDatabase.LoadMainAssetAtPath(path) != null)
        { EditorUtility.DisplayDialog("Phaseを追加", "新しいファイル名を指定してください。", "OK"); return; }
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        var phase = selected != null ? Instantiate(selected) : CreateInstance<PhaseData>();
        phase.name = System.IO.Path.GetFileNameWithoutExtension(path);
        var settings = new SerializedObject(phase);
        settings.FindProperty("startTime").floatValue = phases.Count == 0 ? 0 : phases[phases.Count - 1].StartTime + 30;
        settings.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.CreateAsset(phase, path);
        // アセットは除外・Undo後も残し、参照だけをUndoする。
        var so = new SerializedObject(generator);
        SerializedProperty list = so.FindProperty("phases");
        int index = list.arraySize;
        list.arraySize++;
        list.GetArrayElementAtIndex(index).objectReferenceValue = phase;
        so.ApplyModifiedProperties();
        Undo.SetCurrentGroupName("敵Phaseを追加");
        Undo.CollapseUndoOperations(group);
        selected = phase;
        dirty = true;
    }

    private void RemoveSelected()
    {
        var so = new SerializedObject(generator);
        SerializedProperty list = so.FindProperty("phases");
        for (int i = list.arraySize - 1; i >= 0; i--)
            if (list.GetArrayElementAtIndex(i).objectReferenceValue == selected)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = null;
                list.DeleteArrayElementAtIndex(i);
            }
        so.ApplyModifiedProperties();
        selected = null;
        dirty = true;
    }
}
