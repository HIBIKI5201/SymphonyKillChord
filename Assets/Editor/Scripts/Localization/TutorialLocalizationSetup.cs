using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace KillChord.Editor.Localization
{
    /// <summary>
    ///     チュートリアル用Localizationアセットの初期構成を作成します。
    /// </summary>
    public static class TutorialLocalizationSetup
    {
        /// <summary> チュートリアル字幕のString Table Collection名です。 </summary>
        public const string TutorialSubtitlesTableName = "TutorialSubtitles";

        /// <summary> チュートリアルポップアップ画像のAsset Table Collection名です。 </summary>
        public const string TutorialPopupImagesTableName = "TutorialPopupImages";

        private const string LOCALIZATION_ROOT = "Assets/Localization";
        private const string LOCALES_DIRECTORY = LOCALIZATION_ROOT + "/Locales";
        private const string TABLES_DIRECTORY = LOCALIZATION_ROOT + "/Tables";
        private const string SETTINGS_PATH = LOCALIZATION_ROOT + "/LocalizationSettings.asset";
        private const string POPUP_IMAGES_DIRECTORY = "Assets/Arts/Images/Sprites/TutorialPopupImages/New";

        private static readonly string[] LocaleCodes = { "ja", "en" };
        private static readonly SubtitleDefinition[] SubtitleDefinitions =
        {
            new(
                "triad.tutorial.06",
                "次に、移動と回避についてだ。戦場の移動や敵の攻撃の回避に使うから、よく覚えておいてくれ。",
                "Next, let's go over movement and dodging. You'll use them to navigate the battlefield and evade enemy attacks, so make sure you remember them."),
            new(
                "triad.tutorial.07",
                "次に、お前が行う攻撃についてだ。攻撃によって特徴が異なるから、しっかり把握しておいてくれ。",
                "Next, let's go over your attacks. Each attack has different characteristics, so make sure you understand how they work."),
            new(
                "triad.tutorial.09",
                "さて、実戦だ。教えたことを存分に活用して暴れるんだ。",
                "Now it's time for real combat. Put everything I've taught you to use and go wild.")
        };

        /// <summary>
        ///     チュートリアル用のLocalization Settings、ロケール、テーブルを作成または更新します。
        /// </summary>
        [MenuItem("KillChord/Localization/チュートリアル用テーブルをセットアップ")]
        public static void Setup()
        {
            EnsureDirectory(LOCALIZATION_ROOT);
            EnsureDirectory(LOCALES_DIRECTORY);
            EnsureDirectory(TABLES_DIRECTORY);

            LocalizationSettings settings = EnsureLocalizationSettings();
            IReadOnlyList<Locale> locales = EnsureLocales();
            EnsureDefaultLocale(settings, locales);
            int registeredSubtitleCount = EnsureStringTable(locales);
            int registeredImageCount = EnsurePopupImageTable(locales);

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"[{nameof(TutorialLocalizationSetup)}] セットアップが完了しました。"
                + $" ロケール: {string.Join(", ", LocaleCodes)}、字幕: {registeredSubtitleCount}件、"
                + $"ポップアップ画像: {registeredImageCount}件。");
        }

        /// <summary>
        ///     Localization Settingsアセットを作成または取得します。
        /// </summary>
        /// <returns> 有効なLocalization Settingsです。 </returns>
        private static LocalizationSettings EnsureLocalizationSettings()
        {
            LocalizationSettings settings = LocalizationEditorSettings.ActiveLocalizationSettings;
            if (settings != null)
            {
                return settings;
            }

            string existingGuid = AssetDatabase.FindAssets(
                    "t:LocalizationSettings",
                    new[] { "Assets" })
                .FirstOrDefault();
            if (!string.IsNullOrEmpty(existingGuid))
            {
                string existingPath = AssetDatabase.GUIDToAssetPath(existingGuid);
                settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(existingPath);
            }

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                AssetDatabase.CreateAsset(settings, SETTINGS_PATH);
            }

            LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            return settings;
        }

        /// <summary>
        ///     日本語と英語のロケールを作成または取得します。
        /// </summary>
        /// <returns> チュートリアルのテーブルで使用するロケール一覧です。 </returns>
        private static IReadOnlyList<Locale> EnsureLocales()
        {
            var locales = new List<Locale>(LocaleCodes.Length);
            foreach (string localeCode in LocaleCodes)
            {
                Locale locale = LocalizationEditorSettings.GetLocale(localeCode);
                if (locale == null)
                {
                    locale = Locale.CreateLocale(new LocaleIdentifier(localeCode));
                    string localePath = $"{LOCALES_DIRECTORY}/{locale.LocaleName}.asset";
                    AssetDatabase.CreateAsset(locale, AssetDatabase.GenerateUniqueAssetPath(localePath));
                    LocalizationEditorSettings.AddLocale(locale);
                }

                locales.Add(locale);
            }

            return locales;
        }

        /// <summary>
        ///     日本語を初期ロケールとして設定します。
        /// </summary>
        /// <param name="settings"> 設定対象のLocalization Settingsです。 </param>
        /// <param name="locales"> 利用可能なロケール一覧です。 </param>
        private static void EnsureDefaultLocale(
            LocalizationSettings settings,
            IReadOnlyList<Locale> locales)
        {
            Locale japaneseLocale = locales.First(locale => locale.Identifier.Code == "ja");
            List<IStartupLocaleSelector> selectors = settings.GetStartupLocaleSelectors();
            SpecificLocaleSelector specificSelector = selectors
                .OfType<SpecificLocaleSelector>()
                .FirstOrDefault();
            if (specificSelector == null)
            {
                specificSelector = new SpecificLocaleSelector();
                selectors.Insert(0, specificSelector);
            }

            specificSelector.LocaleId = japaneseLocale.Identifier;
        }

        /// <summary>
        ///     チュートリアル字幕用String Table Collectionを作成または更新します。
        /// </summary>
        /// <param name="locales"> テーブルを作成するロケール一覧です。 </param>
        /// <returns> 登録した字幕の件数です。 </returns>
        private static int EnsureStringTable(IReadOnlyList<Locale> locales)
        {
            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(
                TutorialSubtitlesTableName);
            if (collection == null)
            {
                collection = LocalizationEditorSettings.CreateStringTableCollection(
                    TutorialSubtitlesTableName,
                    TABLES_DIRECTORY,
                    locales.ToList());
            }

            foreach (SubtitleDefinition definition in SubtitleDefinitions)
            {
                SharedTableData.SharedTableEntry sharedEntry = collection.SharedData.GetEntry(definition.Key)
                    ?? collection.SharedData.AddKey(definition.Key);
                foreach (Locale locale in locales)
                {
                    StringTable table = collection.StringTables.FirstOrDefault(
                        candidate => candidate.LocaleIdentifier.Code == locale.Identifier.Code);
                    if (table == null)
                    {
                        throw new InvalidOperationException(
                            $"字幕テーブルに{locale.Identifier.Code}ロケールがありません。");
                    }

                    string localizedText = locale.Identifier.Code == "ja"
                        ? definition.Japanese
                        : definition.English;
                    StringTableEntry entry = table.GetEntry(sharedEntry.Id);
                    if (entry == null || string.IsNullOrWhiteSpace(entry.Value))
                    {
                        entry ??= table.AddEntry(sharedEntry.Id, localizedText);
                        entry.Value = localizedText;
                        EditorUtility.SetDirty(table);
                    }
                }
            }

            EditorUtility.SetDirty(collection);
            EditorUtility.SetDirty(collection.SharedData);
            LocalizationEditorSettings.EditorEvents.RaiseCollectionModified(null, collection);
            return SubtitleDefinitions.Length;
        }

        /// <summary>
        ///     チュートリアルポップアップ画像用Asset Table Collectionを作成して現行画像を登録します。
        /// </summary>
        /// <param name="locales"> テーブルを作成するロケール一覧です。 </param>
        /// <returns> 登録した画像の件数です。 </returns>
        private static int EnsurePopupImageTable(IReadOnlyList<Locale> locales)
        {
            AssetTableCollection collection = LocalizationEditorSettings.GetAssetTableCollection(
                TutorialPopupImagesTableName);
            if (collection == null)
            {
                collection = LocalizationEditorSettings.CreateAssetTableCollection(
                    TutorialPopupImagesTableName,
                    TABLES_DIRECTORY,
                    locales.ToList());
            }

            string[] imageGuids = AssetDatabase.FindAssets("t:Sprite", new[] { POPUP_IMAGES_DIRECTORY });
            foreach (string imageGuid in imageGuids)
            {
                string imagePath = AssetDatabase.GUIDToAssetPath(imageGuid);
                Sprite image = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
                if (image == null)
                {
                    continue;
                }

                foreach (Locale locale in locales)
                {
                    collection.AddAssetToTable(locale.Identifier, image.name, image);
                }
            }

            EditorUtility.SetDirty(collection);
            EditorUtility.SetDirty(collection.SharedData);
            LocalizationEditorSettings.EditorEvents.RaiseCollectionModified(null, collection);
            return imageGuids.Length;
        }

        /// <summary>
        ///     指定したAssets配下のディレクトリを再帰的に作成します。
        /// </summary>
        /// <param name="directoryPath"> 作成するディレクトリのパスです。 </param>
        private static void EnsureDirectory(string directoryPath)
        {
            if (AssetDatabase.IsValidFolder(directoryPath))
            {
                return;
            }

            string parentDirectory = System.IO.Path.GetDirectoryName(directoryPath)?.Replace('\\', '/');
            string directoryName = System.IO.Path.GetFileName(directoryPath);
            if (!string.IsNullOrEmpty(parentDirectory))
            {
                EnsureDirectory(parentDirectory);
                AssetDatabase.CreateFolder(parentDirectory, directoryName);
            }
        }

        /// <summary>
        ///     各ロケールの字幕定義です。
        /// </summary>
        private readonly struct SubtitleDefinition
        {
            public SubtitleDefinition(string key, string japanese, string english)
            {
                Key = key;
                Japanese = japanese;
                English = english;
            }

            /// <summary> テーブルキーです。 </summary>
            public string Key { get; }
            /// <summary> 日本語字幕です。 </summary>
            public string Japanese { get; }
            /// <summary> 英語字幕です。 </summary>
            public string English { get; }
        }
    }
}
