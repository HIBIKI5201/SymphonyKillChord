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
        // 既存のテーブルキーと提供画像のファイル名を対応付けます。
        private static readonly (string Key, string FileStem)[] PopupImageDefinitions =
        {
            ("Attack", "Attack"),
            ("Blue", "BlueAttack"),
            ("Green", "GreenAttack"),
            ("JustAttack", "JustAttack"),
            ("KillChord", "KillChord"),
            ("Move", "Douge"),
            ("Orenge", "OrangeAttack"),
            ("Purple", "PurpleAttack"),
            ("Water", "WaterAttack"),
            ("Yellow", "YellowAttack")
        };

        private static readonly SubtitleDefinition[] SubtitleDefinitions =
        {
            new(
                "triad.tutorial.01",
                "戦場で会うのは久しぶりだなSymphony",
                "Been a while since we met on a battlefield, Symphony."),
            new(
                "triad.tutorial.02",
                "不要だと思うが、基本的なことについて説明させてもらうぞ",
                "You probably don't need this, but I'll walk you through the basics."),
            new(
                "triad.tutorial.03",
                "そして敵に関してだが...この資料を見てくれ。",
                "Now, about the enemy... take a look at this intel."),
            new(
                "triad.tutorial.04",
                "いいか？",
                "Got it?"),
            new(
                "triad.tutorial.05",
                "よし、次に行くぞ。",
                "Good. Moving on."),
            new(
                "triad.tutorial.06",
                "まず移動と回避についてだ。戦場の移動や敵の攻撃の回避に使うから覚えていてくれ。",
                "First up, movement and dodging. You'll need them to get around the battlefield and slip past enemy fire, so keep them in mind."),
            new(
                "triad.tutorial.07",
                "次に、お前が行う攻撃についてだ。攻撃によって特徴が異なるからしっかり把握しておいてくれ。",
                "Next, your attacks. Each one has its own quirks, so get a solid feel for how they work."),
            new(
                "triad.tutorial.08",
                "最後にお前の特殊能力、キルコードについてだ。フレーズを奏でることで発動する特殊な力、使いこなせるかはお前次第だ！",
                "Last, your special ability: the Kill Chord. It's a unique power you unleash by playing phrases. Whether you can master it is up to you!"),
            new(
                "triad.tutorial.09",
                "さて実践だ。教えたことを存分に活用して暴れるんだ",
                "Time for the real thing. Put everything I've taught you to use and cut loose."),
            new(
                "triad.tutorial.10",
                "腕が訛っていないようで安心したよ。",
                "Good to see you haven't gotten rusty."),
            new(
                "triad.tutorial.11",
                "大丈夫だ、この戦闘中は俺が支援しよう。",
                "Don't worry. I'll back you up for the rest of this fight.")
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

            foreach ((string key, string fileStem) in PopupImageDefinitions)
            {
                foreach (Locale locale in locales)
                {
                    string suffix = locale.Identifier.Code == "ja" ? "jp" : locale.Identifier.Code;
                    string imagePath = $"{POPUP_IMAGES_DIRECTORY}/{fileStem}_{suffix}.png";
                    Sprite image = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
                    if (image == null)
                    {
                        throw new InvalidOperationException($"チュートリアル画像が見つかりません: {imagePath}");
                    }

                    collection.AddAssetToTable(locale.Identifier, key, image);
                }
            }

            EditorUtility.SetDirty(collection);
            EditorUtility.SetDirty(collection.SharedData);
            LocalizationEditorSettings.EditorEvents.RaiseCollectionModified(null, collection);
            return PopupImageDefinitions.Length;
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
            /// <summary>
            ///     字幕の定義を生成する。
            /// </summary>
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
