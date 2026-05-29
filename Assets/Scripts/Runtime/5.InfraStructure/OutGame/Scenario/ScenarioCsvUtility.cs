using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    /// <summary>
    /// シナリオ CSV の解析で使う共通処理を提供する。
    /// </summary>
    internal static class ScenarioCsvUtility
    {
        /// <summary>
        /// ヘッダー名から列番号を引ける辞書を構築する。
        /// </summary>
        public static Dictionary<string, int> BuildHeaderIndex(IReadOnlyList<string> headers)
        {
            var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Count; i++)
            {
                string key = headers[i]?.Trim();
                if (string.IsNullOrEmpty(key)) continue;
                index[key] = i;
            }

            return index;
        }

        /// <summary>
        /// 列名に対応する値を取得する。
        /// </summary>
        public static string GetValue(IReadOnlyList<string> values, IReadOnlyDictionary<string, int> headerIndex, string key)
        {
            if (!headerIndex.TryGetValue(key, out int idx)) return string.Empty;
            if (idx < 0 || idx >= values.Count) return string.Empty;
            return values[idx];
        }

        /// <summary>
        /// 指定位置のフィールド値を取得する。
        /// </summary>
        public static string GetField(IReadOnlyList<string> fields, int index)
        {
            return index >= 0 && index < fields.Count ? fields[index] : string.Empty;
        }

        /// <summary>
        /// 必須の整数値を解析する。
        /// </summary>
        public static int ParseRequiredInt(string raw, string columnName, int lineNo)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new FormatException($"line {lineNo}: {columnName} is required.");
            }

            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                throw new FormatException($"line {lineNo}: {columnName} must be int.");
            }
            return value;
        }

        /// <summary>
        /// 必須の小数値を解析する。
        /// </summary>
        public static float ParseRequiredFloat(string raw, string columnName, int lineNo)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new FormatException($"line {lineNo}: {columnName} is required.");
            }

            if (!float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                throw new FormatException($"line {lineNo}: {columnName} must be number.");
            }

            return value;
        }

        /// <summary>
        /// 任意の整数値を既定値付きで解析する。
        /// </summary>
        public static int ParseOptionalInt(string raw, int defaultValue, string columnName, int lineNo)
        {
            if (string.IsNullOrWhiteSpace(raw)) return defaultValue;

            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                throw new FormatException($"line {lineNo}: {columnName} must be int.");
            }

            return value;
        }

        /// <summary>
        /// 任意の小数値を既定値付きで解析する。
        /// </summary>
        public static float ParseOptionalFloat(string raw, float defaultValue, string columnName, int lineNo)
        {
            if (string.IsNullOrWhiteSpace(raw)) return defaultValue;

            if (!float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                throw new FormatException($"line {lineNo}: {columnName} must be number.");
            }

            return value;
        }

        /// <summary>
        /// 任意の真偽値を既定値付きで解析する。
        /// </summary>
        public static bool ParseOptionalBool(string raw, bool defaultValue, string columnName, int lineNo)
        {
            if (string.IsNullOrWhiteSpace(raw)) return defaultValue;

            if (bool.TryParse(raw, out bool boolValue))
            {
                return boolValue;
            }

            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
            {
                return intValue != 0;
            }

            throw new FormatException($"line {lineNo}: {columnName} must be bool or 0/1.");
        }

        /// <summary>
        /// CSV 1 行を列配列へ分解する。
        /// </summary>
        public static List<string> ParseCsvLine(string line)
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
                    if (c == '"')
                    {
                        bool escapedQuote = i + 1 < line.Length && line[i + 1] == '"';
                        if (escapedQuote)
                        {
                            current.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuote = false;
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == ',')
                    {
                        fields.Add(current.ToString());
                        current.Clear();
                    }
                    else if (c == '"')
                    {
                        inQuote = true;
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }

            fields.Add(current.ToString());
            return fields;
        }
    }
}