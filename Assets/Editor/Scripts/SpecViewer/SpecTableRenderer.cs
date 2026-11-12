using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Editor.SpecViewer
{
    /// <summary>
    ///     表のブロックを、横にスクロールできる格子として描く。
    ///     データベースの一覧のような大きな表は、決めた行数までだけ描く。
    /// </summary>
    public static class SpecTableRenderer
    {
        /// <summary>
        ///     表を描く。
        /// </summary>
        /// <param name="table">表のブロック。</param>
        /// <param name="createCell">セルの本文からラベルを作る処理。</param>
        /// <param name="maxRows">描く最大行数。</param>
        /// <param name="minCellWidth">セルの最小幅。</param>
        /// <returns>要素。</returns>
        public static VisualElement Render(SpecBlock table, Func<string, Label> createCell, int maxRows, float minCellWidth)
        {
            int columnCount = 0;
            foreach (IReadOnlyList<string> row in table.Rows) { columnCount = Mathf.Max(columnCount, row.Count); }

            ScrollView scroll = new(ScrollViewMode.Horizontal) { style = { marginTop = MARGIN, marginBottom = MARGIN } };
            VisualElement grid = new() { style = { minWidth = columnCount * minCellWidth } };
            scroll.Add(grid);

            int rowCount = Mathf.Min(table.Rows.Count, maxRows);
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                bool isHeader = table.HasHeaderRow && rowIndex == 0;
                grid.Add(CreateRow(table.Rows[rowIndex], columnCount, isHeader, createCell));
            }

            if (table.Rows.Count > maxRows)
            {
                grid.Add(new Label($"ほか {table.Rows.Count - maxRows} 行は省略しました。全部見るときは Notion で開いてください。"));
            }

            return scroll;
        }

        private const float MARGIN = 4f;
        private const float BORDER_WIDTH = 1f;
        private const float CELL_PADDING = 4f;

        /// <summary>
        ///     表の1行を描く。足りないセルは空で埋める。
        /// </summary>
        /// <param name="cells">セル。</param>
        /// <param name="columnCount">列の数。</param>
        /// <param name="isHeader">見出し行か。</param>
        /// <param name="createCell">セルの本文からラベルを作る処理。</param>
        /// <returns>行の要素。</returns>
        private static VisualElement CreateRow(IReadOnlyList<string> cells, int columnCount, bool isHeader, Func<string, Label> createCell)
        {
            VisualElement row = new() { style = { flexDirection = FlexDirection.Row } };
            if (isHeader) { row.style.backgroundColor = SpecViewerStyles.TableHeaderBackground; }

            for (int column = 0; column < columnCount; column++)
            {
                Label cell = createCell(column < cells.Count ? cells[column] : string.Empty);
                cell.style.flexGrow = 1;
                cell.style.flexBasis = 0;
                cell.style.marginTop = 0;
                cell.style.paddingLeft = CELL_PADDING;
                cell.style.paddingRight = CELL_PADDING;
                cell.style.paddingTop = CELL_PADDING / 2f;
                cell.style.paddingBottom = CELL_PADDING / 2f;
                cell.style.borderLeftWidth = BORDER_WIDTH;
                cell.style.borderRightWidth = BORDER_WIDTH;
                cell.style.borderTopWidth = BORDER_WIDTH;
                cell.style.borderBottomWidth = BORDER_WIDTH;
                cell.style.borderLeftColor = SpecViewerStyles.BorderColor;
                cell.style.borderRightColor = SpecViewerStyles.BorderColor;
                cell.style.borderTopColor = SpecViewerStyles.BorderColor;
                cell.style.borderBottomColor = SpecViewerStyles.BorderColor;
                if (isHeader) { cell.style.unityFontStyleAndWeight = FontStyle.Bold; }
                row.Add(cell);
            }

            return row;
        }
    }
}
