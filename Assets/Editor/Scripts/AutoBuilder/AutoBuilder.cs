using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KillChord.Editor.AutoBuilder
{
    /// <summary>
    ///     コマンドラインから複数のビルドプロファイルを順番にビルドします。
    /// </summary>
    public static class AutoBuilder
    {
        /// <summary>
        /// 【GitHub Actions 用エントリポイント】
        /// Unity -batchMode -executeMethod KillChord.Editor.AutoBuilder.AutoBuilder.RunFromCli
        /// </summary>
        public static void RunFromCli()
        {
            string buildMode = GetCliArg("-buildMode");
            PerformMultipleBuilds(isBatchMode: true, buildMode: buildMode);
        }

        /// <summary>
        ///     コマンドライン引数から指定された値を取得します。
        /// </summary>
        /// <param name="name">取得したいコマンドライン引数の名前</param>
        /// <returns>指定されたコマンドライン引数の値。存在しない場合は null を返します。</returns>
        private static string GetCliArg(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name)
                {
                    return args[i + 1];
                }
            }
            return null;
        }
        
        /// <summary>
        ///     複数のビルドプロファイルに基づいてビルドを実行する。
        /// </summary>
        /// <param name="isBatchMode"> true の場合、バッチモードでの実行と判定し、ビルド完了後にエディタを終了する。false の場合は手動実行扱い。 </param>
        /// <param name="buildMode"> "Development" または "Master" を指定した場合、該当プロファイルのみビルドする。null または未指定時は両方をビルドする。 </param>
        public static void PerformMultipleBuilds(bool isBatchMode = false, string buildMode = null)
        {
            Debug.Log($"[{nameof(AutoBuilder)}] Starting multiple builds process via BuildProfile. BuildMode: {buildMode ?? "All"}");

            AutoBuilderSettings settings = AutoBuilderSettings.instance;
            if (settings == null)
            {
                Debug.LogError($"[{nameof(AutoBuilder)}] AutoBuilderSettings not found.");
                ExitIfBatchMode(isBatchMode, exitCode: 1);
                return;
            }

            BuildProfile[] profiles;
            switch (buildMode)
            {
                case "Development":
                    profiles = settings.DevelopBuildProfiles;
                    break;
                case "Master":
                    profiles = settings.MasterBuildProfiles;
                    break;
                case null:
                case "":
                    profiles = (settings.DevelopBuildProfiles ?? Array.Empty<BuildProfile>())
                        .Concat(settings.MasterBuildProfiles ?? Array.Empty<BuildProfile>())
                        .ToArray();
                    break;
                default:
                    Debug.LogError($"[{nameof(AutoBuilder)}] Unknown buildMode: {buildMode}");
                    ExitIfBatchMode(isBatchMode, exitCode: 1);
                    return;
            }

            if (profiles == null || profiles.Length == 0)
            {
                Debug.LogError($"[{nameof(AutoBuilder)}] No build profiles found for buildMode: {buildMode ?? "All"}");
                ExitIfBatchMode(isBatchMode, exitCode: 1);
                return;
            }

            BuildProfile originalProfile = BuildProfile.GetActiveBuildProfile();
            bool allSuccess = true;

            try
            {
                for (int i = 0; i < profiles.Length; i++)
                {
                    BuildProfile profile = profiles[i];
                    if (profile == null)
                    {
                        Debug.LogError($"[{nameof(AutoBuilder)}] BuildProfile is null. Index: {i}");
                        allSuccess = false;
                        continue;
                    }

                    ShowProgress(isBatchMode, profile.name, i, profiles.Length);
                    Debug.Log($"[{nameof(AutoBuilder)}] Building profile: {profile.name}");

                    if (!ExecuteBuildForProfile(profile))
                    {
                        allSuccess = false;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                allSuccess = false;
            }
            finally
            {
                if (!Application.isBatchMode && !isBatchMode)
                {
                    EditorUtility.ClearProgressBar();
                }

                if (!TryRestoreBuildProfile(originalProfile))
                {
                    allSuccess = false;
                }
            }

            if (allSuccess)
            {
                Debug.Log($"[{nameof(AutoBuilder)}] All builds completed successfully.");
                ExitIfBatchMode(isBatchMode, exitCode: 0);
            }
            else
            {
                Debug.LogError($"[{nameof(AutoBuilder)}] One or more builds failed.");
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
            // 環境変数 UNITY_BUILD_OUTPUT_DIR が指定されていれば優先して使用する
            string envDir = Environment.GetEnvironmentVariable("UNITY_BUILD_OUTPUT_DIR");
            string baseOutputDir;
            if (!string.IsNullOrEmpty(envDir))
            {
                baseOutputDir = envDir;
                Debug.Log($"[AutoBuilder] Using UNITY_BUILD_OUTPUT_DIR from environment: {baseOutputDir}");
            }
            else
            {
                baseOutputDir = Path.Combine(Application.dataPath, "../Builds");
            }

            // 相対パスが指定されている場合はプロジェクトルート基準で絶対化する
            if (!Path.IsPathRooted(baseOutputDir))
            {
                baseOutputDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), baseOutputDir));
            }

            string outputDir = Path.Combine(baseOutputDir, profile.name);
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }
            
            Directory.CreateDirectory(outputDir);

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

        /// <summary>
        ///     エディタ実行時に現在の自動ビルド進捗を表示します。
        /// </summary>
        /// <param name="isBatchMode"> バッチモード実行の場合はtrueです。 </param>
        /// <param name="profileName"> ビルド対象プロファイル名です。 </param>
        /// <param name="currentIndex"> 現在のプロファイル位置です。 </param>
        /// <param name="profileCount"> 全プロファイル数です。 </param>
        private static void ShowProgress(bool isBatchMode, string profileName, int currentIndex, int profileCount)
        {
            if (Application.isBatchMode || isBatchMode)
            {
                return;
            }

            float progress = profileCount > 0
                ? (float)currentIndex / profileCount
                : 0f;
            EditorUtility.DisplayProgressBar(
                "AutoBuilder",
                $"自動ビルドを実行中です。 ({currentIndex + 1}/{profileCount})\n{profileName}",
                progress);
        }

        /// <summary>
        ///     自動ビルド開始前のビルドプロファイルへ戻します。
        /// </summary>
        /// <param name="originalProfile"> 自動ビルド開始前のプロファイルです。 </param>
        /// <returns> 復元できた場合はtrueです。 </returns>
        private static bool TryRestoreBuildProfile(BuildProfile originalProfile)
        {
            try
            {
                if (BuildProfile.GetActiveBuildProfile() != originalProfile)
                {
                    BuildProfile.SetActiveBuildProfile(originalProfile);
                }

                Debug.Log($"[{nameof(AutoBuilder)}] Restored the original build profile.");
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[{nameof(AutoBuilder)}] Failed to restore the original build profile. {exception}");
                return false;
            }
        }
    }
}
