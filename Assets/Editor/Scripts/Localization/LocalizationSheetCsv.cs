using System;
using System.Collections.Generic;
using System.Text;

namespace KillChord.Editor.Localization
{
    /// <summary>
    ///     訳語の正本スプレッドシート（キー / 日本語 / 英語 / 備考）のCSVを読み書きします。
    /// </summary>
    public static class LocalizationSheetCsv
    {
        /// <summary> キー列の見出しです。 </summary>
        public const string KEY_HEADER = "キー";
        /// <summary> 日本語列の見出しです。 </summary>
        public const string JAPANESE_HEADER = "日本語";
        /// <summary> 英語列の見出しです。 </summary>
        public const string ENGLISH_HEADER = "英語";
        /// <summary> 備考列の見出しです。 </summary>
        public const string NOTE_HEADER = "備考";

        /// <summary>
        ///     CSVの文字列を行の一覧に変換します。見出し行の列名で列を探すため、列の順番は問いません。
        /// </summary>
        /// <param name="csvText"> スプレッドシートから書き出したCSVです。 </param>
        /// <param name="rows"> 読み取った行の一覧です。キーが空の行は含みません。 </param>
        /// <param name="error"> 読み取れなかったときの理由です。 </param>
        /// <returns> 読み取れたときはtrueです。 </returns>
        public static bool TryParse(string csvText, out List<LocalizationSheetRow> rows, out string error)
        {
            rows = new List<LocalizationSheetRow>();
            List<List<string>> records = ParseRecords(csvText ?? string.Empty);
            if (records.Count == 0)
            {
                error = "CSVが空です。";
                return false;
            }

            // 見出し行から列の位置を決める。
            List<string> header = records[0];
            int keyColumn = FindColumn(header, KEY_HEADER);
            int japaneseColumn = FindColumn(header, JAPANESE_HEADER);
            int englishColumn = FindColumn(header, ENGLISH_HEADER);
            int noteColumn = FindColumn(header, NOTE_HEADER);
            if (keyColumn < 0 || japaneseColumn < 0 || englishColumn < 0)
            {
                error = $"見出し行に「{KEY_HEADER}」「{JAPANESE_HEADER}」「{ENGLISH_HEADER}」の列がありません。";
                return false;
            }

            var usedKeys = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 1; i < records.Count; i++)
            {
                List<string> record = records[i];
                string key = GetCell(record, keyColumn).Trim();
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                // 同じキーが2行あると、どちらの訳を使うか決められないため読み取りをやめる。
                if (!usedKeys.Add(key))
                {
                    error = $"キー「{key}」が複数の行にあります（{i + 1}行目）。";
                    return false;
                }

                rows.Add(new LocalizationSheetRow(
                    key,
                    GetCell(record, japaneseColumn),
                    GetCell(record, englishColumn),
                    GetCell(record, noteColumn)));
            }

            error = string.Empty;
            return true;
        }

        /// <summary>
        ///     行の一覧を、見出し付きのCSVの文字列に変換します。
        /// </summary>
        /// <param name="rows"> 書き出す行の一覧です。 </param>
        /// <returns> CSVの文字列です。改行はCRLFです。 </returns>
        public static string Write(IEnumerable<LocalizationSheetRow> rows)
        {
            var builder = new StringBuilder();
            AppendRecord(builder, KEY_HEADER, JAPANESE_HEADER, ENGLISH_HEADER, NOTE_HEADER);
            foreach (LocalizationSheetRow row in rows)
            {
                AppendRecord(builder, row.Key, row.Japanese, row.English, row.Note);
            }

            return builder.ToString();
        }

        private const char QUOTE = '"';
        private const char SEPARATOR = ',';
        private const string RECORD_SEPARATOR = "\r\n";

        /// <summary>
        ///     RFC 4180 形式のCSVを、レコードごとのセルの一覧に分けます。
        ///     セル内の改行はLFにそろえます。
        /// </summary>
        private static List<List<string>> ParseRecords(string csvText)
        {
            var records = new List<List<string>>();
            var record = new List<string>();
            var cell = new StringBuilder();
            bool isQuoted = false;
            bool hasContent = false;

            for (int i = 0; i < csvText.Length; i++)
            {
                char c = csvText[i];
                if (isQuoted)
                {
                    if (c == QUOTE)
                    {
                        // 引用符の中の "" は " 1文字を表す。
                        if (i + 1 < csvText.Length && csvText[i + 1] == QUOTE)
                        {
                            cell.Append(QUOTE);
                            i++;
                        }
                        else
                        {
                            isQuoted = false;
                        }
                    }
                    else if (c != '\r')
                    {
                        cell.Append(c);
                    }

                    continue;
                }

                switch (c)
                {
                    case QUOTE:
                        isQuoted = true;
                        hasContent = true;
                        break;
                    case SEPARATOR:
                        record.Add(cell.ToString());
                        cell.Clear();
                        hasContent = true;
                        break;
                    case '\r':
                        break;
                    case '\n':
                        record.Add(cell.ToString());
                        cell.Clear();
                        records.Add(record);
                        record = new List<string>();
                        hasContent = false;
                        break;
                    default:
                        cell.Append(c);
                        hasContent = true;
                        break;
                }
            }

            // 末尾に改行が無いときの最後のレコードを加える。
            if (hasContent || cell.Length > 0)
            {
                record.Add(cell.ToString());
                records.Add(record);
            }

            // BOM が付いていると見出しの1列目が一致しないため取り除く。
            if (records.Count > 0 && records[0].Count > 0)
            {
                records[0][0] = records[0][0].TrimStart('﻿');
            }

            return records;
        }

        /// <summary>
        ///     見出し行から、指定した名前の列の位置を探します。見つからなければ-1を返します。
        /// </summary>
        private static int FindColumn(List<string> header, string name)
        {
            for (int i = 0; i < header.Count; i++)
            {
                if (header[i].Trim() == name)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        ///     レコードから指定した列のセルを取得します。列が無いときは空文字を返します。
        /// </summary>
        private static string GetCell(List<string> record, int column)
        {
            if (column < 0 || column >= record.Count)
            {
                return string.Empty;
            }

            return record[column];
        }

        /// <summary>
        ///     1レコード分のセルをCSVの形で書き足します。
        /// </summary>
        private static void AppendRecord(StringBuilder builder, params string[] cells)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(SEPARATOR);
                }

                builder.Append(Escape(cells[i] ?? string.Empty));
            }

            builder.Append(RECORD_SEPARATOR);
        }

        /// <summary>
        ///     カンマ・引用符・改行を含むセルを引用符で囲みます。
        /// </summary>
        private static string Escape(string cell)
        {
            bool needsQuote = cell.IndexOf(SEPARATOR) >= 0
                || cell.IndexOf(QUOTE) >= 0
                || cell.IndexOf('\n') >= 0
                || cell.IndexOf('\r') >= 0;
            if (!needsQuote)
            {
                return cell;
            }

            return QUOTE + cell.Replace("\"", "\"\"") + QUOTE;
        }
    }
}
