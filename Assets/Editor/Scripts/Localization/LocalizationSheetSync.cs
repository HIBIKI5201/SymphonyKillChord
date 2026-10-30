using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

namespace KillChord.Editor.Localization
{
    /// <summary>
    ///     Google Drive の訳語の正本スプレッドシートを取得し、同じ名前のString Table Collectionへ反映します。
    ///     スプレッドシート1つが1テーブルに対応し、先頭のシートだけを読みます。
    /// </summary>
    public static class LocalizationSheetSync
    {
        /// <summary> APIキーと正本フォルダのIDが設定されているかどうかです。 </summary>
        public static bool IsConfigured =>
            !string.IsNullOrWhiteSpace(EditorPrefs.GetString(API_KEY_PREFS_KEY, string.Empty))
            && !string.IsNullOrWhiteSpace(EditorPrefs.GetString(FOLDER_ID_PREFS_KEY, string.Empty));

        /// <summary> APIキーを保存するEditorPrefsのキーです。シナリオのCSV取り込みと共用します。 </summary>
        public const string API_KEY_PREFS_KEY = "KillChord.CsvImporter.ApiKey";
        /// <summary> 正本フォルダのIDを保存するEditorPrefsのキーです。 </summary>
        public const string FOLDER_ID_PREFS_KEY = "KillChord.LocalizationSheetSync.FolderId";
        /// <summary> エディタ起動時に自動で取り込むかを保存するEditorPrefsのキーです。 </summary>
        public const string AUTO_SYNC_PREFS_KEY = "KillChord.LocalizationSheetSync.AutoSync";

        /// <summary>
        ///     正本フォルダのスプレッドシートを取得し、差分を確認してからテーブルへ反映します。
        /// </summary>
        /// <param name="confirm">
        ///     反映する前に差分を渡して呼びます。falseを返すと反映しません。nullなら確認せずに反映します。
        /// </param>
        /// <returns> 成功したかどうかと、結果の説明です。 </returns>
        public static async Task<(bool IsSucceeded, string Message)> SyncAsync(
            Func<IReadOnlyList<LocalizationSheetSyncPlan>, bool> confirm)
        {
            string apiKey = EditorPrefs.GetString(API_KEY_PREFS_KEY, string.Empty);
            string folderId = EditorPrefs.GetString(FOLDER_ID_PREFS_KEY, string.Empty);
            var client = new GoogleDriveSheetClient(apiKey);

            (List<GoogleDriveSheetClient.SheetFile> files, string listError) =
                await client.ListSpreadsheetsAsync(folderId);
            if (files == null)
            {
                return (false, listError);
            }

            if (files.Count == 0)
            {
                return (false, "正本フォルダにスプレッドシートがありません。共有設定が「リンクを知っている全員」か確認してください。");
            }

            // 全シートを読み取ってから反映することで、一部のテーブルだけ更新される状態を避ける。
            var plans = new List<LocalizationSheetSyncPlan>();
            var skipped = new List<string>();
            foreach (GoogleDriveSheetClient.SheetFile file in files)
            {
                StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(file.Name);
                if (collection == null)
                {
                    skipped.Add(file.Name);
                    continue;
                }

                (string csv, string exportError) = await client.ExportCsvAsync(file.Id);
                if (csv == null)
                {
                    return (false, $"「{file.Name}」を取得できませんでした。\n{exportError}");
                }

                if (!LocalizationSheetCsv.TryParse(csv, out List<LocalizationSheetRow> rows, out string parseError))
                {
                    return (false, $"「{file.Name}」を読み取れませんでした。{parseError}");
                }

                plans.Add(LocalizationTableSheetBridge.BuildPlan(collection, rows));
            }

            var message = new StringBuilder();
            foreach (LocalizationSheetSyncPlan plan in plans)
            {
                message.AppendLine(plan.Summarize());
            }

            if (skipped.Count > 0)
            {
                message.AppendLine($"同じ名前のテーブルが無いため読まなかったシート: {string.Join(", ", skipped)}");
            }

            if (!plans.Any(plan => plan.HasChanges))
            {
                message.Insert(0, "テーブルは正本と同じです。\n");
                return (true, message.ToString().TrimEnd());
            }

            if (confirm != null && !confirm(plans))
            {
                return (false, "反映をキャンセルしました。");
            }

            foreach (LocalizationSheetSyncPlan plan in plans)
            {
                LocalizationTableSheetBridge.Apply(
                    LocalizationEditorSettings.GetStringTableCollection(plan.CollectionName),
                    plan);
            }

            AssetDatabase.SaveAssets();
            message.Insert(0, "正本の内容をテーブルへ反映しました。\n");
            Debug.Log($"[{nameof(LocalizationSheetSync)}] {message.ToString().TrimEnd()}");
            return (true, message.ToString().TrimEnd());
        }

        /// <summary>
        ///     ローカライズの全String Table Collectionを、正本と同じ形のCSVへ書き出します。
        ///     ファイル名はテーブル名です。スプレッドシートを作り直すときに使います。
        /// </summary>
        /// <param name="directoryPath"> 書き出し先のフォルダです。 </param>
        /// <returns> 書き出したファイルの数です。 </returns>
        public static int ExportAll(string directoryPath)
        {
            int count = 0;
            foreach (StringTableCollection collection in LocalizationEditorSettings.GetStringTableCollections())
            {
                string csv = LocalizationSheetCsv.Write(LocalizationTableSheetBridge.Export(collection));
                string path = Path.Combine(directoryPath, $"{collection.TableCollectionName}.csv");
                File.WriteAllText(path, csv, new UTF8Encoding(true));
                count++;
            }

            return count;
        }
    }
}
