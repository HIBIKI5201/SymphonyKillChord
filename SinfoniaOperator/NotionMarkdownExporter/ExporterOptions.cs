using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SinfoniaStudio.SinfoniaOperator;

namespace SinfoniaStudio.NotionMarkdownExporter
{
    /// <summary>
    ///     エクスポートの実行設定を保持するクラス。
    /// </summary>
    internal sealed class ExporterOptions
    {
        private const string DEFAULT_CONFIG_FILE = "sinfonia-operator.settings.json";

        private ExporterOptions(
            string notionToken,
            string rootPageId,
            string outputDirectory,
            bool downloadsAssets)
        {
            NotionToken = notionToken;
            RootPageId = rootPageId;
            OutputDirectory = outputDirectory;
            DownloadsAssets = downloadsAssets;
        }

        internal string NotionToken { get; }
        internal string RootPageId { get; }
        internal string OutputDirectory { get; }
        internal bool DownloadsAssets { get; }

        /// <summary>
        ///     コマンドライン、設定ファイル、環境変数、対話入力から実行設定を構築する。
        /// </summary>
        /// <param name="args">コマンドライン引数。</param>
        /// <returns>設定の構築結果。</returns>
        internal static ExporterOptionsResult Create(string[] args)
        {
            Dictionary<string, string?> arguments = new(StringComparer.OrdinalIgnoreCase);
            string? parseError = ParseArguments(args, arguments);
            if (parseError != null)
            {
                return ExporterOptionsResult.Failure(parseError, false);
            }

            if (arguments.ContainsKey("help"))
            {
                return ExporterOptionsResult.Help();
            }

            string? explicitConfigPath = GetArgument(arguments, "config");
            string? configPath = explicitConfigPath ?? FindConfigFile();
            if (explicitConfigPath != null && !File.Exists(explicitConfigPath))
            {
                return ExporterOptionsResult.Failure($"設定ファイルが見つかりません: {explicitConfigPath}", false);
            }

            if (configPath != null)
            {
                try
                {
                    OperatorConfig.ClearOverrides();
                    if (!OperatorConfig.LoadJsonFile(configPath))
                    {
                        return ExporterOptionsResult.Failure($"設定ファイルが見つかりません: {configPath}", false);
                    }
                }
                catch (Exception ex)
                {
                    return ExporterOptionsResult.Failure($"設定ファイルを読み込めませんでした: {ex.Message}", false);
                }
            }

            bool isInteractive = args.Length == 0 && !Console.IsInputRedirected;
            string notionToken = OperatorConfig.GetValue(OperatorConfigKeys.NOTION_TOKEN);
            if (string.IsNullOrWhiteSpace(notionToken))
            {
                return ExporterOptionsResult.Failure(
                    $"{OperatorConfigKeys.NOTION_TOKEN}を共有設定ファイルまたは環境変数に設定してください。",
                    isInteractive);
            }

            string rootPage = GetArgument(arguments, "root") ??
                              OperatorConfig.GetValue(OperatorConfigKeys.NOTION_EXPORT_ROOT_PAGE);
            if (string.IsNullOrWhiteSpace(rootPage))
            {
                if (Console.IsInputRedirected)
                {
                    return ExporterOptionsResult.Failure("--rootでルートページURLまたはIDを指定してください。", false);
                }

                isInteractive = true;
                Console.Write("ルートページのURLまたはIDを入力してください: ");
                rootPage = Console.ReadLine() ?? string.Empty;
            }

            if (!NotionIdentifier.TryExtract(rootPage, out string rootPageId))
            {
                return ExporterOptionsResult.Failure("ルートページのURLまたはIDを認識できませんでした。", isInteractive);
            }

            string defaultOutput = FindDefaultOutputDirectory();
            string? outputArgument = GetArgument(arguments, "output");
            string outputDirectory = outputArgument ??
                                     OperatorConfig.GetValue(OperatorConfigKeys.NOTION_EXPORT_OUTPUT);
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                if (!Console.IsInputRedirected && args.Length == 0)
                {
                    isInteractive = true;
                    Console.Write($"出力先（Enterで既定値: {defaultOutput}）: ");
                    outputDirectory = Console.ReadLine() ?? string.Empty;
                }

                if (string.IsNullOrWhiteSpace(outputDirectory))
                {
                    outputDirectory = defaultOutput;
                }
            }

            outputDirectory = Environment.ExpandEnvironmentVariables(outputDirectory);
            if (!Path.IsPathRooted(outputDirectory) && outputArgument == null && configPath != null)
            {
                string baseDirectory = FindRepositoryRoot() ??
                                       Path.GetDirectoryName(Path.GetFullPath(configPath)) ??
                                       Directory.GetCurrentDirectory();
                outputDirectory = Path.Combine(baseDirectory, outputDirectory);
            }
            outputDirectory = Path.GetFullPath(outputDirectory);
            bool downloadsAssets = !arguments.ContainsKey("no-assets");
            ExporterOptions options = new(notionToken.Trim(), rootPageId, outputDirectory, downloadsAssets);
            return ExporterOptionsResult.Success(options, isInteractive);
        }

