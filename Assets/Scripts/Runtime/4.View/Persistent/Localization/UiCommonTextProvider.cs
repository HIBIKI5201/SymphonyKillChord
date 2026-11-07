using KillChord.Runtime.Adaptor.Persistent.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace KillChord.Runtime.View.Persistent.Localization
{
    /// <summary>
    ///     UICommon テーブルから、選択中の言語の文を取得します。
    ///     Localizationの初期化前、テーブルやキーが無いとき、訳文が空のときは、渡された日本語の文を返します。
    /// </summary>
    public sealed class UiCommonTextProvider : IUiTextProvider
    {
        /// <summary>
        ///     UICommon テーブルのキーに対応する、選択中の言語の文を取得します。
        /// </summary>
        /// <param name="key"> UICommon テーブルのキーです。 </param>
        /// <param name="fallback"> 取得できないときに返す日本語の文です。 </param>
        /// <returns> 選択中の言語の文です。 </returns>
        public string GetText(string key, string fallback)
        {
            if (string.IsNullOrEmpty(key))
            {
                return fallback ?? string.Empty;
            }

            // 初期化前に SelectedLocale を参照すると同期で初期化が走るため、終わるまでは既定文を使う。
            if (!LocalizationSettings.InitializationOperation.IsDone)
            {
                return fallback ?? string.Empty;
            }

            Locale locale = LocalizationSettings.SelectedLocale;
            if (locale == null)
            {
                return fallback ?? string.Empty;
            }

            StringTable table = LocalizationSettings.StringDatabase.GetTable(TABLE_NAME, locale);
            StringTableEntry entry = table != null ? table.GetEntry(key) : null;
            string localized = entry?.Value;
            return string.IsNullOrEmpty(localized) ? fallback ?? string.Empty : localized;
        }

        private const string TABLE_NAME = "UICommon";
    }
}
