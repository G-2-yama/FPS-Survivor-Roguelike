using System;
using UnityEditor;
using UnityEngine;

namespace BalanceSim.Editor
{
    internal static class SimChartGUI
    {
        private const float TooltipOffset = 14f;

        private static GUIStyle _tooltipStyle;
        private static GUIStyle _clippedBoldStyle;

        public static GUIStyle ClippedBoldStyle => _clippedBoldStyle ??= new GUIStyle(EditorStyles.boldLabel) { clipping = TextClipping.Clip };

        private static GUIStyle TooltipStyle => _tooltipStyle ??= new GUIStyle(EditorStyles.label)
        {
            padding = new RectOffset(6, 6, 4, 4),
            wordWrap = false,
            richText = false,
        };

        private static Color TooltipBackground => EditorGUIUtility.isProSkin ? new Color(0.1f, 0.1f, 0.1f, 0.95f) : new Color(1f, 1f, 1f, 0.97f);

        public static void DrawTooltip(Vector2 anchor, string text, float maxX)
        {
            var content = new GUIContent(text);
            Vector2 size = TooltipStyle.CalcSize(content);
            var rect = new Rect(anchor.x + TooltipOffset, anchor.y + TooltipOffset, size.x, size.y);
            if (rect.xMax > maxX)
            {
                rect.x = Mathf.Max(0f, anchor.x - TooltipOffset - size.x);
            }

            EditorGUI.DrawRect(new Rect(rect.x - 1f, rect.y - 1f, rect.width + 2f, rect.height + 2f), SimResultWindow.HoverColor);
            EditorGUI.DrawRect(rect, TooltipBackground);
            GUI.Label(rect, content, TooltipStyle);
        }

        public static (double lo, double hi, double step) NiceRange(double min, double max, int tickTarget)
        {
            if (max - min < 1e-9)
            {
                double pad = Math.Abs(max) > 1e-9 ? Math.Abs(max) * 0.1 : 1.0;
                bool nonNegative = min >= 0.0;
                min -= pad;
                max += pad;
                if (nonNegative && min < 0.0)
                {
                    min = 0.0;
                }
            }

            double step = NiceStep((max - min) / tickTarget);
            double lo = Math.Floor(min / step) * step;
            double hi = Math.Ceiling(max / step) * step;
            if (min >= 0.0 && lo < 0.0)
            {
                lo = 0.0;
            }
            return (lo, hi, step);
        }

        public static double NiceStep(double rough)
        {
            if (rough <= 0.0)
            {
                return 1.0;
            }

            double magnitude = Math.Pow(10.0, Math.Floor(Math.Log10(rough)));
            double normalized = rough / magnitude;
            double nice = normalized <= 1.0 ? 1.0 : normalized <= 2.0 ? 2.0 : normalized <= 5.0 ? 5.0 : 10.0;
            return nice * magnitude;
        }

        public static string TickLabel(double value, double step, Func<double, string> format)
        {
            int decimals = step >= 1.0 ? 0 : Mathf.Clamp(-Mathf.FloorToInt(Mathf.Log10((float)step) + 1e-4f), 0, 6);
            return value.ToString("F" + decimals) + UnitOf(format);
        }

        private static string UnitOf(Func<double, string> format)
        {
            string sample = format(0.0);
            int i = 0;
            while (i < sample.Length && (char.IsDigit(sample[i]) || sample[i] == '.' || sample[i] == '-'))
            {
                i++;
            }
            return sample.Substring(i);
        }
    }
}
