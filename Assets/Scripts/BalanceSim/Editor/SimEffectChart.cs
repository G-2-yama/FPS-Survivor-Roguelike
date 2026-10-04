using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BalanceSim.Editor
{
    public class SimEffectChart
    {
        private const float PanelMinWidth = 300f;
        private const float PanelGap = 16f;
        private const float TitleHeight = 20f;
        private const float PlotHeight = 140f;
        private const float AxisLeft = 52f;
        private const float AxisRight = 12f;
        private const float AxisLabelHeight = 16f;
        private const float PlotTopMargin = AxisLabelHeight * 0.5f;
        private const float PanelHeight = TitleHeight + PlotTopMargin + PlotHeight + AxisLabelHeight * 1.5f;
        private const float DotSize = 5f;
        private const float MeanSize = 7f;
        private const float HoverDistance = 6f;
        private const float MaxJitterWidth = 14f;
        private const double EdgePadding = 0.06;
        private const int YTickTarget = 4;
        private const int MaxCandidateLabels = 8;
        private const float DashLength = 6f;
        private const float DashGap = 4f;

        private readonly SimSummaryColumns _columns;
        private readonly Action<int> _onOpen;
        private int _metric = -1;
        private List<Panel> _panels = new();
        private double? _baseline;
        private double _yLo;
        private double _yHi;
        private double _yStep;
        private bool _hasPoints;

        private int _hoverPanel = -1;
        private int _hoverPoint = -1;
        private int _nextHoverPanel;
        private int _nextHoverPoint;
        private Vector2 _hoverMouse;

        public SimEffectChart(SimSummaryColumns columns, Action<int> onOpen)
        {
            _columns = columns;
            _onOpen = onOpen;
        }

        public void ClearHover()
        {
            _hoverPanel = -1;
            _hoverPoint = -1;
        }

        public bool OnGUI(int metric, float width)
        {
            if (metric != _metric)
            {
                Build(metric);
            }

            if (_panels.Count == 0)
            {
                EditorGUILayout.HelpBox("振った値がありません。", MessageType.Info);
                return false;
            }
            if (!_hasPoints)
            {
                EditorGUILayout.HelpBox($"「{_columns.Headers[_metric]}」の値がある通りがありません（全部クリアした場合の死亡時刻など）。", MessageType.Info);
                return false;
            }

            Event e = Event.current;
            bool moving = e.type == EventType.MouseMove;
            if (moving)
            {
                _nextHoverPanel = -1;
                _nextHoverPoint = -1;
            }

            int perRow = Mathf.Max(1, Mathf.FloorToInt((width + PanelGap) / (PanelMinWidth + PanelGap)));
            float panelWidth = (width - PanelGap * (perRow - 1)) / perRow;
            for (int start = 0; start < _panels.Count; start += perRow)
            {
                Rect row = GUILayoutUtility.GetRect(0f, PanelHeight, GUILayout.ExpandWidth(true));
                for (int k = 0; k < perRow && start + k < _panels.Count; k++)
                {
                    var rect = new Rect(row.x + k * (panelWidth + PanelGap), row.y, panelWidth, PanelHeight);
                    DrawPanel(start + k, rect);
                }
                GUILayout.Space(PanelGap);
            }

            if (e.type == EventType.Repaint && _hoverPanel >= 0 && _hoverPanel < _panels.Count && _hoverPoint >= 0)
            {
                SimChartGUI.DrawTooltip(_hoverMouse, TooltipText(_panels[_hoverPanel], _hoverPoint), width);
            }

            if (!moving)
            {
                return false;
            }

            bool changed = _nextHoverPanel != _hoverPanel || _nextHoverPoint != _hoverPoint || _nextHoverPanel >= 0;
            _hoverPanel = _nextHoverPanel;
            _hoverPoint = _nextHoverPoint;
            _hoverMouse = e.mousePosition;
            return changed;
        }

        private void Build(int metric)
        {
            _metric = metric;
            double?[][] keys = _columns.Keys;

            List<double> baselineValues = Enumerable.Range(0, _columns.CaseCount)
                .Where(i => _columns.IsBaseline(i) && keys[i][metric].HasValue)
                .Select(i => keys[i][metric].Value)
                .ToList();
            _baseline = baselineValues.Count > 0 ? baselineValues.Average() : null;

            var panels = new List<Panel>();
            for (int col = _columns.FirstValue; col < _columns.FirstMetric; col++)
            {
                panels.Add(new Panel(col, Enumerable.Range(0, _columns.CaseCount)
                    .Where(i => keys[i][col].HasValue && keys[i][metric].HasValue)
                    .Select(i => new Point(i, keys[i][col].Value, keys[i][metric].Value))
                    .ToList()));
            }
            _panels = panels.OrderByDescending(p => p.Effect ?? -1.0).ToList();

            List<double> ys = _panels.SelectMany(p => p.Points.Select(pt => pt.Y)).ToList();
            _hasPoints = ys.Count > 0;
            if (_baseline.HasValue)
            {
                ys.Add(_baseline.Value);
            }
            if (ys.Count > 0)
            {
                (_yLo, _yHi, _yStep) = SimChartGUI.NiceRange(ys.Min(), ys.Max(), YTickTarget);
            }
            ClearHover();
        }

        private void DrawPanel(int index, Rect rect)
        {
            Panel panel = _panels[index];
            string header = _columns.Headers[panel.Column];
            string title = panel.Effect.HasValue ? $"{header}    平均の差 {_columns.Formats[_metric](panel.Effect.Value)}" : header;
            GUI.Label(new Rect(rect.x, rect.y, rect.width, TitleHeight), new GUIContent(title, header), SimChartGUI.ClippedBoldStyle);

            var plot = new Rect(rect.x + AxisLeft, rect.y + TitleHeight + PlotTopMargin, rect.width - AxisLeft - AxisRight, PlotHeight);
            if (plot.width <= 0f)
            {
                return;
            }

            (double xLo, double xHi) = XRange(panel);
            float jitterWidth = JitterWidth(panel, plot, xLo, xHi);
            Vector2 PointAt(int i) => new(
                XOf(plot, panel.Points[i].X, xLo, xHi) + panel.Points[i].Jitter * jitterWidth,
                YOf(plot, panel.Points[i].Y));

            Event e = Event.current;
            if ((e.type == EventType.MouseMove || (e.type == EventType.MouseDown && e.clickCount == 2)) && plot.Contains(e.mousePosition))
            {
                int nearest = Nearest(panel, e.mousePosition, PointAt);
                if (e.type == EventType.MouseMove)
                {
                    if (nearest >= 0)
                    {
                        _nextHoverPanel = index;
                        _nextHoverPoint = nearest;
                    }
                }
                else if (nearest >= 0)
                {
                    _onOpen(panel.Points[nearest].Case);
                    e.Use();
                }
            }

            if (e.type != EventType.Repaint)
            {
                return;
            }

            EditorGUI.DrawRect(plot, SimResultWindow.BackgroundColor);
            DrawYAxis(plot);
            DrawXAxis(plot, panel, xLo, xHi);

            if (panel.Points.Count == 0)
            {
                GUI.Label(plot, "この値を動かした通りがありません", SimResultWindow.CenteredStyle);
                return;
            }

            if (_baseline.HasValue)
            {
                DrawBaseline(plot, _baseline.Value);
            }

            Color color = SimResultWindow.ColorOf(0);
            float alpha = Mathf.Clamp(1.5f / Mathf.Sqrt(panel.MaxPerCandidate), 0.12f, 0.7f);
            var dotColor = new Color(color.r, color.g, color.b, alpha);
            for (int i = 0; i < panel.Points.Count; i++)
            {
                EditorGUI.DrawRect(Square(PointAt(i), DotSize), dotColor);
            }

            if (panel.MaxPerCandidate > 1 || panel.Means.Count > 1)
            {
                Vector3[] line = panel.Means.Select(m => new Vector3(XOf(plot, m.X, xLo, xHi), YOf(plot, m.Mean), 0f)).ToArray();
                if (line.Length > 1)
                {
                    Handles.color = color;
                    Handles.DrawAAPolyLine(2f, line);
                }
                if (panel.MaxPerCandidate > 1)
                {
                    foreach (Vector3 p in line)
                    {
                        EditorGUI.DrawRect(Square(p, MeanSize + 2f), SimResultWindow.BackgroundColor);
                        EditorGUI.DrawRect(Square(p, MeanSize), color);
                    }
                }
            }

            if (_hoverPanel == index && _hoverPoint >= 0 && _hoverPoint < panel.Points.Count)
            {
                Vector2 p = PointAt(_hoverPoint);
                EditorGUI.DrawRect(Square(p, DotSize + 4f), SimResultWindow.HoverColor);
                EditorGUI.DrawRect(Square(p, DotSize), color);
            }
        }

        private void DrawYAxis(Rect plot)
        {
            for (int k = 0; ; k++)
            {
                double value = _yLo + k * _yStep;
                if (value > _yHi + _yStep * 1e-6)
                {
                    break;
                }

                float y = YOf(plot, value);
                EditorGUI.DrawRect(new Rect(plot.x, y, plot.width, 1f), SimResultWindow.GridColor);
                string label = SimChartGUI.TickLabel(value, _yStep, _columns.Formats[_metric]);
                GUI.Label(new Rect(plot.x - AxisLeft, y - AxisLabelHeight * 0.5f, AxisLeft - 4f, AxisLabelHeight), label, SimResultWindow.RightAlignedStyle);
            }
        }

        private void DrawXAxis(Rect plot, Panel panel, double xLo, double xHi)
        {
            IEnumerable<(double value, string label)> ticks;
            if (panel.Means.Count > 0 && panel.Means.Count <= MaxCandidateLabels)
            {
                ticks = panel.Means.Select(m => (m.X, _columns.Format(panel.Column, m.X)));
            }
            else
            {
                double min = panel.Means.Count > 0 ? panel.Means[0].X : xLo;
                double max = panel.Means.Count > 0 ? panel.Means[panel.Means.Count - 1].X : xHi;
                (double lo, double hi, double step) = SimChartGUI.NiceRange(min, max, YTickTarget);
                ticks = Enumerable.Range(0, (int)Math.Round((hi - lo) / step) + 1)
                    .Select(k => lo + k * step)
                    .Where(v => v >= xLo && v <= xHi)
                    .Select(v => (v, SimChartGUI.TickLabel(v, step, _columns.Formats[panel.Column])));
            }

            float lastRight = float.NegativeInfinity;
            foreach ((double value, string label) in ticks)
            {
                float x = XOf(plot, value, xLo, xHi);
                EditorGUI.DrawRect(new Rect(x, plot.y, 1f, plot.height), SimResultWindow.GridColor);
                float labelWidth = SimResultWindow.CenteredStyle.CalcSize(new GUIContent(label)).x;
                if (x - labelWidth * 0.5f < lastRight + 4f)
                {
                    continue;
                }
                GUI.Label(new Rect(x - labelWidth * 0.5f, plot.yMax + AxisLabelHeight * 0.5f, labelWidth, AxisLabelHeight), label, SimResultWindow.CenteredStyle);
                lastRight = x + labelWidth * 0.5f;
            }
        }

        private void DrawBaseline(Rect plot, double value)
        {
            float y = YOf(plot, value);
            for (float x = plot.x; x < plot.xMax; x += DashLength + DashGap)
            {
                EditorGUI.DrawRect(new Rect(x, y, Mathf.Min(DashLength, plot.xMax - x), 1f), SimResultWindow.HoverColor);
            }
            string label = $"基準 {_columns.Formats[_metric](value)}";
            GUI.Label(new Rect(plot.x, y - AxisLabelHeight, plot.width - 4f, AxisLabelHeight), label, SimResultWindow.RightAlignedStyle);
        }

        private string TooltipText(Panel panel, int pointIndex)
        {
            Point point = panel.Points[pointIndex];
            double?[] row = _columns.Keys[point.Case];
            var text = new StringBuilder();
            text.AppendLine($"#{point.Case + 1}");
            for (int col = _columns.FirstValue; col < _columns.FirstMetric; col++)
            {
                if (row[col].HasValue)
                {
                    text.AppendLine($"{_columns.Headers[col]} = {_columns.Format(col, row[col])}");
                }
            }
            text.AppendLine($"{_columns.Headers[_metric]}: {_columns.Format(_metric, point.Y)}");

            Candidate mean = panel.Means.First(m => m.X == point.X);
            if (mean.Count > 1)
            {
                text.AppendLine($"この値の平均: {_columns.Format(_metric, mean.Mean)}（{mean.Count} 通り）");
            }
            text.Append("ダブルクリックで時系列を開く");
            return text.ToString();
        }

        private static int Nearest(Panel panel, Vector2 mouse, Func<int, Vector2> pointAt)
        {
            int nearest = -1;
            float best = HoverDistance * HoverDistance;
            for (int i = 0; i < panel.Points.Count; i++)
            {
                float d = (pointAt(i) - mouse).sqrMagnitude;
                if (d <= best)
                {
                    best = d;
                    nearest = i;
                }
            }
            return nearest;
        }

        private static (double lo, double hi) XRange(Panel panel)
        {
            if (panel.Means.Count == 0)
            {
                return (0.0, 1.0);
            }

            double min = panel.Means[0].X;
            double max = panel.Means[panel.Means.Count - 1].X;
            if (max - min < 1e-12)
            {
                double pad = Math.Abs(min) > 1e-12 ? Math.Abs(min) * 0.5 : 1.0;
                return (min - pad, max + pad);
            }

            double edge = (max - min) * EdgePadding;
            return (min - edge, max + edge);
        }

        private static float JitterWidth(Panel panel, Rect plot, double xLo, double xHi)
        {
            if (panel.MaxPerCandidate <= 1)
            {
                return 0f;
            }

            float width = MaxJitterWidth;
            for (int i = 1; i < panel.Means.Count; i++)
            {
                float gap = XOf(plot, panel.Means[i].X, xLo, xHi) - XOf(plot, panel.Means[i - 1].X, xLo, xHi);
                width = Mathf.Min(width, gap * 0.5f);
            }
            return width;
        }

        private static float XOf(Rect plot, double value, double lo, double hi)
        {
            return plot.x + plot.width * (float)((value - lo) / (hi - lo));
        }

        private float YOf(Rect plot, double value)
        {
            return plot.yMax - plot.height * Mathf.Clamp01((float)((value - _yLo) / (_yHi - _yLo)));
        }

        private static Rect Square(Vector2 center, float size)
        {
            return new Rect(center.x - size * 0.5f, center.y - size * 0.5f, size, size);
        }

        private readonly struct Point
        {
            public readonly int Case;
            public readonly double X;
            public readonly double Y;
            public readonly float Jitter;

            public Point(int caseIndex, double x, double y)
            {
                Case = caseIndex;
                X = x;
                Y = y;
                Jitter = (unchecked((uint)caseIndex * 2654435761u) >> 8) / (float)(1 << 24) - 0.5f;
            }
        }

        private readonly struct Candidate
        {
            public readonly double X;
            public readonly double Mean;
            public readonly int Count;

            public Candidate(double x, double mean, int count)
            {
                X = x;
                Mean = mean;
                Count = count;
            }
        }

        private class Panel
        {
            public readonly int Column;
            public readonly List<Point> Points;
            public readonly List<Candidate> Means;
            public readonly int MaxPerCandidate;
            public readonly double? Effect;

            public Panel(int column, List<Point> points)
            {
                Column = column;
                Points = points;
                Means = points
                    .GroupBy(p => p.X)
                    .OrderBy(g => g.Key)
                    .Select(g => new Candidate(g.Key, g.Average(p => p.Y), g.Count()))
                    .ToList();
                MaxPerCandidate = Means.Count > 0 ? Means.Max(m => m.Count) : 0;
                Effect = Means.Count >= 2 ? Means.Max(m => m.Mean) - Means.Min(m => m.Mean) : null;
            }
        }
    }
}
