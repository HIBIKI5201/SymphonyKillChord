using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;

namespace KillChord.Editor.Localization
{
    /// <summary>
    ///     スプレッドシートの行とString Table Collectionの間で、差分の計算・反映・書き出しを行います。
    ///     英語が空の行は、英語のテーブルにも日本語を入れます（ローカライズ仕様の「訳語の決め方」）。
    /// </summary>
    public static class LocalizationTableSheetBridge
    {
        /// <summary> 日本語のロケールコードです。 </summary>
        public const string JAPANESE_LOCALE_CODE = "ja";
        /// <summary> 英語のロケールコードです。 </summary>
        public const string ENGLISH_LOCALE_CODE = "en";

        /// <summary>
        ///     スプレッドシートの行をテーブルへ反映したときの差分を計算します。テーブルは変更しません。
        /// </summary>
        /// <param name="collection"> 反映先のString Table Collectionです。 </param>
        /// <param name="rows"> スプレッドシートの行です。 </param>
        /// <returns> 追加・変更する行と、シートに無いキーです。 </returns>
        public static LocalizationSheetSyncPlan BuildPlan(
            StringTableCollection collection,
            IReadOnlyList<LocalizationSheetRow> rows)
        {
            StringTable japaneseTable = GetTable(collection, JAPANESE_LOCALE_CODE);
            StringTable englishTable = GetTable(collection, ENGLISH_LOCALE_CODE);

            var addedRows = new List<LocalizationSheetRow>();
            var changedRows = new List<LocalizationSheetRow>();
            var sheetKeys = new HashSet<string>(StringComparer.Ordinal);
            int untranslatedCount = 0;

            foreach (LocalizationSheetRow row in rows)
            {
                sheetKeys.Add(row.Key);
                if (string.IsNullOrEmpty(row.English))
                {
                    untranslatedCount++;
                }

                SharedTableData.SharedTableEntry sharedEntry = collection.SharedData.GetEntry(row.Key);
                if (sharedEntry == null)
                {
                    addedRows.Add(row);
                    continue;
                }

                // 日本語・英語のどちらかが今のテーブルと違えば変更として扱う。
                string currentJapanese = GetValue(japaneseTable, sharedEntry.Id);
                string currentEnglish = GetValue(englishTable, sharedEntry.Id);
                if (currentJapanese != row.Japanese || currentEnglish != ResolveEnglish(row))
                {
                    changedRows.Add(row);
                }
            }

            List<string> keysMissingInSheet = collection.SharedData.Entries
                .Select(entry => entry.Key)
                .Where(key => !sheetKeys.Contains(key))
                .ToList();

            return new LocalizationSheetSyncPlan(
                collection.TableCollectionName,
                addedRows,
                changedRows,
                keysMissingInSheet,
                untranslatedCount);
        }

        /// <summary>
        ///     差分の行をテーブルへ書き込みます。シートに無いキーは消しません。
        /// </summary>
        /// <param name="collection"> 反映先のString Table Collectionです。 </param>
        /// <param name="plan"> <see cref="BuildPlan"/> で計算した差分です。 </param>
        public static void Apply(StringTableCollection collection, LocalizationSheetSyncPlan plan)
        {
            if (!plan.HasChanges)
            {
                return;
            }

            StringTable japaneseTable = GetTable(collection, JAPANESE_LOCALE_CODE);
            StringTable englishTable = GetTable(collection, ENGLISH_LOCALE_CODE);

            foreach (LocalizationSheetRow row in plan.AddedRows.Concat(plan.ChangedRows))
            {
                SharedTableData.SharedTableEntry sharedEntry = collection.SharedData.GetEntry(row.Key)
                    ?? collection.SharedData.AddKey(row.Key);
                SetValue(japaneseTable, sharedEntry.Id, row.Japanese);
                SetValue(englishTable, sharedEntry.Id, ResolveEnglish(row));
            }

            // 変更を保存対象にし、エディタへ通知する。
            EditorUtility.SetDirty(japaneseTable);
            EditorUtility.SetDirty(englishTable);
            EditorUtility.SetDirty(collection);
            EditorUtility.SetDirty(collection.SharedData);
            LocalizationEditorSettings.EditorEvents.RaiseCollectionModified(null, collection);
        }

        /// <summary>
        ///     テーブルの内容を、スプレッドシートの行の形で取り出します。キーの登録順に並べます。
        ///     英語が日本語と同じ行は、未翻訳とみなして英語を空にします。
        /// </summary>
        /// <param name="collection"> 書き出すString Table Collectionです。 </param>
        /// <returns> スプレッドシートの行です。備考は空です。 </returns>
        public static List<LocalizationSheetRow> Export(StringTableCollection collection)
        {
            StringTable japaneseTable = GetTable(collection, JAPANESE_LOCALE_CODE);
            StringTable englishTable = GetTable(collection, ENGLISH_LOCALE_CODE);

            var rows = new List<LocalizationSheetRow>();
            foreach (SharedTableData.SharedTableEntry sharedEntry in collection.SharedData.Entries)
            {
                string japanese = GetValue(japaneseTable, sharedEntry.Id);
                string english = GetValue(englishTable, sharedEntry.Id);
                if (english == japanese && ContainsJapanese(japanese))
                {
                    english = string.Empty;
                }

                rows.Add(new LocalizationSheetRow(sharedEntry.Key, japanese, english, string.Empty));
            }

            return rows;
        }

        /// <summary>
        ///     英語のテーブルへ入れる文言を決めます。英語が空なら日本語を使います。
        /// </summary>
        private static string ResolveEnglish(LocalizationSheetRow row)
        {
            return string.IsNullOrEmpty(row.English) ? row.Japanese : row.English;
        }

        /// <summary>
        ///     指定したロケールのテーブルを取得します。無ければ例外を投げます。
        /// </summary>
        private static StringTable GetTable(StringTableCollection collection, string localeCode)
        {
            StringTable table = collection.StringTables.FirstOrDefault(
                candidate => candidate.LocaleIdentifier.Code == localeCode);
            if (table == null)
            {
                throw new InvalidOperationException(
                    $"{collection.TableCollectionName}テーブルに{localeCode}ロケールがありません。");
            }

            return table;
        }

        /// <summary>
        ///     テーブルの文言を取得します。エントリが無ければ空文字を返します。改行はLFにそろえます。
        /// </summary>
        private static string GetValue(StringTable table, long id)
        {
            StringTableEntry entry = table.GetEntry(id);
            string value = entry?.Value ?? string.Empty;
            return value.Replace("\r\n", "\n");
        }

        /// <summary>
        ///     テーブルの文言を書き込みます。エントリが無ければ作ります。
        /// </summary>
        private static void SetValue(StringTable table, long id, string value)
        {
            StringTableEntry entry = table.GetEntry(id) ?? table.AddEntry(id, value);
            entry.Value = value;
        }

        /// <summary>
        ///     ひらがな・カタカナ・漢字を含むかどうかを返します。
        /// </summary>
        private static bool ContainsJapanese(string text)
        {
            foreach (char c in text)
            {
                if ((c >= '぀' && c <= 'ヿ') || (c >= '一' && c <= '鿿'))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
