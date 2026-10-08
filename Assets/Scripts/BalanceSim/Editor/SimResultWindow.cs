using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BalanceSim.Editor
{
    public class SimResultWindow : EditorWindow
    {
        private const string WindowTitle = "BalanceSim 結果";
        private const float PlotHeight = 160f;
        private const float AxisLeft = 52f;
        private const float AxisRight = 16f;
        private const float AxisLabelHeight = 16f;
        private const float PlotTopMargin = AxisLabelHeight * 0.5f;
        private const float AxisBottom = AxisLabelHeight * 1.5f;
        private const float LegendLineHeight = 18f;
        private const float SwatchSize = 10f;
        private const float LegendGap = 14f;
        private const float PanelSpacing = 12f;
        private const int YTickCount = 4;
        private static readonly float[] TimeSteps = { 10f, 15f, 30f, 60f, 120f, 300f, 600f, 1200f };

        private static readonly Color[] Palette =
        {
            new(0.30f, 0.60f, 0.95f),
            new(0.95f, 0.55f, 0.20f),
            new(0.35f, 0.75f, 0.35f),
            new(0.90f, 0.35f, 0.35f),
            new(0.65f, 0.45f, 0.85f),
            new(0.60f, 0.45f, 0.35f),
            new(0.90f, 0.50f, 0.75f),
            new(0.55f, 0.55f, 0.55f),
            new(0.75f, 0.75f, 0.25f),
            new(0.25f, 0.75f, 0.80f),
        };

        private const string ResultsFolder = "BalanceSimResults";
        private const float NavButtonWidth = 28f;
        private const float PageLabelWidth = 72f;
        private const float ViewGap = 8f;
        private const float PopupMaxWidth = 280f;
        private const float ScrollbarAllowance = 24f;
        private static readonly string[] SummaryViewLabels = { "表", "値ごと", "組み合わせ" };

        private enum SummaryView
        {
            Table,
            Effects,
            Heatmap,
        }

        private static GUIStyle _rightAlignedStyle;
        private static GUIStyle _centeredStyle;
        private static GUIStyle _pageLabelStyle;

        [SerializeField] private string _sourcePath;
        [SerializeField] private int _page = -1;
        [SerializeField] private TreeViewState<int> _tableState;
        [SerializeField] private MultiColumnHeaderState _tableHeaderState;
        [SerializeField] private SummaryView _summaryView;
        [SerializeField] private string _metricName;
        [SerializeField] private string _heatmapXName;
        [SerializeField] private string _heatmapYName;
        [NonSerialized] private SimResultSet _set;
        private Vector2 _scroll;
        private Vector2 _chartScroll;
        private SimResultSet _columnsSource;
        private SimSummaryColumns _columns;
        private SimEffectChart _effectChart;
        private SimHeatmap _heatmap;
        private int _hoverIndex = -1;
        private bool _hoverFound;
        private readonly Dictionary<string, float> _legendWidths = new();
        private SimResult _legendWidthsSource;
        private SimResultSet _tableSource;
        private SimResultTable _table;
        private int _openRequest = -1;

        [MenuItem("Tools/BalanceSim/結果")]
        public static void Open()
        {
            var window = GetWindow<SimResultWindow>(WindowTitle);
            if (window._set == null)
            {
                window.LoadFrom(window.SourcePathOrLast, false, null);
            }
        }

        public static void ShowResult(SimResultSet set, string sourcePath)
        {
            var window = GetWindow<SimResultWindow>(WindowTitle);
            window.SetResult(set, sourcePath, null);
        }

        private bool HasSummary => _set != null && _set.cases.Count > 1;

        private SimResult CurrentResult => _set != null && _page >= 0 && _page < _set.cases.Count ? _set.cases[_page].result : null;

        private SimResultTable Table
        {
            get
            {
                if (_tableSource != _set)
                {
                    _tableSource = _set;
                    _tableState ??= new TreeViewState<int>();
                    _table = SimResultTable.Create(Columns, _tableState, ref _tableHeaderState, index => _openRequest = index);
                }
                return _table;
            }
        }

        private SimSummaryColumns Columns
        {
            get
            {
                if (_columnsSource != _set)
                {
                    _columnsSource = _set;
                    _columns = SimSummaryColumns.Of(_set);
                    _effectChart = new SimEffectChart(_columns, index => _openRequest = index);
                    _heatmap = new SimHeatmap(_columns, index => _openRequest = index);
                }
                return _columns;
            }
        }

        private bool ChartShown => _page < 0 && _summaryView != SummaryView.Table;

        private string SourcePathOrLast => string.IsNullOrEmpty(_sourcePath) ? SimRunner.LastResultPath : _sourcePath;

        private static string ResultsFolderPath => Path.Combine(SimRunner.ProjectRoot, ResultsFolder);

        private void OnEnable()
        {
            wantsMouseMove = true;
            if (_set == null)
            {
                LoadFrom(SourcePathOrLast, false, _page);
            }
        }

        private void SetResult(SimResultSet set, string sourcePath, int? page)
        {
            _set = set;
            _sourcePath = sourcePath;
            int first = set.cases.Count > 1 ? -1 : 0;
            _page = page.HasValue ? Mathf.Clamp(page.Value, first, set.cases.Count - 1) : first;
            _tableState = null;
            _hoverIndex = -1;
            _scroll = Vector2.zero;
            Repaint();
        }

        private bool LoadFrom(string path, bool reportError, int? page)
        {
            SimResultSet set = null;
            if (File.Exists(path))
            {
                try
                {
                    set = JsonUtility.FromJson<SimResultSet>(File.ReadAllText(path));
                }
                catch (ArgumentException)
                {
                    set = null;
                }
            }

            if (set?.cases == null || set.cases.Count == 0)
            {
                if (reportError)
                {
                    EditorUtility.DisplayDialog(WindowTitle, $"BalanceSim の結果として読めませんでした:\n{path}", "OK");
                }
                return false;
            }

            SetResult(set, path, page);
            return true;
        }

        private void Save()
        {
            Directory.CreateDirectory(ResultsFolderPath);
            string path = EditorUtility.SaveFilePanel("結果を保存", ResultsFolderPath, DefaultFileName(), "json");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            File.WriteAllText(path, JsonUtility.ToJson(_set));
            _sourcePath = path;
            Debug.Log($"[BalanceSim] 結果を保存しました: {path}");
        }

        private string DefaultFileName()
        {
            string name = string.IsNullOrEmpty(_set.valuesName) ? _set.settingsName : _set.valuesName;
            DateTime created = DateTime.TryParseExact(_set.createdAt, SimResultSet.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed)
                ? parsed
                : DateTime.Now;
            return $"{name}_{created:yyyyMMdd_HHmm}";
        }

        private void OpenFile()
        {
            string folder = Directory.Exists(ResultsFolderPath) ? ResultsFolderPath : SimRunner.ProjectRoot;
            string path = EditorUtility.OpenFilePanel("結果を開く", folder, "json");
            if (!string.IsNullOrEmpty(path))
            {
                LoadFrom(path, true, null);
            }
        }

        private void ChangePage(int page)
        {
            _page = page;
            _hoverIndex = -1;
            _scroll = Vector2.zero;
            GUI.FocusControl(null);
            GUIUtility.ExitGUI();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (HasSummary)
                {
                    SimResultTable table = Table;
                    int position = _page < 0 ? -1 : table.PositionOf(_page);
                    using (new EditorGUI.DisabledScope(_page < 0))
                    {
                        if (GUILayout.Button("まとめ", EditorStyles.toolbarButton))
                        {
                            ChangePage(-1);
                        }
                        if (GUILayout.Button("◀", EditorStyles.toolbarButton, GUILayout.Width(NavButtonWidth)))
                        {
                            ChangePage(position <= 0 ? -1 : table.CaseAt(position - 1));
                        }
                    }

                    GUILayout.Label(_page < 0 ? "まとめ" : $"{position + 1} / {table.Count}", PageLabelStyle, GUILayout.Width(PageLabelWidth));

                    using (new EditorGUI.DisabledScope(position >= table.Count - 1))
                    {
                        if (GUILayout.Button("▶", EditorStyles.toolbarButton, GUILayout.Width(NavButtonWidth)))
                        {
                            ChangePage(table.CaseAt(position + 1));
                        }
                    }

                    if (_page < 0)
                    {
                        GUILayout.Space(ViewGap);
                        var view = (SummaryView)GUILayout.Toolbar((int)_summaryView, SummaryViewLabels, EditorStyles.toolbarButton, GUI.ToolbarButtonSize.FitToContents);
                        if (view != _summaryView)
                        {
                            _summaryView = view;
                            ClearChartHover();
                            GUI.FocusControl(null);
                            GUIUtility.ExitGUI();
                        }
                    }
                }

                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(_set == null))
                {
                    if (GUILayout.Button("保存", EditorStyles.toolbarButton))
                    {
                        Save();
                        GUIUtility.ExitGUI();
                    }
                }
                if (GUILayout.Button("開く", EditorStyles.toolbarButton))
                {
                    OpenFile();
                    GUIUtility.ExitGUI();
                }
            }
        }

        private void DrawConditions()
        {
            string source = string.Equals(Path.GetFullPath(_sourcePath), Path.GetFullPath(SimRunner.LastResultPath), StringComparison.OrdinalIgnoreCase)
                ? "最新の実行"
                : Path.GetFileNameWithoutExtension(_sourcePath);
            var parts = new List<string>
            {
                source,
                _set.createdAt,
                $"設定: {_set.settingsName}",
                $"シーン: {_set.sceneName}",
                HasSummary ? $"1通りあたり {_set.runCount} 回" : $"{_set.runCount} 回",
            };
            if (!string.IsNullOrEmpty(_set.valuesName))
            {
                parts.Add($"{(HasSummary ? "探索SO" : "数値SO")}: {_set.valuesName}");
            }
            EditorGUILayout.LabelField(string.Join("    ", parts), EditorStyles.wordWrappedMiniLabel);

            if (_set.fixedValues.Count > 0)
            {
                EditorGUILayout.LabelField($"{(HasSummary ? "固定した値" : "上書きした値")}: {string.Join(", ", _set.fixedValues)}", EditorStyles.wordWrappedMiniLabel);
            }
        }

        private void DrawSummary()
        {
            if (_summaryView == SummaryView.Table)
            {
                Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                Table.OnGUI(rect);
            }
            else
            {
                DrawChart();
            }

            if (_openRequest >= 0)
            {
                int page = _openRequest;
                _openRequest = -1;
                ChangePage(page);
            }
        }

        private void DrawChart()
        {
            SimSummaryColumns columns = Columns;
            bool heatmap = _summaryView == SummaryView.Heatmap;
            int metric;
            int x = -1;
            int y = -1;
            using (new EditorGUILayout.HorizontalScope())
            {
                metric = ColumnPopup("指標", ref _metricName, columns.FirstMetric, columns.Count, columns.FirstMetric);
                if (heatmap && columns.ValueCount >= 2)
                {
                    x = ColumnPopup("横軸", ref _heatmapXName, columns.FirstValue, columns.FirstMetric, columns.FirstValue);
                    y = ColumnPopup("縦軸", ref _heatmapYName, columns.FirstValue, columns.FirstMetric, columns.FirstValue + 1);
                }
                GUILayout.FlexibleSpace();
            }

            _chartScroll = EditorGUILayout.BeginScrollView(_chartScroll);
            float width = position.width - ScrollbarAllowance;
            bool hoverChanged = heatmap ? _heatmap.OnGUI(metric, x, y, width) : _effectChart.OnGUI(metric, width);
            EditorGUILayout.EndScrollView();

            if (hoverChanged)
            {
                Repaint();
            }
        }

        private int ColumnPopup(string label, ref string selectedName, int from, int to, int fallback)
        {
            string[] headers = _columns.Headers;
            int current = Array.IndexOf(headers, selectedName, from, to - from);
            if (current < 0)
            {
                current = fallback;
            }

            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            string[] options = headers.Skip(from).Take(to - from).ToArray();
            int selected = EditorGUILayout.Popup(current - from, options, GUILayout.MaxWidth(PopupMaxWidth)) + from;
            if (selected != current)
            {
                selectedName = headers[selected];
                ClearChartHover();
                GUIUtility.ExitGUI();
            }
            selectedName = headers[current];
            return current;
        }

        private void ClearChartHover()
        {
            _effectChart?.ClearHover();
            _heatmap?.ClearHover();
        }

        private void OnGUI()
        {
            DrawToolbar();
            if (_set == null)
            {
                EditorGUILayout.HelpBox("結果がありません。BalanceSimSettings の Inspector で「実行」を押すか、上の「開く」で保存した結果を開いてください。", MessageType.Info);
                return;
            }

            Event e = Event.current;
            if (e.type == EventType.MouseLeaveWindow)
            {
                _hoverIndex = -1;
                ClearChartHover();
                Repaint();
            }

            _hoverFound = false;
            DrawConditions();
            if (_page < 0)
            {
                DrawSummary();
            }
            else
            {
                SimResultCase current = _set.cases[_page];
                if (HasSummary)
                {
                    EditorGUILayout.LabelField($"振った値: {current.Label}", EditorStyles.wordWrappedLabel);
                }
                EditorGUILayout.LabelField(current.result.message, EditorStyles.wordWrappedLabel);
                if (current.result.runs.Count == 0 && current.result.runCount > 0)
                {
                    EditorGUILayout.LabelField($"各回の記録を残さずに実行した結果です。グラフは平均だけで、{current.result.sampleInterval:0.##}秒ごとの点を結んでいます", EditorStyles.wordWrappedMiniLabel);
                }
                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                foreach (IGrouping<string, SimSeries> group in current.result.series.GroupBy(s => string.IsNullOrEmpty(s.group) ? s.name : s.group))
                {
                    DrawPanel(group.Key, group.ToList());
                    GUILayout.Space(PanelSpacing);
                }
                EditorGUILayout.EndScrollView();
            }

            if (e.type == EventType.MouseMove && !ChartShown)
            {
                if (!_hoverFound)
                {
                    _hoverIndex = -1;
                }
                Repaint();
            }
        }

        private void DrawPanel(string title, List<SimSeries> all)
        {
            List<SimSeries> series = all.Count > 1 ? all.Where(s => s.mean.Any(v => v != 0f)).ToList() : all;
            int sampleCount = all[0].mean.Count;
            int hover = _hoverIndex >= 0 && _hoverIndex < sampleCount ? _hoverIndex : -1;

            string titleText = hover >= 0 ? $"{title}    {hover * CurrentResult.sampleInterval:0.#}秒" : title;
            EditorGUILayout.LabelField(titleText, EditorStyles.boldLabel);

            if (series.Count == 0 || sampleCount == 0)
            {
                EditorGUILayout.LabelField("すべて 0");
                return;
            }

            DrawLegend(title, series, all, hover);

            Rect rect = GUILayoutUtility.GetRect(0f, PlotTopMargin + PlotHeight + AxisBottom, GUILayout.ExpandWidth(true));
            var plot = new Rect(rect.x + AxisLeft, rect.y + PlotTopMargin, rect.width - AxisLeft - AxisRight, PlotHeight);
            if (plot.width <= 0f || plot.height <= 0f)
            {
                return;
            }

            float duration = (sampleCount - 1) * CurrentResult.sampleInterval;
            UpdateHover(plot, sampleCount);

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            bool ratio = series.All(s => s.ratio);
            float yMax = ratio ? 1f : NiceMax(series.Max(s => Mathf.Max(s.mean.Max(), s.p90.DefaultIfEmpty(0f).Max())));

            DrawAxes(plot, duration, yMax, ratio);

            for (int i = 0; i < series.Count; i++)
            {
                Color color = ColorOf(all.IndexOf(series[i]));
                if (!series[i].ratio && series[i].p10.Count == sampleCount && series[i].p90.Count == sampleCount)
                {
                    DrawBand(plot, series[i], sampleCount, yMax, new Color(color.r, color.g, color.b, 0.2f));
                }
                DrawLine(plot, series[i].mean, sampleCount, yMax, color);
            }

            if (hover >= 0)
            {
                float x = plot.x + plot.width * (sampleCount > 1 ? (float)hover / (sampleCount - 1) : 0f);
                EditorGUI.DrawRect(new Rect(x, plot.y, 1f, plot.height), HoverColor);
            }
        }

        private void DrawLegend(string title, List<SimSeries> series, List<SimSeries> all, int hover)
        {
            GUIStyle style = EditorStyles.label;
            float width = position.width - AxisRight - 16f;
            float columnWidth = LegendColumnWidth(title, series, style);
            int columns = Mathf.Max(1, Mathf.FloorToInt((width + LegendGap) / (columnWidth + LegendGap)));
            int rows = (series.Count + columns - 1) / columns;

            Rect rect = GUILayoutUtility.GetRect(0f, rows * LegendLineHeight, GUILayout.ExpandWidth(true));
            for (int i = 0; i < series.Count; i++)
            {
                float x = rect.x + (i % columns) * (columnWidth + LegendGap);
                float y = rect.y + (i / columns) * LegendLineHeight;
                EditorGUI.DrawRect(new Rect(x, y + (LegendLineHeight - SwatchSize) * 0.5f, SwatchSize, SwatchSize), ColorOf(all.IndexOf(series[i])));
                GUI.Label(new Rect(x + SwatchSize + 4f, y, columnWidth - SwatchSize - 4f, LegendLineHeight), LegendText(series[i], hover), style);
            }
        }

        private float LegendColumnWidth(string title, List<SimSeries> series, GUIStyle style)
        {
            if (_legendWidthsSource != CurrentResult)
            {
                _legendWidths.Clear();
                _legendWidthsSource = CurrentResult;
            }

            if (_legendWidths.TryGetValue(title, out float width))
            {
                return width;
            }

            width = 0f;
            foreach (SimSeries s in series)
            {
                for (int i = -1; i < s.mean.Count; i++)
                {
                    width = Mathf.Max(width, style.CalcSize(new GUIContent(LegendText(s, i))).x);
                }
            }

            width += SwatchSize + 4f;
            _legendWidths[title] = width;
            return width;
        }

        private static string LegendText(SimSeries s, int hover)
        {
            if (hover < 0 || hover >= s.mean.Count)
            {
                return s.name;
            }

            if (s.ratio)
            {
                return $"{s.name} {s.mean[hover] * 100f:0}%";
            }

            if (hover < s.p10.Count && hover < s.p90.Count)
            {
                return $"{s.name} 平均 {s.mean[hover]:0.#} (p10 {s.p10[hover]:0.#} 〜 p90 {s.p90[hover]:0.#})";
            }
            return $"{s.name} 平均 {s.mean[hover]:0.#}";
        }

        private void UpdateHover(Rect plot, int sampleCount)
        {
            Event e = Event.current;
            if (e.type != EventType.MouseMove || !plot.Contains(e.mousePosition))
            {
                return;
            }

            float t = Mathf.InverseLerp(plot.xMin, plot.xMax, e.mousePosition.x);
            _hoverIndex = Mathf.RoundToInt(t * (sampleCount - 1));
            _hoverFound = true;
        }

        private static void DrawAxes(Rect plot, float duration, float yMax, bool ratio)
        {
            EditorGUI.DrawRect(plot, BackgroundColor);

            for (int i = 0; i <= YTickCount; i++)
            {
                float value = yMax * i / YTickCount;
                float y = plot.yMax - plot.height * i / YTickCount;
                EditorGUI.DrawRect(new Rect(plot.x, y, plot.width, 1f), GridColor);
                string label = ratio ? $"{value * 100f:0}%" : FormatValue(value);
                GUI.Label(new Rect(plot.x - AxisLeft, y - AxisLabelHeight * 0.5f, AxisLeft - 4f, AxisLabelHeight), label, RightAlignedStyle);
            }

            if (duration <= 0f)
            {
                return;
            }

            float step = TimeSteps.FirstOrDefault(s => s >= duration / 8f);
            if (step <= 0f)
            {
                step = TimeSteps[TimeSteps.Length - 1];
            }

            for (float t = 0f; t <= duration + 1e-3f; t += step)
            {
                float x = plot.x + plot.width * t / duration;
                EditorGUI.DrawRect(new Rect(x, plot.y, 1f, plot.height), GridColor);
                GUI.Label(new Rect(x - 30f, plot.yMax + AxisLabelHeight * 0.5f, 60f, AxisLabelHeight), $"{t:0}秒", CenteredStyle);
            }
        }

        private static void DrawLine(Rect plot, List<float> values, int sampleCount, float yMax, Color color)
        {
            var points = new Vector3[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                points[i] = ToPoint(plot, i, sampleCount, values[i], yMax);
            }

            Handles.color = color;
            Handles.DrawAAPolyLine(2f, points);
        }

        private static void DrawBand(Rect plot, SimSeries s, int sampleCount, float yMax, Color color)
        {
            Handles.color = color;
            var quad = new Vector3[4];
            for (int i = 0; i < sampleCount - 1; i++)
            {
                quad[0] = ToPoint(plot, i, sampleCount, s.p10[i], yMax);
                quad[1] = ToPoint(plot, i + 1, sampleCount, s.p10[i + 1], yMax);
                quad[2] = ToPoint(plot, i + 1, sampleCount, s.p90[i + 1], yMax);
                quad[3] = ToPoint(plot, i, sampleCount, s.p90[i], yMax);
                Handles.DrawAAConvexPolygon(quad);
            }
        }

        private static Vector3 ToPoint(Rect plot, int index, int sampleCount, float value, float yMax)
        {
            float x = plot.x + plot.width * (sampleCount > 1 ? (float)index / (sampleCount - 1) : 0f);
            float y = plot.yMax - plot.height * Mathf.Clamp01(value / yMax);
            return new Vector3(x, y, 0f);
        }

        private static float NiceMax(float max)
        {
            if (max <= 0f)
            {
                return 1f;
            }

            float rough = max / YTickCount;
            float magnitude = Mathf.Pow(10f, Mathf.Floor(Mathf.Log10(rough)));
            float normalized = rough / magnitude;
            float nice = normalized <= 1f ? 1f : normalized <= 2f ? 2f : normalized <= 2.5f ? 2.5f : normalized <= 5f ? 5f : 10f;
            return nice * magnitude * YTickCount;
        }

        private static string FormatValue(float value)
        {
            return value >= 10f || Mathf.Approximately(value, Mathf.Round(value)) ? $"{value:0}" : $"{value:0.#}";
        }

        internal static Color ColorOf(int index) => Palette[(index % Palette.Length + Palette.Length) % Palette.Length];

        internal static GUIStyle RightAlignedStyle => _rightAlignedStyle ??= new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight };

        internal static GUIStyle CenteredStyle => _centeredStyle ??= new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.UpperCenter };

        private static GUIStyle PageLabelStyle => _pageLabelStyle ??= new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter };

        internal static Color BackgroundColor => EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.92f, 0.92f, 0.92f);

        internal static Color GridColor => EditorGUIUtility.isProSkin ? new Color(0.28f, 0.28f, 0.28f) : new Color(0.78f, 0.78f, 0.78f);

        internal static Color HoverColor => EditorGUIUtility.isProSkin ? new Color(0.9f, 0.9f, 0.9f, 0.6f) : new Color(0.1f, 0.1f, 0.1f, 0.6f);
    }
}
