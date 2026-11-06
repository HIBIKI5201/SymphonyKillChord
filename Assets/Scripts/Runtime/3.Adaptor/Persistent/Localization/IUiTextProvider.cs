namespace KillChord.Runtime.Adaptor.Persistent.Localization
{
    /// <summary>
    ///     画面に出す決まった文言を、選択中の言語で取得します。
    ///     Adaptor が組み立てる表示文字列を、Unity の Localization に直接頼らずに日英対応するために使います。
    /// </summary>
    public interface IUiTextProvider
    {
        /// <summary>
        ///     UICommon テーブルのキーに対応する、選択中の言語の文を取得します。
        /// </summary>
        /// <param name="key"> UICommon テーブルのキーです。 </param>
        /// <param name="fallback"> 取得できないときに返す日本語の文です。 </param>
        /// <returns> 選択中の言語の文です。取得できないときは <paramref name="fallback"/> を返します。 </returns>
        string GetText(string key, string fallback);
    }
}