        /// <summary>
        ///     コマンドラインの使用方法を標準出力へ表示する。
        /// </summary>
        internal static void WriteHelp()
        {
            Console.WriteLine("使用方法:");
            Console.WriteLine("  NotionMarkdownExporter.exe [オプション]");
            Console.WriteLine();
            Console.WriteLine("オプション:");
            Console.WriteLine("  --root <URL|ID>     エクスポートするルートページ。");
            Console.WriteLine("  --output <PATH>     出力先。既定値はプロジェクトのDocs\\NotionSpecifications。");
            Console.WriteLine("  --config <PATH>     JSON設定ファイル。");
            Console.WriteLine("  --no-assets         画像や添付ファイルをダウンロードしない。");
            Console.WriteLine("  --help              このヘルプを表示する。");
            Console.WriteLine();
            Console.WriteLine("設定キー:");
            Console.WriteLine($"  {OperatorConfigKeys.NOTION_TOKEN}          必須。Notion内部インテグレーションのトークン。");
            Console.WriteLine($"  {OperatorConfigKeys.NOTION_EXPORT_ROOT_PAGE}  任意。ルートページURLまたはID。");
            Console.WriteLine($"  {OperatorConfigKeys.NOTION_EXPORT_OUTPUT}      任意。出力先。");
        }

        /// <summary>
        ///     コマンドライン引数を名前と値の辞書へ変換する。
        /// </summary>
        /// <param name="args">コマンドライン引数。</param>
        /// <param name="result">解析結果の格納先。</param>
        /// <returns>解析に失敗した場合はエラーメッセージ、それ以外はnull。</returns>
        private static string? ParseArguments(string[] args, Dictionary<string, string?> result)
        {
            HashSet<string> valueOptions = new(StringComparer.OrdinalIgnoreCase)
            {
                "root",
                "output",
                "config"
            };
            HashSet<string> flagOptions = new(StringComparer.OrdinalIgnoreCase)
            {
                "no-assets",
                "help"
            };

            for (int index = 0; index < args.Length; index++)
            {
                string argument = args[index];
                if (!argument.StartsWith("--", StringComparison.Ordinal))
                {
                    return $"不明な引数です: {argument}";
                }

                string name = argument[2..];
                if (flagOptions.Contains(name))
                {
                    result[name] = null;
                    continue;
                }

                if (!valueOptions.Contains(name))
                {
                    return $"不明なオプションです: {argument}";
                }

                if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
                {
                    return $"{argument}の値が指定されていません。";
                }

                result[name] = args[++index];
            }

            return null;
        }

        /// <summary>
        ///     引数辞書から指定した値を取得する。
        /// </summary>
        /// <param name="arguments">引数辞書。</param>
        /// <param name="name">引数名。</param>
        /// <returns>値があればその文字列、それ以外はnull。</returns>
        private static string? GetArgument(Dictionary<string, string?> arguments, string name)
        {
            return arguments.TryGetValue(name, out string? value) ? value : null;
        }

        /// <summary>
        ///     カレントディレクトリと実行ファイル位置の祖先から既定設定ファイルを探す。
        /// </summary>
        /// <returns>見つかった設定ファイルのパス。見つからない場合はnull。</returns>
        private static string? FindConfigFile()
        {
            foreach (string startDirectory in GetSearchStartDirectories())
            {
                DirectoryInfo? current = new(Path.GetFullPath(startDirectory));
                while (current != null)
                {
                    string candidate = Path.Combine(current.FullName, DEFAULT_CONFIG_FILE);
                    if (File.Exists(candidate)) { return candidate; }
                    current = current.Parent;
                }
            }

            return null;
        }

        /// <summary>
        ///     Gitリポジトリのルートを探し、既定の仕様書出力先を決定する。
        /// </summary>
        /// <returns>既定の出力先。</returns>
        private static string FindDefaultOutputDirectory()
        {
            string? repositoryRoot = FindRepositoryRoot();
            if (repositoryRoot != null) { return Path.Combine(repositoryRoot, "Docs", "NotionSpecifications"); }

            return Path.Combine(Directory.GetCurrentDirectory(), "NotionSpecifications");
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
        /// <param name="isInteractive">対話入力を使用したかどうか。</param>
        /// <returns>成功結果。</returns>
        internal static ExporterOptionsResult Success(ExporterOptions options, bool isInteractive)
        {
            return new ExporterOptionsResult(options, string.Empty, false, isInteractive);
        }

        /// <summary>
        ///     設定構築に失敗した結果を生成する。
        /// </summary>
        /// <param name="message">エラーメッセージ。</param>
        /// <param name="isInteractive">対話入力を使用したかどうか。</param>
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
