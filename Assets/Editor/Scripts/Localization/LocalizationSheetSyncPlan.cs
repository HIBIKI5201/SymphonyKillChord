using System.Collections.Generic;

namespace KillChord.Editor.Localization
{
    /// <summary>
    ///     1つのString Table Collectionへ、スプレッドシートの内容を反映するときの差分です。
    /// </summary>
    public sealed class LocalizationSheetSyncPlan
    {
        /// <summary>
        ///     差分を生成します。
        /// </summary>
        /// <param name="collectionName"> 反映先のString Table Collection名です。 </param>
        /// <param name="addedRows"> テーブルに無いキーの行です。 </param>
        /// <param name="changedRows"> 日本語か英語の文言が変わる行です。 </param>
        /// <param name="keysMissingInSheet"> テーブルにあってスプレッドシートに無いキーです。 </param>
        /// <param name="untranslatedCount"> 英語が空の行の数です。 </param>
        public LocalizationSheetSyncPlan(
            string collectionName,
            IReadOnlyList<LocalizationSheetRow> addedRows,
            IReadOnlyList<LocalizationSheetRow> changedRows,
            IReadOnlyList<string> keysMissingInSheet,
            int untranslatedCount)
        {
            CollectionName = collectionName;
            AddedRows = addedRows;
            ChangedRows = changedRows;
            KeysMissingInSheet = keysMissingInSheet;
            UntranslatedCount = untranslatedCount;
        }

        /// <summary> 反映先のString Table Collection名です。 </summary>
        public string CollectionName { get; }
        /// <summary> テーブルに無いキーの行です。 </summary>
        public IReadOnlyList<LocalizationSheetRow> AddedRows { get; }
        /// <summary> 日本語か英語の文言が変わる行です。 </summary>
        public IReadOnlyList<LocalizationSheetRow> ChangedRows { get; }
        /// <summary> テーブルにあってスプレッドシートに無いキーです。テーブルからは消しません。 </summary>
        public IReadOnlyList<string> KeysMissingInSheet { get; }
        /// <summary> 英語が空の行の数です。英語のテーブルには日本語を入れます。 </summary>
        public int UntranslatedCount { get; }
        /// <summary> テーブルへ書き込む行があるかどうかです。 </summary>
        public bool HasChanges => AddedRows.Count > 0 || ChangedRows.Count > 0;

        /// <summary>
        ///     差分の要約を1行の文字列で返します。
        /// </summary>
        /// <returns> 追加・変更・シートに無いキー・未翻訳の件数です。 </returns>
        public string Summarize()
        {
            return $"{CollectionName}: 追加 {AddedRows.Count} / 変更 {ChangedRows.Count}"
                + $" / シートに無いキー {KeysMissingInSheet.Count} / 英語が空 {UntranslatedCount}";
        }
    }
}
