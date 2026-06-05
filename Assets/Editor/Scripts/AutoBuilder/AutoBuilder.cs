using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile; // Unity 6のBuildProfile APIを使用
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KillChord.Editor.AutoBuilder
{
    public static class AutoBuilder
    {
        /// <summary>
        ///     複数のビルドプロファイルに基づいてビルドを実行する。
        /// </summary>
        /// <param name="isBatchMode">
        ///     true の場合、バッチモードでの実行と判定し、ビルド完了後にエディタを終了する。
        ///     false の場合は手動実行扱い。
        /// </param>
        public static void PerformMultipleBuilds(bool isBatchMode = false)
        {
            Debug.Log("Starting multiple builds process via BuildProfile...");

            var settings = AutoBuilderSettings.instance;
            if (settings == null || settings.DevelopBuildProfiles.Length == 0 ||
                settings.MasterBuildProfiles.Length == 0)
            {
                Debug.LogError($"Build settings not found or empty");
                ExitIfBatchMode(isBatchMode, exitCode: 1);
                return;
            }

            bool allSuccess = true;
            var allProfiles = settings.DevelopBuildProfiles.Concat(settings.MasterBuildProfiles);

            foreach (BuildProfile profile in allProfiles)
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
                ExitIfBatchMode(isBatchMode, exitCode: 0);
            }
            else
            {
                Debug.LogError("One or more builds failed.");
                ExitIfBatchMode(isBatchMode, exitCode: 1);
            }
        }

        /// <summary>
        ///     バッチモード、または isBatchMode が true の場合のみ、エディタプロセスを指定されたコードで終了する。
        ///     これにより、手動での CI/CD トリガー時のエディタ強制終了を回避できる。
        /// </summary>
        /// <param name="isBatchMode">強制的にバッチモード判定を行うかどうか（通常は Application.isBatchMode と組み合わせる）</param>
        /// <param name="exitCode">終了コード（0: 成功、1: 失敗）</param>
        private static void ExitIfBatchMode(bool isBatchMode, int exitCode)
        {
            bool shouldExit = Application.isBatchMode || isBatchMode;
            if (shouldExit)
            {
                Debug.Log($"Exiting Unity editor with code {exitCode} (Batch Mode)");
                EditorApplication.Exit(exitCode);
            }
        }

        /// <summary>
        /// 指定されたビルドプロファイルに基づいてビルドを実行し、結果を返す。
        /// </summary>
        /// <param name="profile">ビルドプロファイル</param>
        /// <returns>ビルド成功時は true、失敗時は false</returns>
        private static bool ExecuteBuildForProfile(BuildProfile profile)
        {
            string outputDir = Path.Combine(Application.dataPath, "../Builds", profile.name);
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            BuildPlayerOptions options = AutoBuildExecuter.CreateBuildPlayerOptions(profile);

            // 拡張子等はターゲットプラットフォームに応じてプロファイル側で設定されている前提
            string extension = AutoBuildExecuter.GetExtension(options.target);
            options.locationPathName = Path.Combine(outputDir, $"{profile.name}{extension}");


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