using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SinfoniaStudio.NotionMarkdownExporter;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     既存ページをゴミ箱へ移すコマンド。完全削除ではなく、Notion上で復元できる。
    /// </summary>
    internal static class ArchiveCommand
    {
        private static readonly string[] _valueOptions = Array.Empty<string>();
        private static readonly string[] _flagOptions = { "confirm", "with-children" };
        private static readonly string[] _repeatableOptions = Array.Empty<string>();

        /// <summary>
        ///     archiveコマンドを実行する。
        /// </summary>
        /// <param name="args">サブコマンド名を除いた引数。</param>
        /// <returns>正常終了時は0。</returns>
        internal static async Task<int> RunAsync(string[] args)
        {
            CommandArguments arguments = CommandArguments.Parse(args, _valueOptions, _flagOptions, _repeatableOptions);
            string target = arguments.GetRequiredOperand("対象ページ");
            bool isConfirmed = arguments.HasFlag("confirm");
            bool includesChildren = arguments.HasFlag("with-children");
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
            RejectAllowedRoot(environment.AllowedRootPageIds, page);

            int childPageCount = await client.CountChildPagesAsync(page.Id);

            string currentTitle = string.IsNullOrWhiteSpace(page.Title) ? "（未設定）" : page.Title;
            Console.WriteLine($"対象: {currentTitle}");
            Console.WriteLine($"URL: {page.Url}");
            Console.WriteLine($"許可ルート: {allowedRootId}");
            Console.WriteLine($"直下の子ページ・子データベース: {childPageCount}件");
            Console.WriteLine("操作: ゴミ箱へ移す（Notion上で復元できる。完全削除はしない）");

            if (childPageCount > 0 && !includesChildren)
            {
                throw new WriterException(
                    $"このページには子ページ・子データベースが{childPageCount}件あり、一緒にゴミ箱へ移ります。" +
                    "中身を確かめたうえで、移してよい場合は --with-children を付けてください。");
            }

            if (!isConfirmed)
            {
                Console.WriteLine();
                Console.WriteLine("ゴミ箱へ移していません。内容を確認し、--confirm を付けて再実行してください。");
                return 0;
            }

            await client.ArchivePageAsync(page.Id);
            Console.WriteLine($"ゴミ箱へ移しました: {page.Url}");
            return 0;
        }

        /// <summary>
        ///     書き込み許可ルートそのものを対象にした場合は拒否する。
        ///     許可ルートをゴミ箱へ移すと、その配下すべてが書き込めなくなる。
        /// </summary>
        /// <param name="allowedRootIds">書き込み許可ルートのID。</param>
        /// <param name="page">対象ページ。</param>
        private static void RejectAllowedRoot(IReadOnlyCollection<string> allowedRootIds, NotionPageInfo page)
        {
            string normalizedPageId = Normalize(page.Id);
            if (allowedRootIds.Any(rootId => Normalize(rootId) == normalizedPageId))
            {
                throw new WriterException(
                    $"書き込み許可ルート自身はゴミ箱へ移せません: {page.Title}。Notion上で操作してください。");
            }
        }

        /// <summary>
        ///     IDをハイフンなし・小文字へ揃える。
        /// </summary>
        /// <param name="id">ページID。</param>
        /// <returns>正規化したID。</returns>
        private static string Normalize(string id)
        {
            return id.Replace("-", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
        }
    }
}
