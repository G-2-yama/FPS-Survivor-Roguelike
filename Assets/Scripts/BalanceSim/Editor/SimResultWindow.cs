using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
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

        private static GUIStyle _rightAlignedStyle;
        private static GUIStyle _centeredStyle;

        [SerializeField] private SimResult _result;
        private Vector2 _scroll;
        private int _hoverIndex = -1;
        private bool _hoverFound;
        private readonly Dictionary<string, float> _legendWidths = new();
        private SimResult _legendWidthsSource;

        [MenuItem("Tools/BalanceSim/結果")]
        public static void Open()
        {
            var window = GetWindow<SimResultWindow>(WindowTitle);
            if (window._result == null)
            {
                window.LoadLast();
            }
        }

        public static void ShowResult(SimResult result)
        {
            var window = GetWindow<SimResultWindow>(WindowTitle);
            window._result = result;
            window._hoverIndex = -1;
            window.Repaint();
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
            if (_result == null)
            {
                LoadLast();
            }
        }

        private void LoadLast()
        {
            if (File.Exists(SimRunner.OutputPath))
            {
                _result = JsonUtility.FromJson<SimResult>(File.ReadAllText(SimRunner.OutputPath));
            }
        }

        private void OnGUI()
        {
            if (_result == null || _result.series.Count == 0)
            {
                EditorGUILayout.HelpBox("結果がありません。BalanceSimSettings の Inspector で「実行」を押してください。", MessageType.Info);
                return;
            }

            Event e = Event.current;
            if (e.type == EventType.MouseLeaveWindow)
            {
                _hoverIndex = -1;
                Repaint();
            }

            _hoverFound = false;
            EditorGUILayout.LabelField(_result.message, EditorStyles.wordWrappedLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (IGrouping<string, SimSeries> group in _result.series.GroupBy(s => string.IsNullOrEmpty(s.group) ? s.name : s.group))
            {
                DrawPanel(group.Key, group.ToList());
                GUILayout.Space(PanelSpacing);
            }
            EditorGUILayout.EndScrollView();

            if (e.type == EventType.MouseMove)
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

            string titleText = hover >= 0 ? $"{title}    {hover * _result.sampleInterval:0.#}秒" : title;
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

            float duration = (sampleCount - 1) * _result.sampleInterval;
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
            if (_legendWidthsSource != _result)
            {
                _legendWidths.Clear();
                _legendWidthsSource = _result;
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

        private static Color ColorOf(int index) => Palette[(index % Palette.Length + Palette.Length) % Palette.Length];

        private static GUIStyle RightAlignedStyle => _rightAlignedStyle ??= new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight };

        private static GUIStyle CenteredStyle => _centeredStyle ??= new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.UpperCenter };

        private static Color BackgroundColor => EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.92f, 0.92f, 0.92f);

        private static Color GridColor => EditorGUIUtility.isProSkin ? new Color(0.28f, 0.28f, 0.28f) : new Color(0.78f, 0.78f, 0.78f);

        private static Color HoverColor => EditorGUIUtility.isProSkin ? new Color(0.9f, 0.9f, 0.9f, 0.6f) : new Color(0.1f, 0.1f, 0.1f, 0.6f);
    }
}
