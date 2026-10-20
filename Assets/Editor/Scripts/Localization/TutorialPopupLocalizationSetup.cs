using System;
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
    ///     チュートリアル説明ポップアップの文言(タイトル・本文・閉じる案内)のString Tableを作成します。
    ///     本文の入力アイコンは、UICommonの入力アイコンと同じSmart Stringで入力機器ごとに切り替えます。
    /// </summary>
    public static class TutorialPopupLocalizationSetup
    {
        /// <summary> チュートリアル説明ポップアップのString Table Collection名です。 </summary>
        public const string TUTORIAL_POPUPS_TABLE_NAME = "TutorialPopups";

        /// <summary>
        ///     チュートリアル説明ポップアップのString Tableを作成または更新します。
        ///     入力済みの文言は上書きしません。
        /// </summary>
        [MenuItem("KillChord/Localization/チュートリアルポップアップ用テーブルをセットアップ")]
        public static void Setup()
        {
            List<Locale> locales = LocalizationEditorSettings.GetLocales()
                .Where(locale => locale.Identifier.Code == "ja" || locale.Identifier.Code == "en")
                .ToList();
            if (locales.Count != 2)
            {
                Debug.LogError(
                    $"[{nameof(TutorialPopupLocalizationSetup)}] 日本語と英語のロケールが必要です。先にチュートリアル用テーブルをセットアップしてください。");
                return;
            }

            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(TUTORIAL_POPUPS_TABLE_NAME)
                ?? LocalizationEditorSettings.CreateStringTableCollection(TUTORIAL_POPUPS_TABLE_NAME, TABLES_DIRECTORY, locales);

            foreach (EntryDefinition definition in _definitions)
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
                            $"{TUTORIAL_POPUPS_TABLE_NAME}テーブルに{locale.Identifier.Code}ロケールがありません。");
                    }

                    string text = locale.Identifier.Code == "ja" ? definition.Japanese : definition.English;
                    StringTableEntry entry = table.GetEntry(sharedEntry.Id);
                    if (entry != null && !string.IsNullOrWhiteSpace(entry.Value))
                    {
                        continue;
                    }

                    entry ??= table.AddEntry(sharedEntry.Id, text);
                    entry.Value = text;

                    // 入力アイコンを含む文言は、入力機器の変数を参照するSmart Stringにする。
                    entry.IsSmart = text.Contains("{input.");
                    EditorUtility.SetDirty(table);
                }
            }

            EditorUtility.SetDirty(collection);
            EditorUtility.SetDirty(collection.SharedData);
            LocalizationEditorSettings.EditorEvents.RaiseCollectionModified(null, collection);
            AssetDatabase.SaveAssets();
            Debug.Log($"[{nameof(TutorialPopupLocalizationSetup)}] {_definitions.Length}件の文言を確認しました。");
        }

        private const string TABLES_DIRECTORY = "Assets/Localization/Tables";

        // UICommonの ui.input.glyph.attack_button / dodge / move と同じ定義。
        private const string ATTACK_GLYPH =
            "{input.device:choose(keyboard|xbox|playstation|switch):<sprite name=\"ml\">|<sprite name=\"xb\">|<sprite name=\"po\">|<sprite name=\"na\">}";
        private const string DODGE_GLYPH =
            "{input.device:choose(keyboard|xbox|playstation|switch):<sprite name=\"space\">|<sprite name=\"xa\">|<sprite name=\"px\">|<sprite name=\"nb\">}";
        private const string MOVE_GLYPH =
            "{input.device:choose(keyboard|xbox|playstation|switch):<size=80%><sprite name=\"w\"><sprite name=\"a\"><sprite name=\"s\"><sprite name=\"d\"></size>|<sprite name=\"jl\">|<sprite name=\"jl\">|<sprite name=\"jl\">}";

        private static readonly EntryDefinition[] _definitions =
        {
            new("tutorial.popup.close", "閉じる", "Close"),
            new("tutorial.popup.close_tap", "タップして閉じる", "Tap to close"),

            new("tutorial.popup.move.title", "移動と回避", "Move & Dodge"),
            new("tutorial.popup.move.body",
                "移動と回避で、\n敵の攻撃を避けることができます。\n回避は前方へステップし、一時的にあらゆる\n攻撃を無効化できる。\n\n"
                + DODGE_GLYPH + " 回避　　　　" + MOVE_GLYPH + " 移動",
                "Moving and dodging let you avoid enemy attacks.\nDodge steps you forward and briefly nullifies all incoming attacks.\n\n"
                + DODGE_GLYPH + " Dodge　　　　" + MOVE_GLYPH + " Move"),

            new("tutorial.popup.attack.title", "攻撃", "Attack"),
            new("tutorial.popup.attack.body",
                ATTACK_GLYPH + "ボタンを押すことで攻撃が可能\n画面下のバーに波があり、\n色によって攻撃の種類が変わる",
                "Press the " + ATTACK_GLYPH + " button to attack.\nThere's a wave on the bar at the bottom of the screen,\nand the color changes the type of attack."),

            new("tutorial.popup.kill_chord.title", "キルコード（スキル）", "Kill Chord (Skill)"),
            new("tutorial.popup.kill_chord.body",
                "画面下にある赤枠内にある六角形の色や\n白枠のアイコンのタイミングでの\n攻撃を連続で行うことで、\nキルコードという特殊な攻撃を発動できる",
                "By landing attacks in a row that match\nthe colors of the hexagons\ninside the red frame, and the timing of the icons\nin the white frame at the bottom of the screen,\nyou can trigger a special attack called a Kill Chord."),

            new("tutorial.popup.just_attack.title", "Just攻撃", "Just Attack"),
            new("tutorial.popup.just_attack.body",
                "画面下のバーの判定が\n各攻撃の黄色の線の時に攻撃をすると\nJust攻撃という\n通常よりも強力な攻撃を行います",
                "If you attack exactly when the judgment bar\nat the bottom of the screen lines up\nwith each attack's yellow line,\nyou'll perform a \"Just Attack\" —\nan attack stronger than normal."),

            new("tutorial.popup.green.title", "緑攻撃", "Green Attack"),
            new("tutorial.popup.green.body",
                "使用武器：ライフル\n緑色のタイミングで攻撃時に行う、\n比較的扱いやすい攻撃。\nその攻撃はあらゆる場面で\n安定した性能を発揮する。",
                "Weapon: Rifle\nPerformed by attacking on the green timing.\nA relatively easy-to-use attack\nthat delivers stable performance\nin any situation."),

            new("tutorial.popup.orange.title", "オレンジ攻撃", "Orange Attack"),
            new("tutorial.popup.orange.body",
                "使用武器：サブマシンガン\nオレンジ色のタイミングで攻撃時に行う、\n素早い連続攻撃。\n3点バーストで弾を発射し、\n多段ヒットでダメージを与える。",
                "Weapon: Submachine Gun\nPerformed by attacking on the orange timing.\nA rapid, continuous attack.\nFires 3-round bursts,\ndealing damage with multiple hits."),

            new("tutorial.popup.blue.title", "青攻撃", "Blue Attack"),
            new("tutorial.popup.blue.body",
                "使用武器：ショットガン\n青のタイミングで攻撃時に行う、\n広範囲を攻撃できる近距離範囲攻撃。\n扇状に弾を発射し、複数の敵を同時に攻撃\nできるが、遠距離にいる的には効果が薄い。",
                "Weapon: Shotgun\nPerformed by attacking on the blue timing.\nA close-range attack that covers a wide area.\nFires a fan-shaped spread that can hit multiple enemies at once,\nbut is less effective against distant targets."),

            new("tutorial.popup.purple.title", "紫攻撃", "Purple Attack"),
            new("tutorial.popup.purple.body",
                "使用武器：対物ライフル\n紫のタイミングで攻撃時に行う攻撃、\nダメージが高く、射程の長いが攻撃に時間が\nかかる弾丸は一直線に飛び、敵を貫通して\n複数の敵を同時に攻撃できる",
                "Weapon: Anti-Materiel Rifle\nPerformed by attacking on the purple timing.\nHigh damage and long range, but slow to execute.\nThe bullet travels in a straight line and pierces enemies,\nhitting multiple targets at once."),

            new("tutorial.popup.light_blue.title", "水色攻撃", "Light Blue Attack"),
            new("tutorial.popup.light_blue.body",
                "使用武器：リボルバー\n水色のタイミングで攻撃時に行う、\n威力と制圧力を兼ね備えた攻撃。\n攻撃対象をノックバックさせやすい。",
                "Weapon: Revolver\nPerformed by attacking on the light blue timing.\nAn attack that combines power and suppression.\nEasily knocks back the target."),

            new("tutorial.popup.yellow.title", "黄攻撃", "Yellow Attack"),
            new("tutorial.popup.yellow.body",
                "使用武器：ハンドガン\n黄色のタイミングで攻撃時に行う、\n素早く威力の高い攻撃。\nそのため、入力難易度は高く\n正確なリズム入力によって真価を発揮する。",
                "Weapon: Handgun\nPerformed by attacking on the yellow timing.\nA fast, high-power attack.\nBecause of that, it's difficult to input,\nand shows its true value with precise rhythm timing."),
        };

        /// <summary>
        ///     各ロケールの文言の定義です。
        /// </summary>
        private readonly struct EntryDefinition
        {
            /// <summary>
            ///     文言の定義を生成する。
            /// </summary>
            public EntryDefinition(string key, string japanese, string english)
            {
                Key = key;
                Japanese = japanese;
                English = english;
            }

            /// <summary> テーブルキーです。 </summary>
            public string Key { get; }
            /// <summary> 日本語の文言です。 </summary>
            public string Japanese { get; }
            /// <summary> 英語の文言です。 </summary>
            public string English { get; }
        }
    }
}
