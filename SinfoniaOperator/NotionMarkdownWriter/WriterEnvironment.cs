using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SinfoniaStudio.NotionMarkdownExporter;
using SinfoniaStudio.SinfoniaOperator;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     設定ファイルと環境変数から書き込みツールの実行環境を構築するクラス。
    /// </summary>
    internal sealed class WriterEnvironment
    {
        /// <summary> pullした作業ファイルの既定の置き場。 </summary>
        private const string WORK_DIRECTORY_NAME = ".notion-work";

        private WriterEnvironment(
            string notionToken,
            IReadOnlyList<string> allowedRootPageIds,
            string exportDirectory,
            string workDirectory)
        {
            NotionToken = notionToken;
            AllowedRootPageIds = allowedRootPageIds;
            ExportDirectory = exportDirectory;
            WorkDirectory = workDirectory;
        }

        /// <summary> Notion内部インテグレーションのトークン。 </summary>
        internal string NotionToken { get; }

        /// <summary> 書き込みを許可するルートページID。この子孫だけが編集対象になる。 </summary>
        internal IReadOnlyList<string> AllowedRootPageIds { get; }

        /// <summary> Markdownエクスポートの出力先ディレクトリ。 </summary>
        internal string ExportDirectory { get; }

        /// <summary> pullした作業ファイルの既定の出力先。 </summary>
        internal string WorkDirectory { get; }

        /// <summary>
        ///     公開設定と秘密設定を読み込み、実行環境を構築する。
        /// </summary>
        /// <returns>構築した実行環境。</returns>
        internal static WriterEnvironment Load()
        {
            OperatorConfig.ClearOverrides();
            string? environmentConfigPath = FindConfigFile(OperatorConfig.ENVIRONMENT_CONFIG_FILE_NAME);
            string? secretsConfigPath = FindConfigFile(OperatorConfig.SECRETS_CONFIG_FILE_NAME);
            if (environmentConfigPath != null) { OperatorConfig.LoadJsonFile(environmentConfigPath); }
            if (secretsConfigPath != null) { OperatorConfig.LoadJsonFile(secretsConfigPath); }

            string notionToken = OperatorConfig.GetValue(OperatorConfigKeys.NOTION_TOKEN);
            if (string.IsNullOrWhiteSpace(notionToken))
            {
                throw new WriterException(
                    $"{OperatorConfigKeys.NOTION_TOKEN}を秘密設定ファイルまたは環境変数に設定してください。");
            }

            IReadOnlyList<string> allowedRootPageIds = ReadAllowedRootPageIds();
            string exportDirectory = ResolveExportDirectory(environmentConfigPath);
            string workDirectory = ResolveWorkDirectory(environmentConfigPath);
            return new WriterEnvironment(notionToken.Trim(), allowedRootPageIds, exportDirectory, workDirectory);
        }

        /// <summary>
        ///     書き込み許可ルートページIDを設定から読み取り、UUID形式へ正規化する。
        /// </summary>
        /// <returns>正規化した許可ルートページID。</returns>
        private static IReadOnlyList<string> ReadAllowedRootPageIds()
        {
            string[] rawValues = OperatorConfig.GetValues(OperatorConfigKeys.NOTION_WRITE_ALLOWED_ROOTS);
            List<string> ids = new();
            foreach (string rawValue in rawValues)
            {
                if (string.IsNullOrWhiteSpace(rawValue)) { continue; }

                if (!NotionIdentifier.TryExtract(rawValue, out string id))
                {
                    throw new WriterException(
                        $"{OperatorConfigKeys.NOTION_WRITE_ALLOWED_ROOTS}にページIDとして解釈できない値があります: {rawValue}");
                }

                if (!ids.Contains(id, StringComparer.OrdinalIgnoreCase)) { ids.Add(id); }
            }

            if (ids.Count == 0)
            {
                throw new WriterException(
                    $"{OperatorConfigKeys.NOTION_WRITE_ALLOWED_ROOTS}が空です。" +
                    "書き込みを許可するページIDを公開設定へ登録してください。");
            }

            return ids;
        }

        /// <summary>
        ///     エクスポート出力先を解決する。
        /// </summary>
        /// <param name="configBasePath">読み込んだ公開設定のパス。</param>
        /// <returns>絶対パスの出力先。</returns>
        private static string ResolveExportDirectory(string? configBasePath)
        {
            string configured = OperatorConfig.GetValue(OperatorConfigKeys.NOTION_EXPORT_OUTPUT);
            if (string.IsNullOrWhiteSpace(configured))
            {
                string? repositoryRoot = FindRepositoryRoot();
                return repositoryRoot != null
                    ? Path.Combine(repositoryRoot, "Docs", "NotionSpecifications")
                    : Path.Combine(Directory.GetCurrentDirectory(), "NotionSpecifications");
            }

            configured = Environment.ExpandEnvironmentVariables(configured);
            if (Path.IsPathRooted(configured)) { return Path.GetFullPath(configured); }

            string baseDirectory = FindRepositoryRoot() ??
                                   (configBasePath != null
                                       ? Path.GetDirectoryName(Path.GetFullPath(configBasePath))
                                       : null) ??
                                   Directory.GetCurrentDirectory();
            return Path.GetFullPath(Path.Combine(baseDirectory, configured));
        }

        /// <summary>
        ///     作業ファイルの既定の置き場を解決する。
        /// </summary>
        /// <param name="configBasePath">読み込んだ公開設定のパス。</param>
        /// <returns>絶対パスの作業ディレクトリ。</returns>
        private static string ResolveWorkDirectory(string? configBasePath)
        {
            string baseDirectory = configBasePath != null
                ? Path.GetDirectoryName(Path.GetFullPath(configBasePath)) ?? Directory.GetCurrentDirectory()
                : Directory.GetCurrentDirectory();
            return Path.Combine(baseDirectory, WORK_DIRECTORY_NAME);
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
}
