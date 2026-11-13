using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Editor.SpecViewer
{
    /// <summary>
    ///     表のブロックを、ページの幅に合わせた格子として描く。
    ///     列の幅は、列の中の最も長いセルの文字数に合わせて配分する（長い文の列を広くする）。
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
            int rowCount = Mathf.Min(table.Rows.Count, maxRows);
            float[] weights = CalculateColumnWeights(table, rowCount);
            VisualElement grid = new() { style = { marginTop = MARGIN, marginBottom = MARGIN, flexShrink = 0 } };
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                bool isHeader = table.HasHeaderRow && rowIndex == 0;
                grid.Add(CreateRow(table.Rows[rowIndex], weights, isHeader, minCellWidth, createCell));
            }

            if (table.Rows.Count > maxRows)
            {
                grid.Add(new Label($"ほか {table.Rows.Count - maxRows} 行は省略しました。全部見るときは Notion で開いてください。"));
            }

            return grid;
        }

        private const float MARGIN = 4f;
        private const float BORDER_WIDTH = 1f;
        private const float CELL_PADDING = 4f;
        private const float MIN_WEIGHT_CHARACTERS = 4f;
        private const float MAX_WEIGHT_CHARACTERS = 60f;

        /// <summary>
        ///     列ごとの幅の比率を、列の中で最も長いセルの文字数から決める。
        /// </summary>
        /// <param name="table">表のブロック。</param>
        /// <param name="rowCount">描く行数。</param>
        /// <returns>列ごとの比率。</returns>
        private static float[] CalculateColumnWeights(SpecBlock table, int rowCount)
        {
            int columnCount = 0;
            for (int row = 0; row < rowCount; row++) { columnCount = Mathf.Max(columnCount, table.Rows[row].Count); }

            float[] weights = new float[columnCount];
            for (int row = 0; row < rowCount; row++)
            {
                IReadOnlyList<string> cells = table.Rows[row];
                for (int column = 0; column < cells.Count; column++)
                {
                    weights[column] = Mathf.Max(weights[column], cells[column]?.Length ?? 0);
                }
            }

            // 極端に偏らないよう、比率に上限と下限を付ける。
            for (int column = 0; column < columnCount; column++)
            {
                weights[column] = Mathf.Clamp(weights[column], MIN_WEIGHT_CHARACTERS, MAX_WEIGHT_CHARACTERS);
            }

            return weights;
        }

        /// <summary>
        ///     表の1行を描く。足りないセルは空で埋める。
        /// </summary>
        /// <param name="cells">セル。</param>
        /// <param name="weights">列ごとの幅の比率。</param>
        /// <param name="isHeader">見出し行か。</param>
        /// <param name="minCellWidth">セルの最小幅。</param>
        /// <param name="createCell">セルの本文からラベルを作る処理。</param>
        /// <returns>行の要素。</returns>
        private static VisualElement CreateRow(
            IReadOnlyList<string> cells,
            float[] weights,
            bool isHeader,
            float minCellWidth,
            Func<string, Label> createCell)
        {
            VisualElement row = new() { style = { flexDirection = FlexDirection.Row, flexShrink = 0 } };
            if (isHeader) { row.style.backgroundColor = SpecViewerStyles.TableHeaderBackground; }

            for (int column = 0; column < weights.Length; column++)
            {
                Label cell = createCell(column < cells.Count ? cells[column] : string.Empty);
                cell.style.flexGrow = weights[column];
                cell.style.flexShrink = 1;
                cell.style.flexBasis = 0;
                cell.style.minWidth = minCellWidth;
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
