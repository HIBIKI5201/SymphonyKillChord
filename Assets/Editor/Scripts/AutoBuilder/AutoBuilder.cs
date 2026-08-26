using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Build.Profile;
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
            string selectedProfiles = GetCliArg("-selectedProfiles");
            PerformMultipleBuilds(isBatchMode: true, buildMode: buildMode, selectedProfiles: selectedProfiles);
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
        /// <param name="selectedProfiles"> カンマ区切りのプロファイル名。指定時は該当名のみへさらに絞り込む。null または空文字時は絞り込みなし。 </param>
        private static void PerformMultipleBuilds(bool isBatchMode = false, string buildMode = null, string selectedProfiles = null)
        {
            Debug.Log(
                $"[{nameof(AutoBuilder)}] Starting multiple builds process via BuildProfile. BuildMode: {buildMode ?? "All"}");

            AutoBuilderSettings settings = AutoBuilderSettings.instance;
            if (settings == null)
            {
                Debug.LogError($"[{nameof(AutoBuilder)}] AutoBuilderSettings not found.");
                AutoBuildExecuter.ExitIfBatchMode(isBatchMode, exitCode: 1);
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
                    AutoBuildExecuter.ExitIfBatchMode(isBatchMode, exitCode: 1);
                    return;
            }

            if (profiles == null || profiles.Length == 0)
            {
                Debug.LogError($"[{nameof(AutoBuilder)}] No build profiles found for buildMode: {buildMode ?? "All"}");
                AutoBuildExecuter.ExitIfBatchMode(isBatchMode, exitCode: 1);
                return;
            }
            
            profiles = profiles.Where(profile => profile != null).ToArray();

            // -selectedProfiles 指定時はプロファイル名で一致するものだけに絞り込む。
            // 未指定・空文字時は buildMode の結果をそのまま使う（従来動作を維持）。
            if (!string.IsNullOrWhiteSpace(selectedProfiles))
            {
                string[] requestedNames = selectedProfiles
                    .Split(',')
                    .Select(name => name.Trim())
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToArray();

                HashSet<string> availableNames = profiles.Select(p => p.name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                foreach (string requestedName in requestedNames)
                {
                    if (!availableNames.Contains(requestedName))
                    {
                        Debug.LogWarning($"[{nameof(AutoBuilder)}] Requested profile not found in buildMode '{buildMode ?? "All"}': {requestedName}");
                    }
                }

                profiles = profiles
                    .Where(profile => requestedNames.Contains(profile.name, StringComparer.OrdinalIgnoreCase))
                    .ToArray();

                if (profiles.Length == 0)
                {
                    Debug.LogError($"[{nameof(AutoBuilder)}] selectedProfiles matched no profiles: {selectedProfiles}");
                    AutoBuildExecuter.ExitIfBatchMode(isBatchMode, exitCode: 1);
                    return;
                }
            }

            // 環境変数 UNITY_BUILD_OUTPUT_DIR が指定されていれば優先して使用する
            string envDir = Environment.GetEnvironmentVariable("UNITY_BUILD_OUTPUT_DIR");
            string baseOutputDir;
            if (!string.IsNullOrEmpty(envDir))
            {
                baseOutputDir = envDir;
                Debug.Log($"[{nameof(AutoBuilder)}] Using UNITY_BUILD_OUTPUT_DIR from environment: {baseOutputDir}");
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

            // 実行プロセスをAutoBuildExecuterへ委譲する。
            AutoBuildExecuter.Run(baseOutputDir, profiles, isBatchMode);
        }
    }
}