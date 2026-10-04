using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BalanceSim.Editor
{
    public class SimResultTable : TreeView<int>
    {
        private const float CellPadding = 16f;
        private const float SortArrowWidth = 16f;

        private readonly string[][] _texts;
        private readonly double?[][] _keys;
        private readonly Action<int> _onOpen;
        private readonly List<int> _order = new();

        private SimResultTable(TreeViewState<int> state, MultiColumnHeader header, string[][] texts, double?[][] keys, Action<int> onOpen)
            : base(state, header)
        {
            _texts = texts;
            _keys = keys;
            _onOpen = onOpen;
            showAlternatingRowBackgrounds = true;
            header.sortingChanged += _ => Reload();
            Reload();
        }

        public int Count => _order.Count;

        public int CaseAt(int position) => _order[position];

        public int PositionOf(int caseIndex) => _order.IndexOf(caseIndex);

        public static SimResultTable Create(SimSummaryColumns source, TreeViewState<int> state, ref MultiColumnHeaderState headerState, Action<int> onOpen)
        {
            string[] headers = source.Headers;
            double?[][] keys = source.Keys;
            string[][] texts = keys
                .Select(row => row.Select((key, col) => source.Format(col, key)).ToArray())
                .ToArray();

            var columns = new MultiColumnHeaderState.Column[headers.Length];
            for (int col = 0; col < headers.Length; col++)
            {
                var content = new GUIContent(headers[col]);
                float width = MultiColumnHeader.DefaultStyles.columnHeader.CalcSize(content).x + SortArrowWidth;
                foreach (string[] row in texts)
                {
                    width = Mathf.Max(width, EditorStyles.label.CalcSize(new GUIContent(row[col])).x);
                }

                columns[col] = new MultiColumnHeaderState.Column
                {
                    headerContent = content,
                    headerTextAlignment = TextAlignment.Right,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = width + CellPadding,
                    autoResize = false,
                    allowToggleVisibility = false,
                    canSort = true,
                    sortedAscending = true,
                };
            }

            var newState = new MultiColumnHeaderState(columns);
            if (headerState != null && SameHeaders(headerState, newState))
            {
                MultiColumnHeaderState.OverwriteSerializedFields(headerState, newState);
            }
            headerState = newState;

            return new SimResultTable(state, new MultiColumnHeader(newState), texts, keys, onOpen);
        }

        private static bool SameHeaders(MultiColumnHeaderState a, MultiColumnHeaderState b)
        {
            return a.columns.Length == b.columns.Length
                && a.columns.Zip(b.columns, (x, y) => x.headerContent?.text == y.headerContent?.text).All(same => same);
        }

        protected override TreeViewItem<int> BuildRoot()
        {
            var root = new TreeViewItem<int>(-1, -1);
            for (int i = 0; i < _texts.Length; i++)
            {
                root.AddChild(new TreeViewItem<int>(i, 0));
            }
            return root;
        }

        protected override IList<TreeViewItem<int>> BuildRows(TreeViewItem<int> root)
        {
            IList<TreeViewItem<int>> rows = base.BuildRows(root);
            int column = multiColumnHeader.sortedColumnIndex;
            if (column >= 0)
            {
                double sign = multiColumnHeader.IsSortedAscending(column) ? 1.0 : -1.0;
                List<TreeViewItem<int>> sorted = rows
                    .OrderBy(r => _keys[r.id][column].HasValue ? 0 : 1)
                    .ThenBy(r => sign * (_keys[r.id][column] ?? 0.0))
                    .ToList();
                rows.Clear();
                foreach (TreeViewItem<int> row in sorted)
                {
                    rows.Add(row);
                }
            }

            _order.Clear();
            _order.AddRange(rows.Select(r => r.id));
            return rows;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            string[] texts = _texts[args.item.id];
            for (int i = 0; i < args.GetNumVisibleColumns(); i++)
            {
                Rect rect = args.GetCellRect(i);
                CenterRectUsingSingleLineHeight(ref rect);
                DefaultGUI.LabelRightAligned(rect, texts[args.GetColumn(i)], args.selected, args.focused);
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            _onOpen(id);
        }
    }
}
