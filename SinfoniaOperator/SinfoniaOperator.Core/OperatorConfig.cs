using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

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
        ///     フラットなJSONファイル（{"KEY": "value", ...}）を読み込み、設定値として登録する。
        ///     ファイルが存在しない場合はfalseを返す。
        /// </summary>
        /// <param name="path">JSON設定ファイルのパス。</param>
        /// <returns>ファイルを読み込めた場合はtrue。</returns>
        public static bool LoadJsonFile(string path)
        {
            return LoadJsonFile(path, Array.Empty<string>());
        }

        /// <summary>
        ///     フラットなJSONファイルから、指定したキーだけを設定値として登録する。
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
                // オブジェクトや配列は設定値として扱わない。
                if (prop.Value.Type == JTokenType.Object || prop.Value.Type == JTokenType.Array) { continue; }
                if (includedKeySet != null && !includedKeySet.Contains(prop.Name)) { continue; }

                _overrides[prop.Name] = prop.Value.Type == JTokenType.Null ? string.Empty : prop.Value.ToString();
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

        private static readonly Dictionary<string, string> _overrides = new();
    }
}
