using KillChord.Editor.SourceDataProvider.Core;
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
        ///     -gameDataVariant release|demo|exhibition [-buildMode Development|Master] [-selectedProfiles Windows,Android]
        ///     exhibition（展示版）は Master だけを持ちます。
        /// </summary>
        public static void RunFromCli()
        {
            string buildMode = GetCliArg("-buildMode");
            string gameDataVariant = GetCliArg("-gameDataVariant");
            string selectedProfiles = GetCliArg("-selectedProfiles");

            PerformMultipleBuilds(
                isBatchMode: true,
                gameDataVariant: gameDataVariant,
                buildMode: buildMode,
                selectedProfiles: selectedProfiles);
        }

        /// <summary>
        ///     GitHub Actions がプロファイルごとにUnityプロセスを分離できるよう、
        ///     指定されたゲームデータ種別とビルドモードに登録されたプロファイル名をファイルへ出力します。
        /// </summary>
        public static void ExportProfileNamesFromCli()
        {
            // コマンドライン引数を読み取り、設定が揃っているかを確認する。
            string buildMode = GetCliArg("-buildMode");
            string gameDataVariant = GetCliArg("-gameDataVariant");
            string selectedProfiles = GetCliArg("-selectedProfiles");
            string outputPath = GetCliArg("-profileListFile");
            AutoBuilderSettings settings = AutoBuilderSettings.instance;

            if (settings == null
                || string.IsNullOrWhiteSpace(outputPath)
                || !TryParseVariant(gameDataVariant, out AutoBuildVariant variant)
                || !TryResolveModes(buildMode, out AutoBuildMode[] modes))
            {
                Debug.LogError(
                    $"[{nameof(AutoBuilder)}] Failed to export build profiles. " +
                    $"Variant: '{gameDataVariant}', BuildMode: '{buildMode}', OutputPath: '{outputPath}'");
                AutoBuildExecuter.ExitIfBatchMode(forceBatchMode: true, exitCode: 1);
                return;
            }

            // 種別とモードに合うプロファイルを集め、指定があればさらに絞り込む。
            BuildProfile[] profiles = CollectProfiles(settings, variant, modes);
            if (profiles.Length == 0 || !ValidateProfileVariants(profiles, variant))
            {
                Debug.LogError(
                    $"[{nameof(AutoBuilder)}] No valid build profiles found for variant '{variant}' / buildMode '{buildMode}'");
                AutoBuildExecuter.ExitIfBatchMode(forceBatchMode: true, exitCode: 1);
                return;
            }

            profiles = FilterProfiles(profiles, selectedProfiles, variant, buildMode);
            if (profiles.Length == 0)
            {
                Debug.LogError($"[{nameof(AutoBuilder)}] selectedProfiles matched no profiles: {selectedProfiles}");
                AutoBuildExecuter.ExitIfBatchMode(forceBatchMode: true, exitCode: 1);
                return;
            }

            // プロファイル名をファイルへ書き出して終了する。
            string[] profileNames = profiles
                .Where(profile => profile != null)
                .Select(profile => profile.name)
                .ToArray();

            string fullOutputPath = Path.GetFullPath(outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath) ?? Directory.GetCurrentDirectory());
            File.WriteAllLines(fullOutputPath, profileNames);
            Debug.Log($"[{nameof(AutoBuilder)}] Exported {profileNames.Length} build profile names to: {fullOutputPath}");
            AutoBuildExecuter.ExitIfBatchMode(forceBatchMode: true, exitCode: 0);
        }

        /// <summary> 展示版のビルドプロファイルだけに設定するコンパイルシンボルです。 </summary>
        private const string EXHIBITION_DEFINE = "KILLCHORD_DEMO_EXHIBITION";

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
        ///     種別とモードで決まる枠のビルドプロファイルを順番にビルドする。
        /// </summary>
        /// <param name="isBatchMode"> true の場合、バッチモードでの実行と判定し、ビルド完了後にエディタを終了する。 </param>
        /// <param name="gameDataVariant"> release、demo、exhibition（展示版）のいずれか。必須です。 </param>
        /// <param name="buildMode"> "Development" または "Master"。null または空文字時は種別にある Development と Master をビルドする。 </param>
        /// <param name="selectedProfiles"> カンマ区切りのプロファイル名。指定時は該当名のみへさらに絞り込む。 </param>
        private static void PerformMultipleBuilds(
            bool isBatchMode,
            string gameDataVariant,
            string buildMode,
            string selectedProfiles)
        {
            Debug.Log(
                $"[{nameof(AutoBuilder)}] Starting multiple builds process via BuildProfile. " +
                $"Variant: {gameDataVariant ?? "(none)"}, BuildMode: {buildMode ?? "All"}");

            AutoBuilderSettings settings = AutoBuilderSettings.instance;
            if (settings == null)
            {
                Debug.LogError($"[{nameof(AutoBuilder)}] AutoBuilderSettings not found.");
                AutoBuildExecuter.ExitIfBatchMode(isBatchMode, exitCode: 1);
                return;
            }

            if (!TryParseVariant(gameDataVariant, out AutoBuildVariant variant)
                || !TryResolveModes(buildMode, out AutoBuildMode[] modes))
            {
                AutoBuildExecuter.ExitIfBatchMode(isBatchMode, exitCode: 1);
                return;
            }

            BuildProfile[] profiles = CollectProfiles(settings, variant, modes);
            if (profiles.Length == 0)
            {
                Debug.LogError(
                    $"[{nameof(AutoBuilder)}] No build profiles found for variant '{variant}' / buildMode '{buildMode ?? "All"}'");
                AutoBuildExecuter.ExitIfBatchMode(isBatchMode, exitCode: 1);
                return;
            }

            if (!ValidateProfileVariants(profiles, variant))
            {
                AutoBuildExecuter.ExitIfBatchMode(isBatchMode, exitCode: 1);
                return;
            }

            profiles = FilterProfiles(profiles, selectedProfiles, variant, buildMode);
            if (profiles.Length == 0)
            {
                Debug.LogError($"[{nameof(AutoBuilder)}] selectedProfiles matched no profiles: {selectedProfiles}");
                AutoBuildExecuter.ExitIfBatchMode(isBatchMode, exitCode: 1);
                return;
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

        /// <summary>
        ///     コマンドライン引数のゲームデータ種別を解釈します。
        /// </summary>
        /// <param name="gameDataVariant"> release、demo、exhibition のいずれかです。 </param>
        /// <param name="variant"> 解釈した種別です。 </param>
        /// <returns> 解釈できた場合は true です。 </returns>
        private static bool TryParseVariant(string gameDataVariant, out AutoBuildVariant variant)
        {
            if (Enum.TryParse(gameDataVariant, ignoreCase: true, out variant)
                && Enum.IsDefined(typeof(AutoBuildVariant), variant))
            {
                return true;
            }

            Debug.LogError(
                $"[{nameof(AutoBuilder)}] Build requires -gameDataVariant release, demo or exhibition. Value: '{gameDataVariant ?? string.Empty}'");
            return false;
        }

        /// <summary>
        ///     コマンドライン引数のビルドモードを解釈します。未指定時は Development と Master を対象にします。
        /// </summary>
        /// <param name="buildMode"> Development または Master です。 </param>
        /// <param name="modes"> 対象のビルドモード一覧です。 </param>
        /// <returns> 解釈できた場合は true です。 </returns>
        private static bool TryResolveModes(string buildMode, out AutoBuildMode[] modes)
        {
            if (string.IsNullOrEmpty(buildMode))
            {
                modes = new[] { AutoBuildMode.Development, AutoBuildMode.Master };
                return true;
            }

            if (Enum.TryParse(buildMode, ignoreCase: true, out AutoBuildMode mode)
                && Enum.IsDefined(typeof(AutoBuildMode), mode))
            {
                modes = new[] { mode };
                return true;
            }

            Debug.LogError($"[{nameof(AutoBuilder)}] Unknown buildMode: {buildMode}");
            modes = Array.Empty<AutoBuildMode>();
            return false;
        }

        /// <summary>
        ///     種別とモードに対応する枠からビルドプロファイルを集めます。
        /// </summary>
        /// <param name="settings"> オートビルダー設定です。 </param>
        /// <param name="variant"> 種別です。 </param>
        /// <param name="modes"> 対象のビルドモード一覧です。 </param>
        /// <returns> 集めたビルドプロファイルです。 </returns>
        private static BuildProfile[] CollectProfiles(
            AutoBuilderSettings settings,
            AutoBuildVariant variant,
            IEnumerable<AutoBuildMode> modes)
        {
            List<BuildProfile> profiles = new();
            foreach (AutoBuildMode mode in modes)
            {
                if (!AutoBuilderSettings.TryGetSlot(variant, mode, out AutoBuildSlot slot))
                {
                    continue;
                }

                profiles.AddRange((settings.GetProfiles(slot) ?? Array.Empty<BuildProfile>())
                    .Where(profile => profile != null));
            }

            return profiles.ToArray();
        }

        /// <summary>
        ///     指定された名前に一致するビルドプロファイルへ絞り込みます。
        /// </summary>
        /// <param name="profiles"> 絞り込み対象のビルドプロファイルです。 </param>
        /// <param name="selectedProfiles"> カンマ区切りのプロファイル名です。空の場合は絞り込みません。 </param>
        /// <param name="variant"> 対象の種別です。 </param>
        /// <param name="buildMode"> 対象のビルドモードです。 </param>
        /// <returns> 名前に一致したビルドプロファイルです。 </returns>
        private static BuildProfile[] FilterProfiles(
            BuildProfile[] profiles,
            string selectedProfiles,
            AutoBuildVariant variant,
            string buildMode)
        {
            if (string.IsNullOrWhiteSpace(selectedProfiles))
            {
                return profiles;
            }

            string[] requestedNames = selectedProfiles
                .Split(',')
                .Select(name => name.Trim())
                .Where(name => !string.IsNullOrEmpty(name))
                .ToArray();

            foreach (string requestedName in requestedNames)
            {
                bool isMatched = profiles.Any(profile => IsProfileMatch(profile.name, requestedName));
                if (!isMatched)
                {
                    Debug.LogWarning(
                        $"[{nameof(AutoBuilder)}] Requested profile not found in variant '{variant}' / " +
                        $"buildMode '{buildMode ?? "All"}': {requestedName}");
                }
            }

            return profiles
                .Where(profile => requestedNames.Any(requested => IsProfileMatch(profile.name, requested)))
                .ToArray();
        }

        /// <summary>
        ///     枠へ登録されたプロファイルのシンボルが、枠の種別と一致しているか確認します。
        ///     ゲームデータ（KILLCHORD_DEMO の有無）に加え、展示版のシンボルは展示版の枠のプロファイルだけが持つことを確かめます。
        /// </summary>
        /// <param name="profiles"> 確認するビルドプロファイルです。 </param>
        /// <param name="variant"> 枠の種別です。 </param>
        /// <returns> すべて一致する場合は true です。 </returns>
        private static bool ValidateProfileVariants(IEnumerable<BuildProfile> profiles, AutoBuildVariant variant)
        {
            GameDataVariant expectedDataVariant = variant == AutoBuildVariant.Release
                ? GameDataVariant.Release
                : GameDataVariant.Demo;
            bool isExhibitionExpected = variant == AutoBuildVariant.Exhibition;

            bool isValid = true;
            foreach (BuildProfile profile in profiles)
            {
                GameDataVariant profileDataVariant = GameDataVariantProfiles.GetVariant(profile);
                bool isExhibitionProfile = profile.scriptingDefines != null
                    && profile.scriptingDefines.Contains(EXHIBITION_DEFINE, StringComparer.Ordinal);
                if (profileDataVariant == expectedDataVariant && isExhibitionProfile == isExhibitionExpected)
                {
                    continue;
                }

                string exhibitionNote = isExhibitionProfile ? " with " + EXHIBITION_DEFINE : string.Empty;
                Debug.LogError(
                    $"[{nameof(AutoBuilder)}] {profile.name} is registered in the {variant} slot, " +
                    $"but its scripting defines indicate {profileDataVariant}{exhibitionNote}.");
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        ///     文字列から英数字のみを抽出し、小文字に変換した正規化文字列を返します。
        /// </summary>
        /// <param name="input">対象文字列</param>
        /// <returns>英数字のみの小文字文字列。</returns>
        private static string NormalizeProfileName(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            char[] chars = input
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray();

            return new string(chars);
        }

        /// <summary>
        ///     プロファイル名と要求された名前が一致するかどうかを判定します。
        ///     大文字小文字の違い、記号の有無、プレフィックスや部分一致を考慮して柔軟に照合します。
        /// </summary>
        /// <param name="profileName">プロファイル名</param>
        /// <param name="requestedName">要求されたプロファイル名</param>
        /// <returns>一致する場合は true、それ以外は false。</returns>
        private static bool IsProfileMatch(string profileName, string requestedName)
        {
            if (string.Equals(profileName, requestedName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string normProfile = NormalizeProfileName(profileName);
            string normRequested = NormalizeProfileName(requestedName);

            if (string.IsNullOrEmpty(normProfile) || string.IsNullOrEmpty(normRequested))
            {
                return false;
            }

            return normProfile.Contains(normRequested) || normRequested.Contains(normProfile);
        }
    }
}
