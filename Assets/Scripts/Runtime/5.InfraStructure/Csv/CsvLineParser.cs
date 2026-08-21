using System.Collections.Generic;
using System.Text;

namespace KillChord.Runtime.InfraStructure.Csv
{
    /// <summary>
    ///     CSV の 1 行を列へ分解する共通処理を提供します。
    /// </summary>
    /// <remarks>
    ///     ダブルクォートで囲まれたフィールドと、その中の <c>""</c> によるエスケープに対応します。
    /// </remarks>
    public static class CsvLineParser
    {
        /// <summary>
        ///     CSV 1 行を列配列へ分解します。
        /// </summary>
        /// <param name="line"> 分解する 1 行です。 </param>
        /// <returns> 列ごとの文字列一覧です。 </returns>
        public static List<string> ParseLine(string line)
        {
            var fields = new List<string>();
            if (line == null)
            {
                fields.Add(string.Empty);
                return fields;
            }

            var current = new StringBuilder(line.Length);
            bool inQuote = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQuote)
                {
                    if (c != QUOTE)
                    {
                        current.Append(c);
                        continue;
                    }

                    // 連続するダブルクォートはエスケープされた 1 文字として扱う。
                    bool isEscapedQuote = i + 1 < line.Length && line[i + 1] == QUOTE;
                    if (isEscapedQuote)
                    {
                        current.Append(QUOTE);
                        i++;
                        continue;
                    }

                    inQuote = false;
                    continue;
                }

                if (c == SEPARATOR)
                {
                    fields.Add(current.ToString());
                    current.Clear();
                    continue;
                }

                if (c == QUOTE)
                {
                    inQuote = true;
                    continue;
                }

                current.Append(c);
            }

            fields.Add(current.ToString());
            return fields;
        }

        /// <summary>
        ///     指定位置のフィールド値を取得します。範囲外の場合は空文字を返します。
        /// </summary>
        /// <param name="fields"> 列ごとの文字列一覧です。 </param>
        /// <param name="index"> 取得する列番号です。 </param>
        /// <returns> フィールド値です。 </returns>
        public static string GetField(IReadOnlyList<string> fields, int index)
        {
            return fields != null && index >= 0 && index < fields.Count ? fields[index] : string.Empty;
        }

        /// <summary> 列の区切り文字です。 </summary>
        private const char SEPARATOR = ',';

        /// <summary> フィールドを囲むダブルクォートです。 </summary>
        private const char QUOTE = '"';
    }
}
