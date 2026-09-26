using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider.Core
{
    /// <summary>
    ///     アクティブなBuild Profileのゲームデータ種別に合わせて、Addressables Groupの有効状態を切り替えます。
    ///     コンパイルシンボルとシーン一覧はBuild Profile自身が保持するため、ここでは変更しません。
    /// </summary>
    internal static class GameDataVariantBuildSettings
    {
        /// <summary>
        ///     アクティブなBuild Profileの種別をGroupへ反映し、体験版Profileのシーン一覧を同期します。
        /// </summary>
        /// <returns> 設定を適用できた場合は true、それ以外は false です。 </returns>
        public static bool ApplyActiveProfile()
        {
            GameDataVariantProfiles.SynchronizeActiveProfileScenes();
            return Apply(GameDataVariantProfiles.GetActiveVariant());
        }

        /// <summary>
        ///     指定種別に合わせてAddressables Groupのビルド含有状態を更新します。
        /// </summary>
        /// <param name="variant"> 反映するゲームデータ種別です。 </param>
        /// <returns> 設定を適用できた場合は true、それ以外は false です。 </returns>
        public static bool Apply(GameDataVariant variant)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError($"[{nameof(GameDataVariantBuildSettings)}] Addressables Settingsがありません。");
                return false;
            }

            AddressableAssetGroup releaseGroup = EnsureGroup(
                settings,
                GameDataVariantEditorState.RELEASE_GROUP_NAME);
            AddressableAssetGroup demoGroup = EnsureGroup(
                settings,
                GameDataVariantEditorState.DEMO_GROUP_NAME);
            AddressableAssetGroup sharedGroup = EnsureGroup(
                settings,
                GameDataVariantEditorState.SHARED_GROUP_NAME);

            SetIncludeInBuild(releaseGroup, variant == GameDataVariant.Release);
            SetIncludeInBuild(demoGroup, variant == GameDataVariant.Demo);
            SetIncludeInBuild(sharedGroup, true);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true);
            return true;
        }

        /// <summary>
        ///     Build Profile切り替えによるドメインリロード後に、アクティブなProfileの設定を反映します。
        /// </summary>
        [InitializeOnLoadMethod]
        private static void ApplyActiveProfileAfterLoad()
        {
            EditorApplication.delayCall += () => ApplyActiveProfile();
        }

        /// <summary>
        ///     指定名のGroupを取得し、なければ標準Schema付きで作成します。
        /// </summary>
        /// <param name="settings"> Addressablesの設定です。 </param>
        /// <param name="groupName"> 取得するGroup名です。 </param>
        /// <returns> 取得または作成したGroupです。 </returns>
        private static AddressableAssetGroup EnsureGroup(
            AddressableAssetSettings settings,
            string groupName)
        {
            AddressableAssetGroup group = settings.FindGroup(groupName);
            if (group != null)
            {
                return group;
            }

            return settings.CreateGroup(
                groupName,
                false,
                false,
                false,
                null,
                typeof(BundledAssetGroupSchema),
                typeof(ContentUpdateGroupSchema));
        }

        /// <summary>
        ///     Groupのビルド含有状態を設定します。
        /// </summary>
        /// <param name="group"> 対象のGroupです。 </param>
        /// <param name="includeInBuild"> ビルドへ含める場合はtrueです。 </param>
        private static void SetIncludeInBuild(AddressableAssetGroup group, bool includeInBuild)
        {
            BundledAssetGroupSchema schema = group?.GetSchema<BundledAssetGroupSchema>();
            if (schema != null && schema.IncludeInBuild != includeInBuild)
            {
                schema.IncludeInBuild = includeInBuild;
                EditorUtility.SetDirty(schema);
            }
        }
    }

}
