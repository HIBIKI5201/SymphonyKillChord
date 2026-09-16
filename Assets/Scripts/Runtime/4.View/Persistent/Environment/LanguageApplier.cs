using KillChord.Runtime.Adaptor.Persistent.Environment;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace KillChord.Runtime.View.Persistent.Environment
{
    /// <summary>
    ///     Unity Localizationへ表示言語を適用するクラス。
    /// </summary>
    public sealed class LanguageApplier : ILanguageApplier
    {
        /// <summary>
        ///     指定した表示言語に対応するLocaleへ切り替える。
        /// </summary>
        public void Apply(string localeCode)
        {
            Locale locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
            if (locale == null)
            {
                Debug.LogError(
                    $"[{nameof(LanguageApplier)}] 表示言語に対応するLocaleが見つかりませんでした。Locale={localeCode}");
                return;
            }

            LocalizationSettings.SelectedLocale = locale;
        }
    }
}
