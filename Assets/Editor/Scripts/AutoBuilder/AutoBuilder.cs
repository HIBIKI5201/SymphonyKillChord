using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile; // Unity 6のBuildProfile APIを使用
using UnityEditor.Build.Reporting;
using UnityEngine;
using KillChord.Editor.AutoBuilder;

namespace DevelopProducts.AutoBuild
{
    public static class AutoBuilder
    {
        public static void PerformMultipleBuilds()
        {
            Debug.Log("Starting multiple builds process via BuildProfile...");

            var settings = AutoBuilderSettings.instance;
            if (settings == null || settings.DevelopBuildProfiles.Length == 0 ||
                settings.MasterBuildProfiles.Length == 0)
            {
                Debug.LogError($"Build settings not found or empty");
                EditorApplication.Exit(1);
                return;
            }

            bool allSuccess = true;
            var profileGUIDs = settings.DevelopBuildProfiles.Concat(settings.MasterBuildProfiles);

            foreach (BuildProfile profile in profileGUIDs)
            {
                // BuildProfileごとにビルド実行。
                Debug.Log($"Building profile: {profile.name}");

                bool success = ExecuteBuildForProfile(profile);
                if (!success)
                {
                    allSuccess = false;
                }
            }

            if (allSuccess)
            {
                Debug.Log("All builds completed successfully.");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError("One or more builds failed.");
                EditorApplication.Exit(1);
            }
        }

        private static bool ExecuteBuildForProfile(BuildProfile profile)
        {
            string outputDir = $"Builds/{profile.name}";
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            BuildPlayerOptions options = AutoBuildExecuter.CreateBuildPlayerOptions(profile);

            // 拡張子等はターゲットプラットフォームに応じてプロファイル側で設定されている前提
            string extension = AutoBuildExecuter.GetExtension(options.target);

            options.locationPathName = $"{outputDir}/{profile.name}{extension}";

            // ビルドの実行
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[Success] {profile.name} : {summary.totalSize} bytes");
                return true;
            }

            Debug.LogError($"[Failed] {profile.name} : {summary.result}");
            return false;
        }
    }
}