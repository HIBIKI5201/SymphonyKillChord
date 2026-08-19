using SinfoniaStudio.SinfoniaOperator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SinfoniaStudio.DiscordLogExporter
{
    /// <summary>
    ///     Discordログエクスポートの実行設定を保持するクラス。
    /// </summary>
    internal sealed class ExporterOptions
    {
        private ExporterOptions(string discordBotToken, IReadOnlyList<ulong> channelIds, string outputDirectory)
        {
            DiscordBotToken = discordBotToken;
            ChannelIds = channelIds;
            OutputDirectory = outputDirectory;
        }

        internal string DiscordBotToken { get; }
        internal IReadOnlyList<ulong> ChannelIds { get; }
        internal string OutputDirectory { get; }

        /// <summary>
        ///     コマンドライン、設定ファイル、環境変数から実行設定を構築する。
        /// </summary>
        /// <param name="args">コマンドライン引数。</param>
        /// <returns>設定の構築結果。</returns>
        internal static ExporterOptionsResult Create(string[] args)
        {
            string? configPath = null;
            for (int index = 0; index < args.Length; index++)
            {
                string argument = args[index];
                if (string.Equals(argument, "--help", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Length != 1)
                    {
                        return ExporterOptionsResult.Failure("--helpと他のオプションは同時に指定できません。", false);
                    }

                    return ExporterOptionsResult.Help();
                }

                if (!string.Equals(argument, "--config", StringComparison.OrdinalIgnoreCase))
                {
                    return ExporterOptionsResult.Failure($"不明なオプションです: {argument}", false);
                }

                if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
                {
                    return ExporterOptionsResult.Failure("--configの値が指定されていません。", false);
                }

                if (configPath != null)
                {
                    return ExporterOptionsResult.Failure("--configは1回だけ指定できます。", false);
                }

                configPath = args[++index];
            }

            bool isInteractive = args.Length == 0 && !Console.IsInputRedirected;
            OperatorConfig.ClearOverrides();
            try
            {
                string? loadError = LoadConfig(configPath);
                if (loadError != null)
                {
                    return ExporterOptionsResult.Failure(loadError, isInteractive);
                }
            }
            catch (Exception ex)
            {
                return ExporterOptionsResult.Failure($"設定ファイルを読み込めませんでした: {ex.Message}", isInteractive);
            }

            string token = OperatorConfig.GetValue(OperatorConfigKeys.DISCORD_BOT_TOKEN);
            if (string.IsNullOrWhiteSpace(token))
            {
                return ExporterOptionsResult.Failure(
                    $"{OperatorConfigKeys.DISCORD_BOT_TOKEN}を秘密設定ファイルまたは環境変数に設定してください。",
                    isInteractive);
            }

            string[] channelIdValues = OperatorConfig.GetValues(OperatorConfigKeys.DISCORD_LOG_CHANNEL_IDS);
            List<ulong> channelIds = new();
            foreach (string value in channelIdValues)
            {
                if (!ulong.TryParse(value, out ulong channelId) || channelId == 0)
                {
                    return ExporterOptionsResult.Failure(
                        $"{OperatorConfigKeys.DISCORD_LOG_CHANNEL_IDS}に無効なチャンネルIDがあります: {value}",
                        isInteractive);
                }

                if (!channelIds.Contains(channelId))
                {
                    channelIds.Add(channelId);
                }
            }

            if (channelIds.Count == 0)
            {
                return ExporterOptionsResult.Failure(
                    $"{OperatorConfigKeys.DISCORD_LOG_CHANNEL_IDS}に取得対象のチャンネルIDを設定してください。",
                    isInteractive);
            }

            string? repositoryRoot = FindRepositoryRoot();
            if (repositoryRoot == null)
            {
                return ExporterOptionsResult.Failure("Gitリポジトリのルートを特定できませんでした。", isInteractive);
            }

            string outputDirectory = Path.Combine(repositoryRoot, "Docs", "DiscordLog");
            ExporterOptions options = new(token.Trim(), channelIds, outputDirectory);
            return ExporterOptionsResult.Success(options, isInteractive);
        }

        /// <summary>
        ///     コマンドラインの使用方法を標準出力へ表示する。
        /// </summary>
        internal static void WriteHelp()
        {
            Console.WriteLine("使用方法:");
            Console.WriteLine("  DiscordLogExporter.exe [オプション]");
            Console.WriteLine();
            Console.WriteLine("オプション:");
            Console.WriteLine("  --config <PATH>     明示的に読み込むJSON設定ファイル。");
            Console.WriteLine("  --help              このヘルプを表示する。");
            Console.WriteLine();
            Console.WriteLine("設定キー:");
            Console.WriteLine($"  {OperatorConfigKeys.DISCORD_BOT_TOKEN}       必須。秘密設定または環境変数に置くBotトークン。");
            Console.WriteLine($"  {OperatorConfigKeys.DISCORD_LOG_CHANNEL_IDS}  必須。取得対象のチャンネルID配列。");
            Console.WriteLine();
            Console.WriteLine("出力先はプロジェクトのDocs\\DiscordLogに固定されています。");
        }

        /// <summary>
        ///     明示設定、または既定位置の公開・秘密設定を読み込む。
        /// </summary>
        /// <param name="explicitConfigPath">明示された設定ファイル。未指定の場合はnull。</param>
        /// <returns>読み込みに失敗した場合はエラーメッセージ、それ以外はnull。</returns>
        private static string? LoadConfig(string? explicitConfigPath)
        {
            if (explicitConfigPath != null)
            {
                if (!OperatorConfig.LoadJsonFile(explicitConfigPath))
                {
                    return $"設定ファイルが見つかりません: {explicitConfigPath}";
                }

                if (string.Equals(
                        Path.GetFileName(explicitConfigPath),
                        OperatorConfig.ENVIRONMENT_CONFIG_FILE_NAME,
                        StringComparison.OrdinalIgnoreCase))
                {
                    string directory = Path.GetDirectoryName(Path.GetFullPath(explicitConfigPath)) ??
                                       Directory.GetCurrentDirectory();
                    OperatorConfig.LoadJsonFile(Path.Combine(directory, OperatorConfig.SECRETS_CONFIG_FILE_NAME));
                }

                return null;
            }

            string? environmentConfigPath = FindConfigFile(OperatorConfig.ENVIRONMENT_CONFIG_FILE_NAME);
            string? secretsConfigPath = FindConfigFile(OperatorConfig.SECRETS_CONFIG_FILE_NAME);
            string? legacyConfigPath = FindConfigFile(OperatorConfig.LEGACY_CONFIG_FILE_NAME);
            if (environmentConfigPath != null)
            {
                OperatorConfig.LoadJsonFile(environmentConfigPath);
            }

            if (secretsConfigPath != null)
            {
                OperatorConfig.LoadJsonFile(secretsConfigPath);
            }
            else if (legacyConfigPath != null)
            {
                if (environmentConfigPath == null)
                {
                    OperatorConfig.LoadJsonFile(legacyConfigPath);
                }
                else
                {
                    OperatorConfig.LoadJsonFile(legacyConfigPath, OperatorConfigKeys.DISCORD_BOT_TOKEN);
                }
            }

            return null;
        }

        /// <summary>
        ///     カレントディレクトリと実行ファイル位置の祖先から設定ファイルを探す。
        /// </summary>
        /// <param name="fileName">設定ファイル名。</param>
        /// <returns>見つかった設定ファイルのパス。見つからない場合はnull。</returns>
        private static string? FindConfigFile(string fileName)
        {
            foreach (string startDirectory in GetSearchStartDirectories())
            {
                DirectoryInfo? current = new(Path.GetFullPath(startDirectory));
                while (current != null)
                {
                    string candidate = Path.Combine(current.FullName, fileName);
                    if (File.Exists(candidate)) { return candidate; }
                    current = current.Parent;
                }
            }

            return null;
        }

        /// <summary>
        ///     カレントディレクトリと実行ファイル位置の祖先からGitリポジトリのルートを探す。
        /// </summary>
        /// <returns>リポジトリルート。見つからない場合はnull。</returns>
        private static string? FindRepositoryRoot()
        {
            foreach (string startDirectory in GetSearchStartDirectories())
            {
                DirectoryInfo? current = new(Path.GetFullPath(startDirectory));
                while (current != null)
                {
                    if (Directory.Exists(Path.Combine(current.FullName, ".git"))) { return current.FullName; }
                    current = current.Parent;
                }
            }

            return null;
        }

        /// <summary>
        ///     設定やリポジトリを探索する開始ディレクトリを列挙する。
        /// </summary>
        /// <returns>重複を除外したディレクトリ一覧。</returns>
        private static IEnumerable<string> GetSearchStartDirectories()
        {
            return new[]
            {
                Directory.GetCurrentDirectory(),
                AppContext.BaseDirectory
            }.Distinct(StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    ///     実行設定の構築結果を保持するクラス。
    /// </summary>
    internal sealed class ExporterOptionsResult
    {
        private ExporterOptionsResult(
            ExporterOptions? options,
            string errorMessage,
            bool isHelpRequested,
            bool isInteractive)
        {
            Options = options;
            ErrorMessage = errorMessage;
            IsHelpRequested = isHelpRequested;
            IsInteractive = isInteractive;
        }

        internal ExporterOptions? Options { get; }
        internal string ErrorMessage { get; }
        internal bool IsHelpRequested { get; }
        internal bool IsInteractive { get; }

        /// <summary>
        ///     設定構築に成功した結果を生成する。
        /// </summary>
        /// <param name="options">構築した設定。</param>
        /// <param name="isInteractive">対話実行かどうか。</param>
        /// <returns>成功結果。</returns>
        internal static ExporterOptionsResult Success(ExporterOptions options, bool isInteractive)
        {
            return new ExporterOptionsResult(options, string.Empty, false, isInteractive);
        }

        /// <summary>
        ///     設定構築に失敗した結果を生成する。
        /// </summary>
        /// <param name="message">エラーメッセージ。</param>
        /// <param name="isInteractive">対話実行かどうか。</param>
        /// <returns>失敗結果。</returns>
        internal static ExporterOptionsResult Failure(string message, bool isInteractive)
        {
            return new ExporterOptionsResult(null, message, false, isInteractive);
        }

        /// <summary>
        ///     ヘルプ表示を要求する結果を生成する。
        /// </summary>
        /// <returns>ヘルプ表示結果。</returns>
        internal static ExporterOptionsResult Help()
        {
            return new ExporterOptionsResult(null, string.Empty, true, false);
        }
    }
}
