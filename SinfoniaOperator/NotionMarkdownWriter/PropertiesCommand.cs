using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     データベース内の既存ページのプロパティ（カテゴリー等）を後から設定・更新するコマンド。
    ///     createはページ作成時にしかプロパティを指定できないため、付け忘れた場合の是正に使う。
    /// </summary>
    internal static class PropertiesCommand
    {
        private static readonly string[] _valueOptions = Array.Empty<string>();
        private static readonly string[] _flagOptions = { "confirm" };
        private static readonly string[] _repeatableOptions = { "set" };

        /// <summary>
        ///     set-propertiesコマンドを実行する。
        /// </summary>
        /// <param name="args">サブコマンド名を除いた引数。</param>
        /// <returns>正常終了時は0。</returns>
        internal static async Task<int> RunAsync(string[] args)
        {
            CommandArguments arguments = CommandArguments.Parse(args, _valueOptions, _flagOptions, _repeatableOptions);
            string target = arguments.GetRequiredOperand("Markdownパス|URL|ID");
            IReadOnlyList<string> assignments = arguments.GetValues("set");
            if (assignments.Count == 0)
            {
                throw new WriterException("--set \"プロパティ名=値\" を1つ以上指定してください。");
            }

            bool isConfirmed = arguments.HasFlag("confirm");
            WriterEnvironment environment = WriterEnvironment.Load();
            string pageId = LocalPageLocator.ResolvePageId(target, out _);

            using NotionWriteClient client = new(environment.NotionToken);
            WriteScopeGuard guard = new(environment.AllowedRootPageIds, client);
            NotionPageInfo page = await client.GetPageAsync(pageId);

            // 対象はデータベースの行（ページ）自身の編集なので、通常の本文編集と同じ許可判定でよい。
            string allowedRootId = await guard.AuthorizeEditAsync(page);

            string databaseId = await ResolveDatabaseIdAsync(client, page.Parent);
            NotionDatabaseInfo database = await client.GetDatabaseAsync(databaseId);
            Dictionary<string, object> properties = CreateCommand.BuildProperties(database, assignments, out Dictionary<string, string> displayValues);

            Console.WriteLine($"対象ページ: {page.Title}");
            Console.WriteLine($"URL: {page.Url}");
            Console.WriteLine($"許可ルート: {allowedRootId}");
            foreach (KeyValuePair<string, string> value in displayValues)
            {
                Console.WriteLine($"{value.Key}: {value.Value}");
            }

            if (!isConfirmed)
            {
                Console.WriteLine();
                Console.WriteLine("設定していません。内容を確認し、--confirm を付けて再実行してください。");
                return 0;
            }

            NotionPageInfo updated = await client.UpdatePropertiesAsync(page.Id, properties);
            Console.WriteLine();
            Console.WriteLine($"プロパティを更新しました: {updated.Url}");
            return 0;
        }

        /// <summary>
        ///     ページの親を辿り、所属するデータベースのIDを求める。
        ///     データベース行の親はデータソース、データソースの親がデータベースという構造をたどる。
        /// </summary>
        /// <param name="client">APIクライアント。</param>
        /// <param name="parent">起点となるページの親への参照。</param>
        /// <returns>データベースID。</returns>
        private static async Task<string> ResolveDatabaseIdAsync(NotionWriteClient client, NotionParentReference parent)
        {
            const int maxDepth = 8;
            NotionParentReference current = parent;
            for (int depth = 0; depth < maxDepth; depth++)
            {
                if (current.Type == "database_id") { return current.Id; }

                current = current.Type switch
                {
                    "data_source_id" => await client.GetParentAsync(NotionObjectKind.DataSource, current.Id),
                    "page_id" => await client.GetParentAsync(NotionObjectKind.Page, current.Id),
                    "block_id" => await client.GetParentAsync(NotionObjectKind.Block, current.Id),
                    _ => throw new WriterException(
                        "このページの親はデータベースではありません。プロパティを設定できるのはデータベース内の行だけです。")
                };
            }

            throw new WriterException($"データベースの親を{maxDepth}段辿っても見つかりませんでした。");
        }
    }
}
