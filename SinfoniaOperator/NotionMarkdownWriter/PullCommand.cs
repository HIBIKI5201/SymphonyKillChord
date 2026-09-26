using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SinfoniaStudio.NotionMarkdownExporter;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     編集の基準となるMarkdown原文を取得するコマンド。
    /// </summary>
    internal static class PullCommand
    {
        private static readonly string[] _valueOptions = { "out" };
        private static readonly string[] _flagOptions = Array.Empty<string>();

        /// <summary>
        ///     pullコマンドを実行する。
        /// </summary>
        /// <param name="args">サブコマンド名を除いた引数。</param>
        /// <returns>正常終了時は0。</returns>
        internal static async Task<int> RunAsync(string[] args)
        {
            CommandArguments arguments = CommandArguments.Parse(args, _valueOptions, _flagOptions);
            string target = arguments.GetRequiredOperand("対象ページ（エクスポート済みMarkdownのパス、URL、またはID）");
            WriterEnvironment environment = WriterEnvironment.Load();
            string pageId = LocalPageLocator.ResolvePageId(target, out string? localMarkdownPath);

            using NotionWriteClient client = new(environment.NotionToken);
            WriteScopeGuard guard = new(environment.AllowedRootPageIds, client);

            // ローカルの構造で判定できる場合は、API呼び出し前に範囲外を弾く。
            if (localMarkdownPath != null)
            {
                IReadOnlyList<string> ancestors =
                    LocalPageLocator.EnumerateAncestorPageIds(localMarkdownPath, environment.ExportDirectory);
                guard.RejectByLocalMirror(pageId, ancestors);
            }

            NotionPageInfo page = await client.GetPageAsync(pageId);
            string markdown = MarkdownDiffBuilder.Normalize(await client.GetMarkdownAsync(pageId));

            string workFilePath = ResolveWorkFilePath(arguments.GetValue("out"), environment, page);
            string? workDirectory = Path.GetDirectoryName(workFilePath);
            if (!string.IsNullOrEmpty(workDirectory)) { Directory.CreateDirectory(workDirectory); }
            await File.WriteAllTextAsync(workFilePath, markdown, new UTF8Encoding(false));

            PullSnapshot snapshot = new()
            {
                PageId = page.Id,
                PageUrl = page.Url,
                PageTitle = page.Title,
                LastEditedTime = page.LastEditedTime,
                PulledAtUtc = DateTimeOffset.UtcNow,
                Baseline = markdown
            };
            snapshot.Save(workFilePath);

            Console.WriteLine($"ページ: {page.Title}");
            Console.WriteLine($"最終更新: {page.LastEditedTime}");
            Console.WriteLine($"作業ファイル: {workFilePath}");
            Console.WriteLine("このファイルを編集したあと、push で反映してください。");
            return 0;
        }

        /// <summary>
        ///     作業ファイルの出力先を決定する。
        /// </summary>
        /// <param name="outputArgument">--outで指定された値。</param>
        /// <param name="environment">実行環境。</param>
        /// <param name="page">対象ページ。</param>
        /// <returns>作業ファイルの絶対パス。</returns>
        private static string ResolveWorkFilePath(
            string? outputArgument,
            WriterEnvironment environment,
            NotionPageInfo page)
        {
            if (!string.IsNullOrWhiteSpace(outputArgument)) { return Path.GetFullPath(outputArgument); }

            string shortId = NotionIdentifier.ToShortId(page.Id);
            string fileName = $"{shortId}-{CreateSafeFileName(page.Title)}.md";
            return Path.Combine(environment.WorkDirectory, fileName);
        }

        /// <summary>
        ///     ページタイトルをファイル名に使える文字列へ変換する。
        /// </summary>
        /// <param name="title">ページタイトル。</param>
        /// <returns>ファイル名に使える文字列。</returns>
        private static string CreateSafeFileName(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) { return "page"; }

            StringBuilder builder = new();
            foreach (char character in title.Trim())
            {
                builder.Append(Array.IndexOf(Path.GetInvalidFileNameChars(), character) >= 0 ? '_' : character);
            }

            string safeName = builder.ToString();
            return safeName.Length > 40 ? safeName[..40] : safeName;
        }
    }
}
