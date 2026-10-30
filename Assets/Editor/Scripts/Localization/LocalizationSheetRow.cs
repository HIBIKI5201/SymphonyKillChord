namespace KillChord.Editor.Localization
{
    /// <summary>
    ///     訳語の正本スプレッドシートの1行（キー / 日本語 / 英語 / 備考）です。
    /// </summary>
    public readonly struct LocalizationSheetRow
    {
        /// <summary>
        ///     スプレッドシートの1行を生成します。セル内の改行はLFにそろえます。
        /// </summary>
        /// <param name="key"> String Tableのキーです。 </param>
        /// <param name="japanese"> 日本語の文言です。 </param>
        /// <param name="english"> 英語の文言です。未翻訳なら空です。 </param>
        /// <param name="note"> 翻訳者向けの備考です。 </param>
        public LocalizationSheetRow(string key, string japanese, string english, string note)
        {
            Key = key ?? string.Empty;
            Japanese = NormalizeNewLine(japanese);
            English = NormalizeNewLine(english);
            Note = NormalizeNewLine(note);
        }

        /// <summary> String Tableのキーです。 </summary>
        public string Key { get; }
        /// <summary> 日本語の文言です。 </summary>
        public string Japanese { get; }
        /// <summary> 英語の文言です。未翻訳なら空です。 </summary>
        public string English { get; }
        /// <summary> 翻訳者向けの備考です。 </summary>
        public string Note { get; }

        /// <summary>
        ///     改行をLFにそろえます。
        /// </summary>
        private static string NormalizeNewLine(string text)
        {
            return (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
        }
    }
}
