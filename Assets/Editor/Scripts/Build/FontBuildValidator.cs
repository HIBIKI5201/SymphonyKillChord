using KillChord.Editor.Utility;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace KillChord.Editor.Build
{
    /// <summary>
    ///     プレイヤービルドで使用するTextMesh ProとUI Toolkitのフォント設定を検証します。
    /// </summary>
    public sealed class FontBuildValidator : IPreprocessBuildWithReport
    {
        /// <summary> 他のビルド前処理より先にフォントを検証する実行順です。 </summary>
        public int callbackOrder => VALIDATION_ORDER;

        private const int VALIDATION_ORDER = -200;
        private const int DYNAMIC_ATLAS_POPULATION_MODE = 1;
        private const string TMP_SETTINGS_PATH =
            "Assets/Arts/TextAssets/TextMesh Pro/Resources/TMP Settings.asset";
        private const string UI_TOOLKIT_TEXT_SETTINGS_PATH =
            "Assets/Settings/UI Toolkit/UITK Text Settings.asset";
        private const string REQUIRED_CHARACTERS =
            "日本語表示テスト：設定・研究・改造・作戦・戻る・保存・出撃・効果音・攻撃力・クリティカル率";

        private static readonly string[] PANEL_SETTINGS_FOLDERS =
        {
            "Assets/Settings/UI Toolkit",
            "Assets/Level/Data/Master/UI"
        };

        /// <summary>
        ///     ビルド開始前にフォント設定を検証します。
        /// </summary>
        /// <param name="report"> ビルドレポートです。 </param>
        /// <exception cref="BuildFailedException"> フォント設定に不備がある場合に送出されます。 </exception>
        public void OnPreprocessBuild(BuildReport report)
        {
            List<string> errors = CollectErrors();

            if (errors.Count == 0)
            {
                return;
            }

            throw new BuildFailedException(CreateErrorMessage(errors));
        }

        /// <summary>
        ///     メニュー操作からフォント設定を検証します。
        /// </summary>
        [MenuItem(ToolConst.TOOLS_PATH + "Build/Validate Fonts")]
        private static void ValidateFromMenu()
        {
            List<string> errors = CollectErrors();

            if (errors.Count == 0)
            {
                Debug.Log($"[{nameof(FontBuildValidator)}] フォント設定に問題はありません。");
                EditorUtility.DisplayDialog("Font Build Validation", "フォント設定に問題はありません。", "OK");
                return;
            }

            string errorMessage = CreateErrorMessage(errors);
            Debug.LogError(errorMessage);
            EditorUtility.DisplayDialog("Font Build Validation", errorMessage, "OK");
        }

        /// <summary>
        ///     TextMesh ProとUI Toolkitのフォント設定エラーを収集します。
        /// </summary>
        /// <returns> 検出したエラー一覧です。 </returns>
        private static List<string> CollectErrors()
        {
            List<string> errors = new();
            ValidateTextMeshProSettings(errors);
            ValidateUiToolkitSettings(errors);
            return errors;
        }

        /// <summary>
        ///     TextMesh Proの既定フォントを検証します。
        /// </summary>
        /// <param name="errors"> 検出したエラー一覧です。 </param>
        private static void ValidateTextMeshProSettings(List<string> errors)
        {
            TMP_Settings settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(TMP_SETTINGS_PATH);

            if (settings == null)
            {
                errors.Add($"TMP Settingsが見つかりません。Path: {TMP_SETTINGS_PATH}");
                return;
            }

            SerializedObject serializedSettings = new(settings);
            ValidateTextSettingsClearOnBuild(serializedSettings, "TMP Settings", errors);
            TMP_FontAsset fontAsset = serializedSettings.FindProperty("m_defaultFontAsset")?.objectReferenceValue
                as TMP_FontAsset;

            if (fontAsset == null)
            {
                errors.Add("TMP SettingsのDefault Font Assetを設定してください。");
                return;
            }

            ValidateDynamicFontAsset(fontAsset, "TextMesh Pro", errors);
            ValidateRequiredCharacters(fontAsset, "TextMesh Pro", errors);
            ValidateSourceFont(fontAsset.sourceFontFile, "TextMesh Pro", errors);
            ValidateBuildDependencies(TMP_SETTINGS_PATH, fontAsset, fontAsset.sourceFontFile, "TextMesh Pro", errors);
        }

        /// <summary>
        ///     UI Toolkitの既定フォントとPanel Settingsを検証します。
        /// </summary>
        /// <param name="errors"> 検出したエラー一覧です。 </param>
        private static void ValidateUiToolkitSettings(List<string> errors)
        {
            ScriptableObject textSettings =
                AssetDatabase.LoadAssetAtPath<ScriptableObject>(UI_TOOLKIT_TEXT_SETTINGS_PATH);

            if (textSettings == null)
            {
                errors.Add($"UI Toolkit Text Settingsが見つかりません。Path: {UI_TOOLKIT_TEXT_SETTINGS_PATH}");
                return;
            }

            SerializedObject serializedSettings = new(textSettings);
            ValidateTextSettingsClearOnBuild(serializedSettings, "UI Toolkit Text Settings", errors);
            FontAsset fontAsset = serializedSettings.FindProperty("m_DefaultFontAsset")?.objectReferenceValue
                as FontAsset;

            if (fontAsset == null)
            {
                errors.Add("UI Toolkit Text SettingsのDefault Font Assetを設定してください。");
                return;
            }

            ValidateDynamicFontAsset(fontAsset, "UI Toolkit", errors);
            ValidateRequiredCharacters(fontAsset, "UI Toolkit", errors);
            ValidateSourceFont(fontAsset.sourceFontFile, "UI Toolkit", errors);
            ValidateBuildDependencies(
                UI_TOOLKIT_TEXT_SETTINGS_PATH,
                fontAsset,
                fontAsset.sourceFontFile,
                "UI Toolkit",
                errors);
            ValidatePanelSettingsReferences(textSettings, errors);
        }

        /// <summary>
        ///     Text Settingsがビルド時にDynamicフォントデータを消去しない設定か検証します。
        /// </summary>
        /// <param name="serializedSettings"> 検証対象のText Settingsです。 </param>
        /// <param name="label"> エラー表示用の設定名です。 </param>
        /// <param name="errors"> 検出したエラー一覧です。 </param>
        private static void ValidateTextSettingsClearOnBuild(
            SerializedObject serializedSettings,
            string label,
            List<string> errors)
        {
            SerializedProperty clearOnBuild = serializedSettings.FindProperty("m_ClearDynamicDataOnBuild");

            if (clearOnBuild == null || clearOnBuild.boolValue)
            {
                errors.Add($"{label}でClear Dynamic Data On Buildを無効にしてください。");
            }
        }

        /// <summary>
        ///     Dynamicフォントのビルド向け設定を検証します。
        /// </summary>
        /// <param name="fontAsset"> 検証対象のフォントアセットです。 </param>
        /// <param name="label"> エラー表示用のフォント種別です。 </param>
        /// <param name="errors"> 検出したエラー一覧です。 </param>
        private static void ValidateDynamicFontAsset(Object fontAsset, string label, List<string> errors)
        {
            SerializedObject serializedFont = new(fontAsset);
            SerializedProperty populationMode = serializedFont.FindProperty("m_AtlasPopulationMode");
            SerializedProperty multiAtlas = serializedFont.FindProperty("m_IsMultiAtlasTexturesEnabled");
            SerializedProperty clearOnBuild = serializedFont.FindProperty("m_ClearDynamicDataOnBuild");

            if (populationMode == null || populationMode.intValue != DYNAMIC_ATLAS_POPULATION_MODE)
            {
                errors.Add($"{label}の既定フォントをDynamic Atlasに設定してください。Asset: {fontAsset.name}");
            }

            if (multiAtlas == null || !multiAtlas.boolValue)
            {
                errors.Add($"{label}の既定フォントでMulti Atlas Texturesを有効にしてください。Asset: {fontAsset.name}");
            }

            if (clearOnBuild == null || clearOnBuild.boolValue)
            {
                errors.Add($"{label}の既定フォントでClear Dynamic Data On Buildを無効にしてください。Asset: {fontAsset.name}");
            }
        }

        /// <summary>
        ///     ビルドに最低限保持する日本語グリフを検証します。
        /// </summary>
        /// <param name="fontAsset"> 検証対象のフォントアセットです。 </param>
        /// <param name="label"> エラー表示用のフォント種別です。 </param>
        /// <param name="errors"> 検出したエラー一覧です。 </param>
        private static void ValidateRequiredCharacters(FontAsset fontAsset, string label, List<string> errors)
        {
            if (!fontAsset.HasCharacters(REQUIRED_CHARACTERS))
            {
                errors.Add($"{label}の既定フォントに必須日本語グリフが保存されていません。Asset: {fontAsset.name}");
            }
        }

        /// <summary>
        ///     ビルドに最低限保持するTextMesh Proの日本語グリフを検証します。
        /// </summary>
        /// <param name="fontAsset"> 検証対象のフォントアセットです。 </param>
        /// <param name="label"> エラー表示用のフォント種別です。 </param>
        /// <param name="errors"> 検出したエラー一覧です。 </param>
        private static void ValidateRequiredCharacters(TMP_FontAsset fontAsset, string label, List<string> errors)
        {
            if (!fontAsset.HasCharacters(REQUIRED_CHARACTERS))
            {
                errors.Add($"{label}の既定フォントに必須日本語グリフが保存されていません。Asset: {fontAsset.name}");
            }
        }

        /// <summary>
        ///     元フォントファイルが必須文字を収録しているか検証します。
        /// </summary>
        /// <param name="sourceFont"> 検証対象の元フォントです。 </param>
        /// <param name="label"> エラー表示用のフォント種別です。 </param>
        /// <param name="errors"> 検出したエラー一覧です。 </param>
        private static void ValidateSourceFont(Font sourceFont, string label, List<string> errors)
        {
            if (sourceFont == null)
            {
                errors.Add($"{label}のDynamicフォントにSource Font Fileを設定してください。");
                return;
            }

            HashSet<char> missingCharacters = new();

            foreach (char character in REQUIRED_CHARACTERS)
            {
                if (!sourceFont.HasCharacter(character))
                {
                    missingCharacters.Add(character);
                }
            }

            if (missingCharacters.Count > 0)
            {
                errors.Add($"{label}のSource Font Fileに必須文字がありません。Missing: {string.Concat(missingCharacters)}");
            }
        }

        /// <summary>
        ///     Text Settingsからフォントアセットと元フォントへビルド依存が到達するか検証します。
        /// </summary>
        /// <param name="settingsPath"> Text Settingsのアセットパスです。 </param>
        /// <param name="fontAsset"> 既定フォントアセットです。 </param>
        /// <param name="sourceFont"> 元フォントです。 </param>
        /// <param name="label"> エラー表示用のフォント種別です。 </param>
        /// <param name="errors"> 検出したエラー一覧です。 </param>
        private static void ValidateBuildDependencies(
            string settingsPath,
            Object fontAsset,
            Font sourceFont,
            string label,
            List<string> errors)
        {
            HashSet<string> dependencies = new(AssetDatabase.GetDependencies(settingsPath, true));
            string fontAssetPath = AssetDatabase.GetAssetPath(fontAsset);
            string sourceFontPath = AssetDatabase.GetAssetPath(sourceFont);

            if (string.IsNullOrEmpty(fontAssetPath) || !dependencies.Contains(fontAssetPath))
            {
                errors.Add($"{label}の既定フォントがText Settingsのビルド依存に含まれていません。");
            }

            if (string.IsNullOrEmpty(sourceFontPath) || !dependencies.Contains(sourceFontPath))
            {
                errors.Add($"{label}のSource Font FileがText Settingsのビルド依存に含まれていません。");
            }
        }

        /// <summary>
        ///     全Panel Settingsがプロジェクト共通のUI Toolkit Text Settingsを参照しているか検証します。
        /// </summary>
        /// <param name="expectedTextSettings"> 参照すべきText Settingsです。 </param>
        /// <param name="errors"> 検出したエラー一覧です。 </param>
        private static void ValidatePanelSettingsReferences(Object expectedTextSettings, List<string> errors)
        {
            string[] panelSettingsGuids = AssetDatabase.FindAssets("t:PanelSettings", PANEL_SETTINGS_FOLDERS);

            foreach (string guid in panelSettingsGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object panelSettings = AssetDatabase.LoadMainAssetAtPath(path);

                if (panelSettings == null)
                {
                    errors.Add($"Panel Settingsを読み込めません。Path: {path}");
                    continue;
                }

                SerializedObject serializedPanelSettings = new(panelSettings);
                Object textSettings = serializedPanelSettings.FindProperty("textSettings")?.objectReferenceValue;

                if (textSettings != expectedTextSettings)
                {
                    errors.Add($"Panel Settingsに共通のUI Toolkit Text Settingsを設定してください。Path: {path}");
                }
            }
        }

        /// <summary>
        ///     検証エラー一覧をログ表示用メッセージに変換します。
        /// </summary>
        /// <param name="errors"> 検出したエラー一覧です。 </param>
        /// <returns> ログ表示用メッセージです。 </returns>
        private static string CreateErrorMessage(List<string> errors)
        {
            return $"[{nameof(FontBuildValidator)}] ビルド用フォント設定に不備があります。\n- "
                + string.Join("\n- ", errors);
        }
    }
}
