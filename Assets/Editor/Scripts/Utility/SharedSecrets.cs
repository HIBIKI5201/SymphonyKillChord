using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SinfoniaStudio.SinfoniaOperator;
using UnityEngine;

namespace KillChord.Editor.Utility
{
    /// <summary>
    ///     SinfoniaOperator の各 CLI と Unity のエディタツールが共用する秘密設定ファイルを読み書きする。
    ///     ファイルは UserSettings/KillChord/secrets.json に置き、Git の管理対象外とする。
    ///     キーは SinfoniaOperator の設定キーと同じく、アッパースネークケースで揃える。
    /// </summary>
    public static class SharedSecrets
    {
        /// <summary> 共通秘密設定ファイルの絶対パス。 </summary>
        public static string FullPath => Path.Combine(
            Directory.GetParent(Application.dataPath).FullName,
            OperatorConfig.SHARED_SECRETS_CONFIG_RELATIVE_PATH);

        /// <summary> Google Drive 取り込み・アップロードに使う Service Account の JSON 鍵のキー名。 </summary>
        public const string GOOGLE_SERVICE_ACCOUNT_JSON_KEY = "GOOGLE_SERVICE_ACCOUNT_JSON_KEY";

        /// <summary> チケットシステムの API キーのキー名。 </summary>
        public const string TICKET_SYSTEM_API_KEY = "TICKET_SYSTEM_API_KEY";

        /// <summary>
        ///     共通ファイルから値を取得する。値が JSON オブジェクトの場合は1行の JSON 文字列で返す。
        ///     ファイルやキーが無い場合は空文字を返す。
        /// </summary>
        /// <param name="key"> 設定キー。 </param>
        /// <returns> 設定値。 </returns>
        public static string GetValue(string key)
        {
            JObject root = LoadRoot();
            if (root == null || !root.TryGetValue(key, out JToken token)) { return string.Empty; }

            return token.Type switch
            {
                JTokenType.Null => string.Empty,
                JTokenType.String => token.Value<string>() ?? string.Empty,
                _ => token.ToString(Formatting.None)
            };
        }

        /// <summary>
        ///     共通ファイルの値を書き換える。ほかのキーはそのまま残す。
        ///     値が JSON オブジェクトとして読める場合は、読みやすさのためオブジェクトのまま保存する。
        ///     空文字を渡した場合はキーを削除する。
        /// </summary>
        /// <param name="key"> 設定キー。 </param>
        /// <param name="value"> 設定値。 </param>
        public static void SetValue(string key, string value)
        {
            JObject root = LoadRoot() ?? new JObject();
            if (string.IsNullOrEmpty(value))
            {
                root.Remove(key);
            }
            else
            {
                root[key] = TryParseObject(value) ?? (JToken)value;
            }

            // フォルダが無い環境でも書けるよう、先にフォルダを作る。
            string path = FullPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, root.ToString(Formatting.Indented) + "\n");
            _cachedRoot = null;
        }

        /// <summary>
        ///     秘密の値を解決する。共通ファイルの値を優先し、無い場合だけ移行前の設定値を使う。
        ///     移行前の値を使ったときは、移行を促す警告をセッション中に1回だけ出す。
        /// </summary>
        /// <param name="key"> 共通ファイルの設定キー。 </param>
        /// <param name="legacyValue"> 移行前の設定に残っている値。 </param>
        /// <param name="settingsLocation"> 移行ボタンがある設定画面の場所。警告文に使う。 </param>
        /// <returns> 解決した値。どちらにも無い場合は空文字。 </returns>
        public static string ResolveValue(string key, string legacyValue, string settingsLocation)
        {
            string sharedValue = GetValue(key);
            if (!string.IsNullOrEmpty(sharedValue)) { return sharedValue; }
            if (string.IsNullOrEmpty(legacyValue)) { return string.Empty; }

            if (_warnedLegacyKeys.Add(key))
            {
                Debug.LogWarning($"[{nameof(SharedSecrets)}] {key} を移行前の設定から読み込みました。" +
                    $"{settingsLocation} の「共通ファイルへ移す」で " +
                    $"{OperatorConfig.SHARED_SECRETS_CONFIG_RELATIVE_PATH} へ移してください。");
            }

            return legacyValue;
        }

        /// <summary>
        ///     移行前の値を共通ファイルへ移す。共通ファイルにすでに値がある場合は上書きしない。
        /// </summary>
        /// <param name="key"> 共通ファイルの設定キー。 </param>
        /// <param name="legacyValue"> 移行前の設定に残っている値。 </param>
        /// <returns> 移行前の値を消してよい場合は true。 </returns>
        public static bool TryMigrate(string key, string legacyValue)
        {
            if (string.IsNullOrEmpty(legacyValue)) { return true; }

            string sharedValue = GetValue(key);
            if (string.IsNullOrEmpty(sharedValue))
            {
                SetValue(key, legacyValue);
                return true;
            }

            // 同じ値ならすでに移行済みとみなす。異なる値は、どちらが正しいか分からないため残す。
            return string.Equals(Normalize(sharedValue), Normalize(legacyValue), StringComparison.Ordinal);
        }

        private static readonly HashSet<string> _warnedLegacyKeys = new();
        private static JObject _cachedRoot;
        private static DateTime _cachedWriteTime;

        /// <summary>
        ///     共通ファイルを読み込む。更新日時が変わっていなければ前回の内容を使う。
        ///     ファイルが無い、または JSON として読めない場合は null を返す。
        /// </summary>
        /// <returns> ファイルのルートオブジェクト。 </returns>
        private static JObject LoadRoot()
        {
            string path = FullPath;
            if (!File.Exists(path)) { return null; }

            DateTime writeTime = File.GetLastWriteTimeUtc(path);
            if (_cachedRoot != null && writeTime == _cachedWriteTime) { return _cachedRoot; }

            try
            {
                _cachedRoot = JObject.Parse(File.ReadAllText(path));
                _cachedWriteTime = writeTime;
                return _cachedRoot;
            }
            catch (JsonReaderException e)
            {
                Debug.LogError($"[{nameof(SharedSecrets)}] 共通秘密設定ファイルを JSON として読めません: {path}\n{e.Message}");
                return null;
            }
        }

        /// <summary>
        ///     文字列を JSON オブジェクトとして読む。読めない場合は null を返す。
        /// </summary>
        /// <param name="value"> 読み込む文字列。 </param>
        /// <returns> 読み込んだオブジェクト。 </returns>
        private static JObject TryParseObject(string value)
        {
            if (!value.TrimStart().StartsWith("{", StringComparison.Ordinal)) { return null; }

            try
            {
                return JObject.Parse(value);
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

        /// <summary>
        ///     比較用に値を正規化する。JSON オブジェクトは1行の JSON にそろえる。
        /// </summary>
        /// <param name="value"> 正規化する値。 </param>
        /// <returns> 正規化した値。 </returns>
        private static string Normalize(string value)
        {
            JObject parsed = TryParseObject(value);
            return parsed != null ? parsed.ToString(Formatting.None) : value.Trim();
        }
    }
}
