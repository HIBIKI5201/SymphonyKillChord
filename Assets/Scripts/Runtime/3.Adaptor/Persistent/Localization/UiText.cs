using System;
using System.Globalization;

namespace KillChord.Runtime.Adaptor.Persistent.Localization
{
    /// <summary>
    ///     <see cref="IUiTextProvider"/> から文言を取り出し、書式の穴埋めまで行います。
    ///     取得元が無いときや訳文の書式が壊れているときは、日本語の既定文を使います。
    /// </summary>
    public static class UiText
    {
        /// <summary>
        ///     キーに対応する選択中の言語の文を取得します。
        /// </summary>
        /// <param name="provider"> 文言の取得元です。nullなら既定文を返します。 </param>
        /// <param name="key"> UICommon テーブルのキーです。 </param>
        /// <param name="fallback"> 取得できないときに返す日本語の文です。 </param>
        /// <returns> 選択中の言語の文です。 </returns>
        public static string Get(IUiTextProvider provider, string key, string fallback)
        {
            if (provider == null)
            {
                return fallback ?? string.Empty;
            }

            string text = provider.GetText(key, fallback);
            return string.IsNullOrEmpty(text) ? fallback ?? string.Empty : text;
        }

        /// <summary>
        ///     キーに対応する選択中の言語の書式へ、値を埋め込みます。
        /// </summary>
        /// <param name="provider"> 文言の取得元です。nullなら既定の書式を使います。 </param>
        /// <param name="key"> UICommon テーブルのキーです。 </param>
        /// <param name="fallbackFormat"> 取得できないときに使う日本語の書式です。 </param>
        /// <param name="arguments"> 書式の {0}, {1}… に入れる値です。 </param>
        /// <returns> 値を埋め込んだ文です。 </returns>
        public static string Format(
            IUiTextProvider provider,
            string key,
            string fallbackFormat,
            params object[] arguments)
        {
            string format = Get(provider, key, fallbackFormat);
            try
            {
                return string.Format(CultureInfo.InvariantCulture, format, arguments);
            }
            catch (FormatException)
            {
                // 訳文の波括弧が壊れていても画面を止めず、日本語の書式で表示する。
                return string.Format(CultureInfo.InvariantCulture, fallbackFormat, arguments);
            }
        }
    }
}
