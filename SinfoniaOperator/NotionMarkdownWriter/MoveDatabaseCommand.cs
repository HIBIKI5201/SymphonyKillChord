using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SinfoniaStudio.NotionMarkdownExporter;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     データベースそのものを別のページへ移動するコマンド。
    ///     各ページに散らばったDBの実体を、ルートの「データベース」ページへ集約する運用（Issue #2496）のために追加した。
    ///     データベースIDと行・ビュー・リレーションは移動後も変わらない。元の位置へのリンクドビュー作成はAPIで行えないため、Notion上で手作業する。
    /// </summary>
    internal static class MoveDatabaseCommand
    {
        private static readonly string[] _valueOptions = { "to" };
        private static readonly string[] _flagOptions = { "confirm" };
        private static readonly string[] _repeatableOptions = Array.Empty<string>();

        /// <summary>
        ///     move-databaseコマンドを実行する。
        /// </summary>
        /// <param name="args">サブコマンド名を除いた引数。</param>
        /// <returns>正常終了時は0。</returns>
        internal static async Task<int> RunAsync(string[] args)
        {
            CommandArguments arguments = CommandArguments.Parse(args, _valueOptions, _flagOptions, _repeatableOptions);
            string target = arguments.GetRequiredOperand("移動するデータベース");
            string? destinationArgument = arguments.GetValue("to");
            if (string.IsNullOrWhiteSpace(destinationArgument))
            {
                throw new WriterException("--to で移動先のページ（Markdownパス|URL|ID）を指定してください。");
            }

            bool isConfirmed = arguments.HasFlag("confirm");
            WriterEnvironment environment = WriterEnvironment.Load();
            string databaseId = LocalPageLocator.ResolvePageId(target, out string? markdownPath);
            string destinationId = LocalPageLocator.ResolvePageId(destinationArgument, out string? destinationMarkdownPath);

            using NotionWriteClient client = new(environment.NotionToken);
            WriteScopeGuard guard = new(environment.AllowedRootPageIds, client);

            if (markdownPath != null)
            {
                IReadOnlyList<string> ancestors =
                    LocalPageLocator.EnumerateAncestorPageIds(markdownPath, environment.ExportDirectory);
                guard.RejectByLocalMirror(databaseId, ancestors);
            }

            if (destinationMarkdownPath != null)
            {
                IReadOnlyList<string> destinationAncestors =
                    LocalPageLocator.EnumerateAncestorPageIds(destinationMarkdownPath, environment.ExportDirectory);
                guard.RejectByLocalMirror(destinationId, destinationAncestors);
            }

            // リンクドビューはデータソースを持たないため、GetDatabaseAsyncが例外で止める。実体だけが移動対象になる。
            NotionDatabaseInfo database = await client.GetDatabaseAsync(databaseId);
            string allowedRootId = await guard.AuthorizeCreateAsync(database.Id, database.Parent);

            NotionPageInfo destination = await client.GetPageAsync(destinationId);
            string destinationAllowedRootId = await guard.AuthorizeEditAsync(destination);

            bool isAlreadyThere = string.Equals(database.Parent.Type, "page_id", StringComparison.Ordinal) &&
                                   string.Equals(database.Parent.Id, destination.Id, StringComparison.OrdinalIgnoreCase);

            Console.WriteLine($"対象データベース: {database.Title}");
            Console.WriteLine($"URL: {database.Url}");
            Console.WriteLine($"現在の親: {database.Parent.Type} {database.Parent.Id}");
            Console.WriteLine($"許可ルート（移動元）: {allowedRootId}");
            Console.WriteLine($"移動先ページ: {destination.Title}");
            Console.WriteLine($"URL: {destination.Url}");
            Console.WriteLine($"許可ルート（移動先）: {destinationAllowedRootId}");
            Console.WriteLine("注記: 移動してもデータベースIDは変わらない。行・ビュー・リレーション・既存のリンクはそのまま使える。");
            Console.WriteLine("注記: 移動先ページ内での位置と、元の位置へのリンクドビュー作成はNotion上で手作業する。");

            if (isAlreadyThere)
            {
                Console.WriteLine();
                Console.WriteLine("既に移動先ページの直下にあります。何もしていません。");
                return 0;
            }

            if (!isConfirmed)
            {
                Console.WriteLine();
                Console.WriteLine("移動していません。内容を確認し、--confirm を付けて再実行してください。");
                return 0;
            }

            NotionParentReference movedParent = await client.UpdateDatabaseParentAsync(database.Id, destination.Id);
            Console.WriteLine();
            bool actuallyMoved = string.Equals(movedParent.Type, "page_id", StringComparison.Ordinal) &&
                                  string.Equals(movedParent.Id, destination.Id, StringComparison.OrdinalIgnoreCase);
            if (!actuallyMoved)
            {
                throw new WriterException(
                    "Notion APIは200を返しましたが、親は変わっていません" +
                    $"（応答後の親: {movedParent.Type} {movedParent.Id}）。" +
                    "このNotion APIバージョンはデータベースの親変更を実質サポートしていない可能性があります。");
            }

            Console.WriteLine($"移動しました: {database.Url}");
            return 0;
        }
    }
}
