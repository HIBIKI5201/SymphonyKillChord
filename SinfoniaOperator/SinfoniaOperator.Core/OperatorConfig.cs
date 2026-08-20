using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     設定値の取得元を抽象化するクラス。
    ///     JSON設定ファイルが読み込まれていればその値を優先し、なければ環境変数から取得する。
    ///     これによりローカルではJSONファイル、GitHub Actionsではシークレット(環境変数)の
    ///     どちらでも同じキー名で実行できる。
    /// </summary>
    public static class OperatorConfig
    {
        /// <summary>
        ///     フラットなJSONファイル（{"KEY": "value", ...}）を読み込み、設定値として登録する。
        ///     ファイルが存在しない場合はfalseを返す。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool LoadJsonFile(string path)
        {
            if (!File.Exists(path)) { return false; }

            JObject root = JObject.Parse(File.ReadAllText(path));
            int count = 0;
            foreach (JProperty prop in root.Properties())
            {
                // オブジェクトや配列は設定値として扱わない。
                if (prop.Value.Type == JTokenType.Object || prop.Value.Type == JTokenType.Array) { continue; }

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
