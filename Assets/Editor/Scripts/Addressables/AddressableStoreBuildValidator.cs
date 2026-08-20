using KillChord.Editor.Utility;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KillChord.Editor.Addressables
{
    /// <summary>
    ///     Google PlayとSteamへローカルAddressablesを同梱するための設定を検証します。
    /// </summary>
    public sealed class AddressableStoreBuildValidator : IPreprocessBuildWithReport
    {
        /// <summary> Addressables標準ビルド処理より前に検証する実行順です。 </summary>
        public int callbackOrder => -100;

        /// <summary>
        ///     プレイヤービルド前にストア配信用Addressables設定を検証します。
        /// </summary>
        /// <param name="report"> ビルドレポートです。 </param>
        /// <exception cref="BuildFailedException"> ストア配信用設定に不備がある場合に送出されます。 </exception>
        public void OnPreprocessBuild(BuildReport report)
        {
            List<string> errors = CollectErrors(report);

            if (errors.Count == 0)
            {
                return;
            }

            throw new BuildFailedException(CreateErrorMessage(errors));
        }

        /// <summary>
        ///     メニュー操作からストア配信用Addressables設定を検証します。
        /// </summary>
        [MenuItem(ToolConst.TOOLS_PATH + "Addressables/Validate Store Build Settings")]
        private static void ValidateFromMenu()
        {
            List<string> errors = CollectErrors(null);

            if (errors.Count == 0)
            {
                Debug.Log($"[{nameof(AddressableStoreBuildValidator)}] ストア配信用Addressables設定に問題はありません。");
                EditorUtility.DisplayDialog(
                    "Addressables Store Build Validation",
                    "ストア配信用Addressables設定に問題はありません。",
                    "OK");
                return;
            }

            string errorMessage = CreateErrorMessage(errors);
            Debug.LogError(errorMessage);
            EditorUtility.DisplayDialog("Addressables Store Build Validation", errorMessage, "OK");
        }

        /// <summary>
        ///     ストア配信用Addressables設定のエラーを収集します。
        /// </summary>
        /// <param name="report"> ビルドレポートです。メニュー検証時はnullです。 </param>
        /// <returns> 検出したエラー一覧です。 </returns>
        private static List<string> CollectErrors(BuildReport report)
        {
            List<string> errors = new();
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
            {
                errors.Add("AddressableAssetSettingsが見つかりません。");
                return errors;
            }

            if (settings.BuildAddressablesWithPlayerBuild
                != AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer)
            {
                errors.Add("Build Addressables on Player BuildをBuild with Playerに設定してください。");
            }

            if (settings.BuildRemoteCatalog)
            {
                errors.Add("ストア同梱方式ではRemote Catalogを無効にしてください。");
            }

            ValidateGroupSchemas(settings, errors);
            ValidateGooglePlayBuild(report, errors);
            return errors;
        }

        /// <summary>
        ///     全Addressablesグループがローカル同梱向けか検証します。
        /// </summary>
        /// <param name="settings"> Addressables設定です。 </param>
        /// <param name="errors"> 検出したエラー一覧です。 </param>
        private static void ValidateGroupSchemas(AddressableAssetSettings settings, List<string> errors)
        {
            foreach (AddressableAssetGroup group in settings.groups)
            {
                if (group == null)
                {
                    continue;
                }

                BundledAssetGroupSchema schema = group.GetSchema<BundledAssetGroupSchema>();

                if (schema == null)
                {
                    continue;
                }

                string buildPath = schema.BuildPath.GetValue(settings, false);
                string loadPath = schema.LoadPath.GetValue(settings, false);

                if (!string.Equals(
                    buildPath,
                    AddressableAssetSettings.kLocalBuildPathValue,
                    StringComparison.Ordinal))
                {
                    errors.Add($"グループ「{group.Name}」のBuild PathをLocal.BuildPathに設定してください。");
                }

                if (!string.Equals(
                    loadPath,
                    AddressableAssetSettings.kLocalLoadPathValue,
                    StringComparison.Ordinal))
                {
                    errors.Add($"グループ「{group.Name}」のLoad PathをLocal.LoadPathに設定してください。");
                }

                if (schema.Compression != BundledAssetGroupSchema.BundleCompressionMode.LZ4)
                {
                    errors.Add($"グループ「{group.Name}」の圧縮形式をLZ4に設定してください。");
                }

                if (schema.BundleMode != BundledAssetGroupSchema.BundlePackingMode.PackTogether)
                {
                    errors.Add($"グループ「{group.Name}」のBundle ModeをPack Togetherに設定してください。");
                }
            }
        }

        /// <summary>
        ///     Google Play向けリリースビルドがAAB形式か検証します。
        /// </summary>
        /// <param name="report"> ビルドレポートです。 </param>
        /// <param name="errors"> 検出したエラー一覧です。 </param>
        private static void ValidateGooglePlayBuild(BuildReport report, List<string> errors)
        {
            if (report == null || report.summary.platform != BuildTarget.Android)
            {
                return;
            }

            bool isDevelopmentBuild = EditorUserBuildSettings.development;

            if (!isDevelopmentBuild && !EditorUserBuildSettings.buildAppBundle)
            {
                errors.Add("Google Play向けリリースビルドはAAB形式に設定してください。");
            }
        }

        /// <summary>
        ///     検証エラー一覧をログ表示用メッセージに変換します。
        /// </summary>
        /// <param name="errors"> 検出したエラー一覧です。 </param>
        /// <returns> ログ表示用メッセージです。 </returns>
        private static string CreateErrorMessage(List<string> errors)
        {
            return $"[{nameof(AddressableStoreBuildValidator)}] ストア配信用Addressables設定に不備があります。\n- "
                + string.Join("\n- ", errors);
        }
    }
}
