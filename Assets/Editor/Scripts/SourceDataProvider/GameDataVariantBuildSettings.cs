using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     Release/DemoのコンパイルシンボルとAddressables Group有効状態を一括で切り替えます。
    /// </summary>
    internal static class GameDataVariantBuildSettings
    {
        [InitializeOnLoadMethod]
        private static void ApplySelectedVariantAfterLoad()
        {
            EditorApplication.delayCall += () => Apply(GameDataVariantEditorState.SelectedVariant);
        }

        [MenuItem("KillChord/Game Data Variant/Release")]
        private static void SelectRelease()
        {
            GameDataVariantEditorState.SetSelectedVariant(GameDataVariant.Release);
            Apply(GameDataVariant.Release);
        }

        [MenuItem("KillChord/Game Data Variant/Demo")]
        private static void SelectDemo()
        {
            GameDataVariantEditorState.SetSelectedVariant(GameDataVariant.Demo);
            Apply(GameDataVariant.Demo);
        }

        [MenuItem("KillChord/Game Data Variant/Release", true)]
        private static bool ValidateReleaseMenu()
        {
            Menu.SetChecked(
                "KillChord/Game Data Variant/Release",
                GameDataVariantEditorState.SelectedVariant == GameDataVariant.Release);
            return true;
        }

        [MenuItem("KillChord/Game Data Variant/Demo", true)]
        private static bool ValidateDemoMenu()
        {
            Menu.SetChecked(
                "KillChord/Game Data Variant/Demo",
                GameDataVariantEditorState.SelectedVariant == GameDataVariant.Demo);
            return true;
        }

        /// <summary>
        ///     選択種別に合わせてGroupとコンパイルシンボルを更新します。
        /// </summary>
        public static void Apply(GameDataVariant variant)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError($"[{nameof(GameDataVariantBuildSettings)}] Addressables Settingsがありません。");
                return;
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
            UpdateDemoEndScene(variant == GameDataVariant.Demo);
            UpdateDemoDefine(variant == GameDataVariant.Demo);
        }

        /// <summary>
        ///     指定名のGroupを取得し、なければ標準Schema付きで作成します。
        /// </summary>
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
        private static void SetIncludeInBuild(AddressableAssetGroup group, bool includeInBuild)
        {
            BundledAssetGroupSchema schema = group?.GetSchema<BundledAssetGroupSchema>();
            if (schema != null && schema.IncludeInBuild != includeInBuild)
            {
                schema.IncludeInBuild = includeInBuild;
                EditorUtility.SetDirty(schema);
            }
        }

        /// <summary>
        ///     現在のビルドターゲットへ体験版専用defineを反映します。
        /// </summary>
        private static void UpdateDemoDefine(bool enabled)
        {
            NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup);
            string currentValue = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            HashSet<string> defines = new(
                currentValue.Split(';', StringSplitOptions.RemoveEmptyEntries),
                StringComparer.Ordinal);
            bool changed = enabled
                ? defines.Add(DEMO_DEFINE)
                : defines.Remove(DEMO_DEFINE);
            if (!changed)
            {
                return;
            }

            PlayerSettings.SetScriptingDefineSymbols(
                namedBuildTarget,
                string.Join(";", defines));
        }

        /// <summary>
        ///     体験版終了シーンをDemoビルドだけへ含めます。
        /// </summary>
        private static void UpdateDemoEndScene(bool enabled)
        {
            List<EditorBuildSettingsScene> scenes = new(EditorBuildSettings.scenes);
            int index = scenes.FindIndex(scene => string.Equals(
                scene.path,
                DEMO_END_SCENE_PATH,
                StringComparison.Ordinal));
            if (index < 0)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(DEMO_END_SCENE_PATH) != null)
                {
                    scenes.Add(new EditorBuildSettingsScene(DEMO_END_SCENE_PATH, enabled));
                    EditorBuildSettings.scenes = scenes.ToArray();
                }

                return;
            }

            if (scenes[index].enabled == enabled)
            {
                return;
            }

            scenes[index] = new EditorBuildSettingsScene(DEMO_END_SCENE_PATH, enabled);
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        internal const string DEMO_DEFINE = "KILLCHORD_DEMO";
        private const string DEMO_END_SCENE_PATH = "Assets/Level/Scenes/Demo/DemoEnd.unity";
    }

    /// <summary>
    ///     ビルド直前に選択データ種別と成果物設定の不整合を防止します。
    /// </summary>
    internal sealed class GameDataVariantBuildPreprocessor : IPreprocessBuildWithReport
    {
        /// <inheritdoc />
        public int callbackOrder => -1000;

        /// <inheritdoc />
        public void OnPreprocessBuild(BuildReport report)
        {
            GameDataVariantBuildSettings.Apply(GameDataVariantEditorState.SelectedVariant);
        }
    }
}
