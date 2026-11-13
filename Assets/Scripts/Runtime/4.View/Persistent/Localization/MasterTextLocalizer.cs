using System;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace KillChord.Runtime.View.Persistent.Localization
{
    /// <summary>
    ///     マスターデータの日本語の文を、選択中の言語の文に置き換えます。
    ///     訳文は MasterText テーブルから、日本語の文で作ったキー（<see cref="MasterTextKey"/>）で引きます。
    ///     日本語が選ばれているとき、訳文が無いとき、Localizationの初期化前は、元の日本語を返します。
    /// </summary>
    public static class MasterTextLocalizer
    {
        /// <summary>
        ///     マスターデータの日本語の文を、選択中の言語の文に置き換えます。
        /// </summary>
        /// <param name="source"> マスターデータの日本語の文です。 </param>
        /// <returns> 選択中の言語の文です。置き換えられないときは <paramref name="source"/> をそのまま返します。 </returns>
        public static string Localize(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return source ?? string.Empty;
            }

            // 初期化前に SelectedLocale を参照すると同期で初期化が走るため、終わるまでは元の文を使う。
            if (!LocalizationSettings.InitializationOperation.IsDone)
            {
                return source;
            }

            Locale locale = LocalizationSettings.SelectedLocale;
            if (locale == null || locale.Identifier.Code == SOURCE_LOCALE_CODE)
            {
                return source;
            }

            StringTable table = LocalizationSettings.StringDatabase.GetTable(MasterTextKey.TABLE_NAME, locale);
            StringTableEntry entry = table != null ? table.GetEntry(MasterTextKey.Create(source)) : null;
            string localized = entry?.Value;
            return string.IsNullOrEmpty(localized) ? source : localized;
        }

        /// <summary>
        ///     マスターデータの日本語の文の一覧を、選択中の言語の文の一覧に置き換えます。
        /// </summary>
        /// <param name="sources"> マスターデータの日本語の文の一覧です。 </param>
        /// <returns> 選択中の言語の文の一覧です。nullのときは空の一覧を返します。 </returns>
        public static IReadOnlyList<string> LocalizeAll(IReadOnlyList<string> sources)
        {
            if (sources == null)
            {
                return Array.Empty<string>();
            }

            var localized = new string[sources.Count];
            for (int i = 0; i < sources.Count; i++)
            {
                localized[i] = Localize(sources[i]);
            }

            return localized;
        }

        private const string SOURCE_LOCALE_CODE = "ja";
    }
}
