using System;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider.Core
{
    /// <summary>
    ///     Addressablesの梱包前に、ビルドするProfileのゲームデータ種別と必須アセットを確定します。
    /// </summary>
    internal sealed class GameDataVariantBuildProcessor : BuildPlayerProcessor
    {
        /// <summary> AddressablesPlayerBuildProcessor（順序1）より先に実行します。 </summary>
        public override int callbackOrder => -1000;

        /// <summary>
        ///     Profileに対応するGroupだけを有効化し、コードとデータの不整合をビルド前に検出します。
        /// </summary>
        /// <param name="buildPlayerContext"> 対象のPlayerビルドです。 </param>
        public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
        {
            GameDataVariant variant = GameDataVariantProfiles.GetActiveVariant();
#if KILLCHORD_DEMO
            const GameDataVariant COMPILED_VARIANT = GameDataVariant.Demo;
#else
            const GameDataVariant COMPILED_VARIANT = GameDataVariant.Release;
#endif
            if (variant != COMPILED_VARIANT)
            {
                throw new BuildFailedException(
                    $"Build Profileの種別（{variant}）とコンパイル済みコード（{COMPILED_VARIANT}）が一致しません。"
                    + "KILLCHORD_DEMOは共通のPlayer Settingsではなく体験版Build Profileだけに設定してください。");
            }

            // IPreprocessBuildWithReportではAddressablesのビルドが既に終わっているため、ここで適用します。
            if (!GameDataVariantBuildSettings.Apply(variant))
            {
                throw new BuildFailedException("ゲームデータ種別のビルド設定を適用できませんでした。");
            }

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = settings.FindGroup(GameDataVariantEditorState.GetGroupName(variant));
            BundledAssetGroupSchema schema = group?.GetSchema<BundledAssetGroupSchema>();
            if (schema == null || !schema.IncludeInBuild || !schema.IncludeAddressInCatalog)
            {
                throw new BuildFailedException($"{variant}のGroupをアドレス付きでビルドできません。");
            }

            ValidateEntry(group, "StageTreeAsset");
            ValidateEntry(group, "EnemyMissionKeyRepository");
            ValidateEntry(group, "MissionDefinitionRepository");
            if (variant == GameDataVariant.Demo)
            {
                ValidateEntry(group, "DemoRuntime");
                ValidateEntry(group, "DemoExperienceConfig");
            }

            Debug.Log($"[{nameof(GameDataVariantBuildProcessor)}] Addressables build variant: {variant}, group: {group.Name}");
        }

        /// <summary>
        ///     起動時にアドレスで参照する必須アセットがGroup内に存在し、インポート済みか確認します。
        /// </summary>
        /// <param name="group"> ビルドに含めるGroupです。 </param>
        /// <param name="address"> 必須アセットのアドレスです。 </param>
        private static void ValidateEntry(AddressableAssetGroup group, string address)
        {
            AddressableAssetEntry entry = group.entries.FirstOrDefault(
                candidate => string.Equals(candidate.address, address, StringComparison.Ordinal));
            if (entry == null || AssetDatabase.LoadMainAssetAtPath(entry.AssetPath) == null)
            {
                throw new BuildFailedException($"{group.Name}の必須アセットがありません: {address}");
            }
        }
    }
}
