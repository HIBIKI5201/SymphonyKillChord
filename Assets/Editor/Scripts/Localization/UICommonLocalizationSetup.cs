using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace KillChord.Editor.Localization
{
    /// <summary>
    ///     UI共通テキスト用Localizationアセットの初期構成を作成します。
    /// </summary>
    public static class UICommonLocalizationSetup
    {
        /// <summary> UI共通テキストのString Table Collection名です。 </summary>
        public const string UICommonTableName = "UICommon";

        private const string LOCALIZATION_ROOT = "Assets/Localization";
        private const string LOCALES_DIRECTORY = LOCALIZATION_ROOT + "/Locales";
        private const string TABLES_DIRECTORY = LOCALIZATION_ROOT + "/Tables";

        private static readonly string[] LocaleCodes = { "ja", "en" };
        private static readonly UIEntryDefinition[] UIEntryDefinitions =
        {
            new("ui.title.menu.data_reset", "データリセット", "Reset Data"),
            new("ui.title.menu.credit", "クレジット", "Credits"),
            new("ui.title.menu.data_reset_confirm_message", "データを消去しますか？", "Delete all data?"),
            new("ui.title.menu.data_reset_confirm", "消去する", "Delete"),
            new("ui.battle_preparation.title", "戦闘準備", "Battle Preparation"),
            new("ui.battle_preparation.start", "出撃", "Sortie"),
            new("ui.battle_preparation.skill_build", "改造", "Customize"),
            new("ui.setting.environment", "環境設定", "Environment Settings"),
            new("ui.setting.audio", "オーディオ設定", "Audio Settings"),
            new("ui.setting.return_to_title", "タイトルへ戻る", "Return to Title"),
            new("ui.setting.close", "とじる", "Close"),
            new("ui.setting.environment_save", "設定を保存", "Save Settings"),
            new("ui.setting.confirm_return_to_title", "タイトル画面に戻る", "Return to Title Screen"),
            new("ui.setting.cancel_return_to_title", "キャンセル", "Cancel"),
            new("ui.setting.language_japanese", "日本語", "日本語"),
            new("ui.setting.language_english", "English", "English"),
            new("ui.setting.screen_mode_fullscreen", "フルスクリーン", "Fullscreen"),
            new("ui.setting.screen_mode_windowed", "ウィンドウ", "Windowed"),
            new("ui.setting.vibration_strong", "強い", "Strong"),
            new("ui.setting.vibration_weak", "弱い", "Weak"),
            new("ui.setting.vibration_off", "オフ", "Off"),
            new("ui.skill_tree.reset", "振り直す", "Reset"),
            new("ui.skill_tree.close_preview", "動画を閉じる", "Close Video"),
            new("ui.skill_tree.reset_cancel", "キャンセル", "Cancel"),
            new("ui.skill_tree.reset_confirm", "リセット", "Reset"),
            new("ui.skill_tree.unlock_confirm", "解放する", "Unlock"),
            new("ui.skill_detail.unlock_cost_format", "解放する　必要ポイント：{0}", "Unlock (Required Points: {0})"),
            new("ui.skill_detail.unlocked", "解放済み", "Unlocked"),
            new("ui.skill_detail.preview", "スキル動画を表示する", "Show Skill Video"),
            new("ui.skill_detail.back", "戻る", "Back"),
            new("ui.skill_build_dialog.discard_and_close", "破棄して戻る", "Discard and Return"),
            new("ui.skill_build_dialog.save_and_close", "保存して戻る", "Save and Return"),
            new("ui.stage_select.sortie", "出撃", "Sortie"),
            new("ui.stage_select.first_reward", "初回報酬", "First Clear Reward"),
            new("ui.stage_select.success_reward", "成功報酬", "Clear Reward")
        };

        /// <summary>
        ///     UI共通テキスト用のロケールとString Table Collectionを作成または更新します。
        /// </summary>
        [MenuItem("KillChord/Localization/UI共通テキスト用テーブルをセットアップ")]
        public static void Setup()
        {
            EnsureDirectory(LOCALIZATION_ROOT);
            EnsureDirectory(LOCALES_DIRECTORY);
            EnsureDirectory(TABLES_DIRECTORY);

            IReadOnlyList<Locale> locales = EnsureLocales();
            int registeredEntryCount = EnsureStringTable(locales);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"[{nameof(UICommonLocalizationSetup)}] セットアップが完了しました。"
                + $" ロケール: {string.Join(", ", LocaleCodes)}、UI共通テキスト: {registeredEntryCount}件。");
        }

        /// <summary>
        ///     日本語と英語のロケールを作成または取得します。
        /// </summary>
        /// <returns> UI共通テキストのテーブルで使用するロケール一覧です。 </returns>
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
        ///     UI共通テキスト用String Table Collectionを作成または更新します。
        /// </summary>
        /// <param name="locales"> テーブルを作成するロケール一覧です。 </param>
        /// <returns> 登録したUI共通テキストの件数です。 </returns>
        private static int EnsureStringTable(IReadOnlyList<Locale> locales)
        {
            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(
                UICommonTableName);
            if (collection == null)
            {
                collection = LocalizationEditorSettings.CreateStringTableCollection(
                    UICommonTableName,
                    TABLES_DIRECTORY,
                    locales.ToList());
            }

            foreach (UIEntryDefinition definition in UIEntryDefinitions)
            {
                SharedTableData.SharedTableEntry sharedEntry = collection.SharedData.GetEntry(definition.Key)
                    ?? collection.SharedData.AddKey(definition.Key);
                foreach (Locale locale in locales)
                {
                    StringTable table = collection.StringTables.FirstOrDefault(
                        candidate => candidate.LocaleIdentifier.Code == locale.Identifier.Code);
                    if (table == null)
                    {
                        table = collection.AddNewTable(locale.Identifier) as StringTable;
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
            return UIEntryDefinitions.Length;
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
        ///     各ロケールのUI共通テキスト定義です。
        /// </summary>
        private readonly struct UIEntryDefinition
        {
            public UIEntryDefinition(string key, string japanese, string english)
            {
                Key = key;
                Japanese = japanese;
                English = english;
            }

            /// <summary> テーブルキーです。 </summary>
            public string Key { get; }
            /// <summary> 日本語テキストです。 </summary>
            public string Japanese { get; }
            /// <summary> 英語テキストです。 </summary>
            public string English { get; }
        }
    }
}
