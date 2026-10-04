using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BalanceSim.Editor
{
    public class SimHeatmap
    {
        private const float CellGap = 2f;
        private const float CellMinSize = 8f;
        private const float CellMaxWidth = 96f;
        private const float CellMaxHeight = 40f;
        private const float CellTextMinWidth = 40f;
        private const float CellOneLineHeight = 18f;
        private const float CellTwoLineHeight = 34f;
        private const float LabelGap = 6f;
        private const float AxisLabelHeight = 16f;
        private const float LegendWidth = 200f;
        private const float LegendBarHeight = 10f;
        private const int LegendSteps = 40;

        private static readonly Color[] Ramp =
        {
            Hex(0xcde2fb), Hex(0xb7d3f6), Hex(0x9ec5f4), Hex(0x86b6ef), Hex(0x6da7ec), Hex(0x5598e7), Hex(0x3987e5),
            Hex(0x2a78d6), Hex(0x256abf), Hex(0x1c5cab), Hex(0x184f95), Hex(0x104281), Hex(0x0d366b),
        };

        private static GUIStyle _cellStyle;

        private readonly SimSummaryColumns _columns;
        private readonly Action<int> _onOpen;
        private (int metric, int x, int y) _built = (-1, -1, -1);
        private double[] _xs = Array.Empty<double>();
        private double[] _ys = Array.Empty<double>();
        private Cell[,] _cells = new Cell[0, 0];
        private int _filled;
        private double _min;
        private double _max;

        private int _hoverX = -1;
        private int _hoverY = -1;
        private Vector2 _hoverMouse;

        public SimHeatmap(SimSummaryColumns columns, Action<int> onOpen)
        {
            _columns = columns;
            _onOpen = onOpen;
        }

        private static GUIStyle CellStyle => _cellStyle ??= new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Clip };

        public void ClearHover()
        {
            _hoverX = -1;
            _hoverY = -1;
        }

        public bool OnGUI(int metric, int xColumn, int yColumn, float width)
        {
            if (_columns.ValueCount < 2)
            {
                EditorGUILayout.HelpBox("振った値が2つ以上あるときに使えます。", MessageType.Info);
                return false;
            }
            if (xColumn == yColumn)
            {
                EditorGUILayout.HelpBox("縦軸と横軸には別の値を選んでください。", MessageType.Info);
                return false;
            }
            if (_built != (metric, xColumn, yColumn))
            {
                Build(metric, xColumn, yColumn);
            }
            if (_filled == 0)
            {
                EditorGUILayout.HelpBox("この2つの値を同時に動かした通りがありません。1行ずつの探索では1通りで動かす値は1つだけなので、この表示は使えません。", MessageType.Info);
                return false;
            }

            Event e = Event.current;
            bool moving = e.type == EventType.MouseMove;
            int nextX = -1;
            int nextY = -1;

            string[] yLabels = _ys.Select(v => _columns.Format(yColumn, v)).ToArray();
            float yLabelWidth = yLabels.Max(l => EditorStyles.miniLabel.CalcSize(new GUIContent(l)).x) + LabelGap;
            float cellWidth = Mathf.Clamp((width - yLabelWidth) / _xs.Length, CellMinSize, CellMaxWidth);
            float cellHeight = Mathf.Clamp(cellWidth * 0.5f, CellMinSize, CellMaxHeight);

            GUILayout.Label($"縦: {_columns.Headers[yColumn]}", EditorStyles.miniLabel);
            Rect area = GUILayoutUtility.GetRect(0f, cellHeight * _ys.Length + AxisLabelHeight + LabelGap, GUILayout.ExpandWidth(true));
            float left = area.x + yLabelWidth;
            Rect CellRect(int ix, int iy) => new(left + ix * cellWidth, area.y + (_ys.Length - 1 - iy) * cellHeight, cellWidth - CellGap, cellHeight - CellGap);

            if (moving || (e.type == EventType.MouseDown && e.clickCount == 2))
            {
                int ix = Mathf.FloorToInt((e.mousePosition.x - left) / cellWidth);
                int iy = _ys.Length - 1 - Mathf.FloorToInt((e.mousePosition.y - area.y) / cellHeight);
                if (ix >= 0 && ix < _xs.Length && iy >= 0 && iy < _ys.Length && _cells[ix, iy].Count > 0)
                {
                    if (moving)
                    {
                        nextX = ix;
                        nextY = iy;
                    }
                    else if (_cells[ix, iy].Count == 1)
                    {
                        _onOpen(_cells[ix, iy].SingleCase);
                        e.Use();
                    }
                }
            }

            if (e.type == EventType.Repaint)
            {
                DrawCells(metric, cellWidth, cellHeight, CellRect);
                DrawAxisLabels(xColumn, yLabels, area, left, yLabelWidth, cellWidth, cellHeight);
            }

            GUILayout.Label($"横: {_columns.Headers[xColumn]}", EditorStyles.miniLabel);
            GUILayout.Space(LabelGap);
            DrawLegend(metric);

            if (e.type == EventType.Repaint && _hoverX >= 0 && _hoverY >= 0)
            {
                SimChartGUI.DrawTooltip(_hoverMouse, TooltipText(metric, xColumn, yColumn, _hoverX, _hoverY), width);
            }

            if (!moving)
            {
                return false;
            }

            bool changed = nextX != _hoverX || nextY != _hoverY || nextX >= 0;
            _hoverX = nextX;
            _hoverY = nextY;
            _hoverMouse = e.mousePosition;
            return changed;
        }

        private void Build(int metric, int xColumn, int yColumn)
        {
            _built = (metric, xColumn, yColumn);
            double?[][] keys = _columns.Keys;
            List<int> cases = Enumerable.Range(0, _columns.CaseCount)
                .Where(i => keys[i][xColumn].HasValue && keys[i][yColumn].HasValue && keys[i][metric].HasValue)
                .ToList();
            _xs = cases.Select(i => keys[i][xColumn].Value).Distinct().OrderBy(v => v).ToArray();
            _ys = cases.Select(i => keys[i][yColumn].Value).Distinct().OrderBy(v => v).ToArray();
            _cells = new Cell[_xs.Length, _ys.Length];
            foreach (int i in cases)
            {
                int ix = Array.BinarySearch(_xs, keys[i][xColumn].Value);
                int iy = Array.BinarySearch(_ys, keys[i][yColumn].Value);
                _cells[ix, iy].Add(i, keys[i][metric].Value);
            }

            List<double> means = new();
            foreach (Cell cell in _cells)
            {
                if (cell.Count > 0)
                {
                    means.Add(cell.Mean);
                }
            }
            _filled = means.Count;
            _min = means.Count > 0 ? means.Min() : 0.0;
            _max = means.Count > 0 ? means.Max() : 0.0;
            ClearHover();
        }

        private void DrawCells(int metric, float cellWidth, float cellHeight, Func<int, int, Rect> cellRect)
        {
            for (int ix = 0; ix < _xs.Length; ix++)
            {
                for (int iy = 0; iy < _ys.Length; iy++)
                {
                    Rect rect = cellRect(ix, iy);
                    Cell cell = _cells[ix, iy];
                    if (cell.Count == 0)
                    {
                        EditorGUI.DrawRect(rect, SimResultWindow.GridColor);
                        continue;
                    }

                    if (ix == _hoverX && iy == _hoverY)
                    {
                        EditorGUI.DrawRect(new Rect(rect.x - CellGap * 0.5f, rect.y - CellGap * 0.5f, rect.width + CellGap, rect.height + CellGap), SimResultWindow.HoverColor);
                    }

                    Color color = ColorOf(cell.Mean);
                    EditorGUI.DrawRect(rect, color);
                    if (cellWidth < CellTextMinWidth || cellHeight < CellOneLineHeight)
                    {
                        continue;
                    }

                    string text = _columns.Format(metric, cell.Mean);
                    if (cellHeight >= CellTwoLineHeight)
                    {
                        text += $"\n{cell.Count} 通り";
                    }
                    CellStyle.normal.textColor = Luminance(color) > 0.5f ? Color.black : Color.white;
                    GUI.Label(rect, text, CellStyle);
                }
            }
        }

        private void DrawAxisLabels(int xColumn, string[] yLabels, Rect area, float left, float yLabelWidth, float cellWidth, float cellHeight)
        {
            for (int iy = 0; iy < _ys.Length; iy++)
            {
                float y = area.y + (_ys.Length - 1 - iy) * cellHeight;
                GUI.Label(new Rect(area.x, y, yLabelWidth - LabelGap, cellHeight - CellGap), yLabels[iy], SimResultWindow.RightAlignedStyle);
            }

            float labelY = area.y + _ys.Length * cellHeight;
            float lastRight = float.NegativeInfinity;
            for (int ix = 0; ix < _xs.Length; ix++)
            {
                string label = _columns.Format(xColumn, _xs[ix]);
                float labelWidth = SimResultWindow.CenteredStyle.CalcSize(new GUIContent(label)).x;
                float center = left + ix * cellWidth + (cellWidth - CellGap) * 0.5f;
                if (center - labelWidth * 0.5f < lastRight + 4f)
                {
                    continue;
                }
                GUI.Label(new Rect(center - labelWidth * 0.5f, labelY, labelWidth, AxisLabelHeight), label, SimResultWindow.CenteredStyle);
                lastRight = center + labelWidth * 0.5f;
            }
        }

        private void DrawLegend(int metric)
        {
            string minText = _columns.Format(metric, _min);
            string maxText = _columns.Format(metric, _max);
            float minWidth = EditorStyles.miniLabel.CalcSize(new GUIContent(minText)).x;
            float maxWidth = EditorStyles.miniLabel.CalcSize(new GUIContent(maxText)).x;
            Rect rect = GUILayoutUtility.GetRect(0f, AxisLabelHeight, GUILayout.ExpandWidth(true));
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            GUI.Label(new Rect(rect.x, rect.y, minWidth, AxisLabelHeight), minText, EditorStyles.miniLabel);
            float barX = rect.x + minWidth + LabelGap;
            float barY = rect.y + (AxisLabelHeight - LegendBarHeight) * 0.5f;
            float stepWidth = LegendWidth / LegendSteps;
            for (int i = 0; i < LegendSteps; i++)
            {
                double value = _min + (_max - _min) * (i + 0.5) / LegendSteps;
                EditorGUI.DrawRect(new Rect(barX + i * stepWidth, barY, Mathf.Ceil(stepWidth), LegendBarHeight), ColorOf(value));
            }
            GUI.Label(new Rect(barX + LegendWidth + LabelGap, rect.y, maxWidth, AxisLabelHeight), maxText, EditorStyles.miniLabel);
        }

        private string TooltipText(int metric, int xColumn, int yColumn, int ix, int iy)
        {
            Cell cell = _cells[ix, iy];
            var text = new StringBuilder();
            text.AppendLine($"{_columns.Headers[xColumn]} = {_columns.Format(xColumn, _xs[ix])}");
            text.AppendLine($"{_columns.Headers[yColumn]} = {_columns.Format(yColumn, _ys[iy])}");
            text.Append($"{_columns.Headers[metric]}の平均: {_columns.Format(metric, cell.Mean)}（{cell.Count} 通り）");
            if (cell.Count == 1)
            {
                text.Append($"\n#{cell.SingleCase + 1}  ダブルクリックで時系列を開く");
            }
            return text.ToString();
        }

        private Color ColorOf(double value)
        {
            float t = _max - _min > 1e-12 ? (float)((value - _min) / (_max - _min)) : 0.5f;
            if (EditorGUIUtility.isProSkin)
            {
                t = 1f - t;
            }

            float position = Mathf.Clamp01(t) * (Ramp.Length - 1);
            int index = Mathf.Min(Mathf.FloorToInt(position), Ramp.Length - 2);
            return Color.Lerp(Ramp[index], Ramp[index + 1], position - index);
        }

        private static float Luminance(Color c) => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;

        private static Color Hex(int rgb) => new(((rgb >> 16) & 0xff) / 255f, ((rgb >> 8) & 0xff) / 255f, (rgb & 0xff) / 255f);

        private struct Cell
        {
            public int Count;
            public int SingleCase;
            private double _sum;

            public double Mean => Count > 0 ? _sum / Count : 0.0;

            public void Add(int caseIndex, double value)
            {
                SingleCase = Count == 0 ? caseIndex : -1;
                Count++;
                _sum += value;
            }
        }
    }
}
