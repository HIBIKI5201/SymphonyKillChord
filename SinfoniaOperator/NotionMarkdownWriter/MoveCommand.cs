using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SinfoniaStudio.NotionMarkdownExporter;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     既存ページをデータベースの行として移動するコマンド。
    ///     子ページとして置かれている詳細仕様を、DBの直下（行）へ移す運用（03_DB直下への移動リスト.md）のために追加した。
    ///     ページIDは移動後も変わらないため、既存のページメンション・キャッシュマップ・原稿のリンクはそのまま使える。
    /// </summary>
    internal static class MoveCommand
    {
        private static readonly string[] _valueOptions = { "to" };
        private static readonly string[] _flagOptions = { "confirm" };
        private static readonly string[] _repeatableOptions = Array.Empty<string>();

        /// <summary>
        ///     moveコマンドを実行する。
        /// </summary>
        /// <param name="args">サブコマンド名を除いた引数。</param>
        /// <returns>正常終了時は0。</returns>
        internal static async Task<int> RunAsync(string[] args)
        {
            CommandArguments arguments = CommandArguments.Parse(args, _valueOptions, _flagOptions, _repeatableOptions);
            string target = arguments.GetRequiredOperand("移動するページ");
            string? destinationArgument = arguments.GetValue("to");
            if (string.IsNullOrWhiteSpace(destinationArgument))
            {
                throw new WriterException("--to で移動先のデータベース（Markdownパス|URL|ID）を指定してください。");
            }

            bool isConfirmed = arguments.HasFlag("confirm");
            WriterEnvironment environment = WriterEnvironment.Load();
            string pageId = LocalPageLocator.ResolvePageId(target, out string? markdownPath);
            string destinationId = LocalPageLocator.ResolvePageId(destinationArgument, out string? destinationMarkdownPath);

            using NotionWriteClient client = new(environment.NotionToken);
            WriteScopeGuard guard = new(environment.AllowedRootPageIds, client);

            if (markdownPath != null)
            {
                IReadOnlyList<string> ancestors =
                    LocalPageLocator.EnumerateAncestorPageIds(markdownPath, environment.ExportDirectory);
                guard.RejectByLocalMirror(pageId, ancestors);
            }

            if (destinationMarkdownPath != null)
            {
                IReadOnlyList<string> destinationAncestors =
                    LocalPageLocator.EnumerateAncestorPageIds(destinationMarkdownPath, environment.ExportDirectory);
                guard.RejectByLocalMirror(destinationId, destinationAncestors);
            }

            NotionPageInfo page = await client.GetPageAsync(pageId);
            string allowedRootId = await guard.AuthorizeEditAsync(page);

            NotionDatabaseInfo database = await client.GetDatabaseAsync(destinationId);
            string destinationAllowedRootId = await guard.AuthorizeCreateAsync(database.Id, database.Parent);

            bool isAlreadyThere = string.Equals(page.Parent.Type, "data_source_id", StringComparison.Ordinal) &&
                                   string.Equals(page.Parent.Id, database.DataSourceId, StringComparison.OrdinalIgnoreCase);

            Console.WriteLine($"対象: {page.Title}");
            Console.WriteLine($"URL: {page.Url}");
            Console.WriteLine($"現在の親: {page.Parent.Type} {page.Parent.Id}");
            Console.WriteLine($"許可ルート（移動元）: {allowedRootId}");
            Console.WriteLine($"移動先データベース: {database.Title}");
            Console.WriteLine($"URL: {database.Url}");
            Console.WriteLine($"許可ルート（移動先）: {destinationAllowedRootId}");
            Console.WriteLine("注記: 移動してもページIDは変わらない。既存のページメンション・キャッシュマップ・原稿のリンクはそのまま使える。");
            Console.WriteLine("注記: このコマンドは親の変更だけを行う。DBのプロパティはset-propertiesで、親ページの本文修正はpushで別途行うこと。");

            if (isAlreadyThere)
            {
                Console.WriteLine();
                Console.WriteLine("既に移動先データベースの行になっています。何もしていません。");
                return 0;
            }

            if (!isConfirmed)
            {
                Console.WriteLine();
                Console.WriteLine("移動していません。内容を確認し、--confirm を付けて再実行してください。");
                return 0;
            }

            NotionPageInfo moved = await client.UpdateParentAsync(page.Id, database.DataSourceId);
            Console.WriteLine();
            bool actuallyMoved = string.Equals(moved.Parent.Type, "data_source_id", StringComparison.Ordinal) &&
                                  string.Equals(moved.Parent.Id, database.DataSourceId, StringComparison.OrdinalIgnoreCase);
            if (!actuallyMoved)
            {
                throw new WriterException(
                    "Notion APIは200を返しましたが、親は変わっていません" +
                    $"（応答後の親: {moved.Parent.Type} {moved.Parent.Id}）。" +
                    "このNotion APIバージョンはページの親変更を実質サポートしていない可能性があります。");
            }

            Console.WriteLine($"移動しました: {moved.Url}");
            return 0;
        }
    }
}
