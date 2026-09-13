using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     Build Profileに設定されたシンボルからゲームデータ種別を判定し、体験版専用シーンの登録を同期します。
    ///     ゲームデータ種別の正本はBuild Profileであり、ユーザーごとのEditor状態には依存しません。
    /// </summary>
    internal static class GameDataVariantProfiles
    {
        /// <summary> 体験版Build Profileだけに設定するコンパイルシンボルです。 </summary>
        public const string DEMO_DEFINE = "KILLCHORD_DEMO";

        /// <summary> 体験版Build Profileだけに含める終了シーンのパスです。 </summary>
        public const string DEMO_END_SCENE_PATH = "Assets/Level/Scenes/Demo/DemoEnd.unity";

        /// <summary>
        ///     Build Profileのゲームデータ種別を取得します。
        /// </summary>
        /// <param name="profile"> 判定するBuild Profileです。nullの場合はReleaseとして扱います。 </param>
        /// <returns> 判定したゲームデータ種別です。 </returns>
        public static GameDataVariant GetVariant(BuildProfile profile)
        {
            if (profile == null || profile.scriptingDefines == null)
            {
                return GameDataVariant.Release;
            }

            return profile.scriptingDefines.Contains(DEMO_DEFINE, StringComparer.Ordinal)
                ? GameDataVariant.Demo
                : GameDataVariant.Release;
        }

        /// <summary>
        ///     アクティブなBuild Profileのゲームデータ種別を取得します。
        /// </summary>
        /// <returns> 判定したゲームデータ種別です。 </returns>
        public static GameDataVariant GetActiveVariant()
        {
            return GetVariant(BuildProfile.GetActiveBuildProfile());
        }

        /// <summary>
        ///     体験版Build Profileのシーン一覧を、グローバルのシーン一覧へ終了シーンを加えた内容に揃えます。
        ///     グローバルのシーン一覧を正本にすることで、Release側へ追加したシーンの反映漏れを防ぎます。
        /// </summary>
        /// <param name="profile"> 同期するBuild Profileです。 </param>
        /// <returns> シーン一覧を更新した場合はtrueです。 </returns>
        public static bool SynchronizeDemoScenes(BuildProfile profile)
        {
            if (profile == null || GetVariant(profile) != GameDataVariant.Demo)
            {
                return false;
            }

            EditorBuildSettingsScene[] expectedScenes = CreateDemoSceneList();
            if (profile.overrideGlobalScenes && AreScenesEqual(profile.scenes, expectedScenes))
            {
                return false;
            }

            profile.overrideGlobalScenes = true;
            profile.scenes = expectedScenes;
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssetIfDirty(profile);
            return true;
        }

        /// <summary>
        ///     アクティブなBuild Profileが体験版の場合、シーン一覧を同期します。
        /// </summary>
        /// <returns> シーン一覧を更新した場合はtrueです。 </returns>
        public static bool SynchronizeActiveProfileScenes()
        {
            return SynchronizeDemoScenes(BuildProfile.GetActiveBuildProfile());
        }

        /// <summary>
        ///     グローバルのシーン一覧をもとに、終了シーンを有効化した体験版用のシーン一覧を作成します。
        /// </summary>
        /// <returns> 体験版用のシーン一覧です。 </returns>
        private static EditorBuildSettingsScene[] CreateDemoSceneList()
        {
            List<EditorBuildSettingsScene> scenes = new();
            bool hasDemoEndScene = false;

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                bool isDemoEndScene = string.Equals(scene.path, DEMO_END_SCENE_PATH, StringComparison.Ordinal);
                hasDemoEndScene |= isDemoEndScene;
                scenes.Add(new EditorBuildSettingsScene(scene.path, scene.enabled || isDemoEndScene));
            }

            if (!hasDemoEndScene && AssetDatabase.LoadAssetAtPath<SceneAsset>(DEMO_END_SCENE_PATH) != null)
            {
                scenes.Add(new EditorBuildSettingsScene(DEMO_END_SCENE_PATH, true));
            }

            return scenes.ToArray();
        }

        /// <summary>
        ///     二つのシーン一覧が、順序・パス・有効状態のすべてで一致するか判定します。
        /// </summary>
        /// <param name="left"> 比較する左側のシーン一覧です。 </param>
        /// <param name="right"> 比較する右側のシーン一覧です。 </param>
        /// <returns> 一致する場合はtrueです。 </returns>
        private static bool AreScenesEqual(
            IReadOnlyList<EditorBuildSettingsScene> left,
            IReadOnlyList<EditorBuildSettingsScene> right)
        {
            if (left == null || right == null || left.Count != right.Count)
            {
                return false;
            }

            for (int i = 0; i < left.Count; i++)
            {
                if (!string.Equals(left[i].path, right[i].path, StringComparison.Ordinal)
                    || left[i].enabled != right[i].enabled)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
