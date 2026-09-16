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
            new("ui.title.menu.data_reset_confirm_message", "データを消去しますか？", "Erase all data?"),
            new("ui.title.menu.data_reset_confirm", "消去する", "Erase"),
            new("ui.battle_preparation.title", "戦闘準備", "Battle Preparation"),
            new("ui.battle_preparation.start", "出撃", "Sortie"),
            new("ui.battle_preparation.skill_build", "改造", "Customize"),
            new("ui.setting.environment", "環境設定", "Display Settings"),
            new("ui.setting.audio", "オーディオ設定", "Audio Settings"),
            new("ui.setting.return_to_title", "タイトルへ戻る", "Return to Title"),
            new("ui.setting.close", "とじる", "Close"),
            new("ui.setting.environment_save", "設定を保存", "Save Settings"),
            new("ui.setting.confirm_return_to_title", "タイトル画面に戻る", "Return to Title"),
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
            new("ui.skill_detail.unlock_cost_format", "スキルを解放する　必要ポイント：{0}", "Unlock Skill (Required Points: {0})"),
            new("ui.skill_detail.unlocked", "解放済み", "Unlocked"),
            new("ui.skill_detail.preview", "スキル動画を表示する", "Show Skill Video"),
            new("ui.skill_detail.back", "戻る", "Back"),
            new("ui.skill_build_dialog.discard_and_close", "破棄して戻る", "Discard & Back"),
            new("ui.skill_build_dialog.save_and_close", "保存して戻る", "Save & Back"),
            new("ui.stage_select.sortie", "出撃", "Sortie"),
            new("ui.stage_select.first_reward", "初回報酬", "First Clear Reward"),
            new("ui.stage_select.success_reward", "成功報酬", "Success Reward"),
            new("ui.skill_build.points_heading", "改造P：", "Mod Points:"),
            new("ui.skill_build.details", "詳細", "Details"),
            new("ui.skill_build.empty_selection", "スキルを選択してください", "Please select a skill"),
            new("ui.skill.type", "スキルタイプ", "Skill Type"),
            new("ui.skill.effect", "スキル効果", "Skill Effect"),
            new("ui.skill.level", "レベル", "Level"),
            new("ui.points.mod", "改造P", "Mod Points"),
            new("ui.skill.formation", "編成", "Formation"),
            new("ui.skill_build_dialog.message", "装備中スキルの変更が保存されていません。保存しますか？", "Your equipped skill changes haven't been saved. Save them?"),
            new("ui.skill.locked", "未開放", "Locked"),
            new("ui.player_status.title", "ステータス", "Status"),
            new("ui.player_status.health", "HP", "HP"),
            new("ui.player_status.attack", "攻撃力", "Attack"),
            new("ui.player_status.critical_chance", "会心率", "Crit Rate"),
            new("ui.player_status.critical_damage", "会心ダメージ", "Crit Damage"),
            new("ui.player_status.range", "射程範囲", "Range"),
            new("ui.skill_detail.info", "情報", "Info"),
            new("ui.skill_detail.activation_combo", "発動コンボ", "Activation Combo"),
            new("ui.skill_detail.genre", "スキルジャンル：", "Skill Genre:"),
            new("ui.skill_detail.effect", " スキル効果", " Skill Effect"),
            new("ui.skill_tree.title", "研究", "Research"),
            new("ui.skill_tree.unlock_title", "解放しますか？", "Unlock this skill?"),
            new("ui.skill_tree.stats_to_increase", "上昇するパラメータ", "Stats to Increase"),
            new("ui.skill_tree.skills_to_unlock", "解放されるスキル", "Skills to Unlock"),
            new("ui.skill_tree.hide_confirmation", "このウィンドウを表示しない", "Don't show this again"),
            new("ui.skill_tree.list_separator", "、", ", "),
            new("ui.battle_preparation.equipped_skills", "装備中のスキル", "Equipped Skills"),
            new("ui.battle_preparation.equipped_skill_details", "装備スキル詳細", "Equipped Skill Details"),
            new("ui.stage_select.title", "作戦", "Operations"),
            new("ui.home.mod_points", "改造ポイント", "Mod Points"),
            new("ui.home.unlock_points", "解放ポイント", "Unlock Points"),
            new("ui.setting.title", "設定", "Settings"),
            new("ui.setting.screen_mode", "画面モード", "Screen Mode"),
            new("ui.setting.resolution", "解像度", "Resolution"),
            new("ui.setting.quality", "画質", "Quality"),
            new("ui.setting.brightness", "明るさ", "Brightness"),
            new("ui.setting.return_to_title_message", "タイトル画面に戻りますか？", "Return to the title screen?"),
            new("ui.title.menu.sound_effect", "効果音", "SE"),
            new("ui.title.menu.data_reset_warning", "※削除するとデータは戻せません。", "* Deleted data cannot be restored."),
            new("ui.points.unlock", "解放P", "Unlock Points"),
            new("ui.battle_preparation.skill_type_heading", "スキルの種類　：", "Skill Type: "),
            new("ui.skill_tree.points_format", "解放P：{0}", "Unlock Points: {0}"),
            new("ui.skill_tree.reset_message_format", "スキルツリーをリセットしますか？\n返却される研究ポイント：{0}", "Reset the skill tree?\n返却される研究ポイント：{0}"),
            new("ui.skill_tree.unlock_points_format", "研究P：{0} → {1}", "研究P：{0} → {1}"),
            new("ui.setting.language", "言語", "言語"),
            new("ui.setting.vibration", "振動", "振動"),
            new("ui.setting.rhythm_offset", "リズム判定タイミング", "リズム判定タイミング"),
            new("ui.skill.empty_slot_symbol", "＋", "+"),
            new("ui.skill.max_level", "レベルMax", "Max Level"),
            new("ui.skill_detail.status_boost", "ステータス強化", "Stat Boost"),
            new("ui.setting.rhythm_offset_seconds_format", "{0}秒", "{0}s")
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
