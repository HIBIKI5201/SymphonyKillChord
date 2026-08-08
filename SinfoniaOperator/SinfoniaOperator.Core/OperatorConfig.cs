using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     設定値の取得元を抽象化するクラス。
    ///     JSON設定ファイルが読み込まれていればその値を優先し、なければ環境変数から取得する。
    ///     複数ファイルで同じキーを読み込んだ場合は、後から読み込んだ値を優先する。
    ///     これにより公開設定、ローカル秘密設定、GitHub Actionsの環境変数を
    ///     同じキー名で扱える。
    /// </summary>
    public static class OperatorConfig
    {
        public const string ENVIRONMENT_CONFIG_FILE_NAME = "sinfonia-operator.env.json";
        public const string SECRETS_CONFIG_FILE_NAME = "sinfonia-operator.secrets.json";
        public const string LEGACY_CONFIG_FILE_NAME = "sinfonia-operator.settings.json";

        /// <summary>
        ///     フラットなJSONファイル（{"KEY": "value", "ARRAY": ["value"], ...}）を読み込み、
        ///     設定値として登録する。
        ///     ファイルが存在しない場合はfalseを返す。
        /// </summary>
        /// <param name="path">JSON設定ファイルのパス。</param>
        /// <returns>ファイルを読み込めた場合はtrue。</returns>
        public static bool LoadJsonFile(string path)
        {
            return LoadJsonFile(path, Array.Empty<string>());
        }

        /// <summary>
        ///     フラットなJSONファイルから、指定したキーのスカラー値または配列だけを設定値として登録する。
        ///     ファイルが存在しない場合はfalseを返す。
        /// </summary>
        /// <param name="path">JSON設定ファイルのパス。</param>
        /// <param name="includedKeys">読み込み対象キー。未指定の場合は全項目。</param>
        /// <returns>ファイルを読み込めた場合はtrue。</returns>
        public static bool LoadJsonFile(string path, params string[] includedKeys)
        {
            if (!File.Exists(path)) { return false; }

            JObject root = JObject.Parse(File.ReadAllText(path));
            HashSet<string>? includedKeySet = includedKeys.Length == 0
                ? null
                : new HashSet<string>(includedKeys, StringComparer.Ordinal);
            int count = 0;
            foreach (JProperty prop in root.Properties())
            {
                if (includedKeySet != null && !includedKeySet.Contains(prop.Name)) { continue; }

                if (prop.Value.Type == JTokenType.Object) { continue; }
                if (prop.Value.Type == JTokenType.Array)
                {
                    string[] values = prop.Value
                        .Children()
                        .Where(value => value.Type != JTokenType.Object && value.Type != JTokenType.Array)
                        .Select(value => value.Type == JTokenType.Null ? string.Empty : value.ToString())
                        .ToArray();
                    _arrayOverrides[prop.Name] = values;
                    _overrides.Remove(prop.Name);
                    count++;
                    continue;
                }

                _overrides[prop.Name] = prop.Value.Type == JTokenType.Null ? string.Empty : prop.Value.ToString();
                _arrayOverrides.Remove(prop.Name);
                count++;
            }

            OperatorLog.Write($"[OperatorConfig] JSON設定を読み込みました: {path} ({count} 件)");
            return true;
        }

        /// <summary>
        ///     読み込んだJSON設定を破棄する。
        /// </summary>
        public static void ClearOverrides()
        {
            _overrides.Clear();
            _arrayOverrides.Clear();
        }

        /// <summary>
        ///     設定値を取得する。JSON設定を優先し、なければ環境変数から取得する。
        ///     どちらにも無い場合は空文字を返す。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetValue(string key)
        {
            if (_overrides.TryGetValue(key, out string? value) && !string.IsNullOrEmpty(value))
            {
                return value;
            }

            return Environment.GetEnvironmentVariable(key) ?? string.Empty;
        }

        /// <summary>
        ///     設定値の配列を取得する。JSON配列を優先し、なければ環境変数のJSON配列または
        ///     カンマ区切り文字列を読み取る。
        /// </summary>
        /// <param name="key">設定キー。</param>
        /// <returns>設定された文字列配列。未設定の場合は空配列。</returns>
        public static string[] GetValues(string key)
        {
            if (_arrayOverrides.TryGetValue(key, out string[]? values))
            {
                return values.ToArray();
            }

            string environmentValue = Environment.GetEnvironmentVariable(key) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(environmentValue)) { return Array.Empty<string>(); }

            try
            {
                JToken token = JToken.Parse(environmentValue);
                if (token.Type == JTokenType.Array)
                {
                    return token.Children()
                        .Where(value => value.Type != JTokenType.Object && value.Type != JTokenType.Array)
                        .Select(value => value.Type == JTokenType.Null ? string.Empty : value.ToString())
                        .ToArray();
                }
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                // JSON配列でない場合は、カンマ区切りとして扱う。
            }

            return environmentValue
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(value => value.Trim())
                .ToArray();
        }

        private static readonly Dictionary<string, string> _overrides = new();
        private static readonly Dictionary<string, string[]> _arrayOverrides = new();
    }
}
