using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SinfoniaStudio.NotionMarkdownExporter;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     既存ページのタイトルだけを変更するコマンド。
    /// </summary>
    internal static class RenameCommand
    {
        private static readonly string[] _valueOptions = { "title" };
        private static readonly string[] _flagOptions = { "confirm" };
        private static readonly string[] _repeatableOptions = Array.Empty<string>();

        /// <summary>
        ///     renameコマンドを実行する。
        /// </summary>
        /// <param name="args">サブコマンド名を除いた引数。</param>
        /// <returns>正常終了時は0。</returns>
        internal static async Task<int> RunAsync(string[] args)
        {
            CommandArguments arguments = CommandArguments.Parse(args, _valueOptions, _flagOptions, _repeatableOptions);
            string target = arguments.GetRequiredOperand("対象ページ");
            string? title = arguments.GetValue("title");
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new WriterException("--title で新しいページ名を指定してください。");
            }

            bool isConfirmed = arguments.HasFlag("confirm");
            WriterEnvironment environment = WriterEnvironment.Load();
            string pageId = LocalPageLocator.ResolvePageId(target, out string? markdownPath);

            using NotionWriteClient client = new(environment.NotionToken);
            WriteScopeGuard guard = new(environment.AllowedRootPageIds, client);

            NotionPageInfo page = await client.GetPageAsync(pageId);

            if (markdownPath != null)
            {
                IReadOnlyList<string> ancestors =
                    LocalPageLocator.EnumerateAncestorPageIds(markdownPath, environment.ExportDirectory);
                guard.RejectByLocalMirror(pageId, ancestors);
            }

            string allowedRootId = await guard.AuthorizeEditAsync(page);

            string currentTitle = string.IsNullOrWhiteSpace(page.Title) ? "（未設定）" : page.Title;
            Console.WriteLine($"対象: {currentTitle}");
            Console.WriteLine($"URL: {page.Url}");
            Console.WriteLine($"許可ルート: {allowedRootId}");
            Console.WriteLine($"新しいページ名: {title}");

            if (!isConfirmed)
            {
                Console.WriteLine();
                Console.WriteLine("変更していません。内容を確認し、--confirm を付けて再実行してください。");
                return 0;
            }

            await client.UpdatePageTitleAsync(page.Id, title);
            Console.WriteLine($"ページ名を変更しました: {page.Url}");
            return 0;
        }
    }
}
