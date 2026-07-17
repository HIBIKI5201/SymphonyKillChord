using KillChord.Editor.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KillChord.Editor.Addressables
{
    /// <summary>
    ///     Addressablesのアドレスがファイル名形式で一意になっているか検証します。
    /// </summary>
    public sealed class AddressableKeyValidator : IPreprocessBuildWithReport
    {
        /// <summary> ビルド前処理の実行順です。 </summary>
        public int callbackOrder => 0;

        /// <summary>
        ///     ビルド前にAddressablesのアドレスを検証します。
        /// </summary>
        /// <param name="report"> ビルドレポートです。 </param>
        /// <exception cref="BuildFailedException"> アドレスに不備がある場合に送出されます。 </exception>
        public void OnPreprocessBuild(BuildReport report)
        {
            if (TryValidate(out string errorMessage))
            {
                return;
            }

            throw new BuildFailedException(errorMessage);
        }

        /// <summary>
        ///     メニュー操作からAddressablesのアドレスを検証します。
        /// </summary>
        [MenuItem(ToolConst.TOOLS_PATH + "Addressables/Validate Keys")]
        private static void ValidateFromMenu()
        {
            if (TryValidate(out string errorMessage))
            {
                Debug.Log($"[{nameof(AddressableKeyValidator)}] Addressablesのアドレスに問題はありません。");
                EditorUtility.DisplayDialog("Addressables Key Validation", "Addressablesのアドレスに問題はありません。", "OK");
                return;
            }

            Debug.LogError(errorMessage);
            EditorUtility.DisplayDialog("Addressables Key Validation", errorMessage, "OK");
        }

        /// <summary>
        ///     Addressablesの全エントリを検証します。
        /// </summary>
        /// <param name="errorMessage"> 検証に失敗した理由です。 </param>
        /// <returns> 全エントリが有効な場合はtrueです。 </returns>
        private static bool TryValidate(out string errorMessage)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
            {
                errorMessage = $"[{nameof(AddressableKeyValidator)}] AddressableAssetSettingsが見つかりません。";
                return false;
            }

            List<string> errors = new();
            Dictionary<string, string> assetPathByAddress = new(StringComparer.Ordinal);

            foreach (AddressableAssetGroup group in settings.groups)
            {
                if (group == null)
                {
                    continue;
                }

                foreach (AddressableAssetEntry entry in group.entries)
                {
                    ValidateEntry(entry, group.Name, assetPathByAddress, errors);
                }
            }

            if (errors.Count == 0)
            {
                errorMessage = string.Empty;
                return true;
            }

            errorMessage = $"[{nameof(AddressableKeyValidator)}] Addressablesのアドレスに不備があります。\n- "
                + string.Join("\n- ", errors);
            return false;
        }

        /// <summary>
        ///     Addressablesのエントリを検証します。
        /// </summary>
        /// <param name="entry"> 検証対象のエントリです。 </param>
        /// <param name="groupName"> エントリが所属するグループ名です。 </param>
        /// <param name="assetPathByAddress"> アドレスごとのアセットパスです。 </param>
        /// <param name="errors"> 検出したエラー一覧です。 </param>
        private static void ValidateEntry(
            AddressableAssetEntry entry,
            string groupName,
            Dictionary<string, string> assetPathByAddress,
            List<string> errors)
        {
            if (entry == null)
            {
                errors.Add($"グループ「{groupName}」にnullのエントリがあります。");
                return;
            }

            string address = entry.address;
            string assetPath = entry.AssetPath;

            if (string.IsNullOrWhiteSpace(address))
            {
                errors.Add($"アドレスが未設定です。AssetPath: {assetPath}");
                return;
            }

            if (address.Contains('/') || address.Contains('\\'))
            {
                errors.Add($"アドレスにパス区切り文字を使用できません。Address: {address}");
            }

            string expectedAddress = Path.GetFileNameWithoutExtension(assetPath);

            if (!string.Equals(address, expectedAddress, StringComparison.Ordinal))
            {
                errors.Add($"アドレスは拡張子を除いたファイル名にしてください。Address: {address}, Expected: {expectedAddress}");
            }

            if (assetPathByAddress.TryGetValue(address, out string registeredAssetPath))
            {
                errors.Add($"アドレスが重複しています。Address: {address}, Assets: {registeredAssetPath}, {assetPath}");
                return;
            }

            assetPathByAddress.Add(address, assetPath);
        }
    }
}
