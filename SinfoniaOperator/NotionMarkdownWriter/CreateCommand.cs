using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SinfoniaStudio.NotionMarkdownExporter;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     Markdownファイルを本文として、許可ページ配下に子ページまたはデータベース行を作成するコマンド。
    /// </summary>
    internal static class CreateCommand
    {
        private static readonly string[] _valueOptions = { "parent" };
        private static readonly string[] _flagOptions = { "confirm" };
        private static readonly string[] _repeatableOptions = { "set" };

        /// <summary> 確認表示で出力する本文の最大行数。 </summary>
        private const int MAX_PREVIEW_LINE_COUNT = 40;

        /// <summary>
        ///     createコマンドを実行する。
        /// </summary>
        /// <param name="args">サブコマンド名を除いた引数。</param>
        /// <returns>正常終了時は0。</returns>
        internal static async Task<int> RunAsync(string[] args)
        {
            CommandArguments arguments = CommandArguments.Parse(args, _valueOptions, _flagOptions, _repeatableOptions);
            string markdownFilePath = Path.GetFullPath(arguments.GetRequiredOperand("本文のMarkdownファイル"));
            if (!File.Exists(markdownFilePath))
            {
                throw new WriterException($"Markdownファイルが見つかりません: {markdownFilePath}");
            }

            string? parentArgument = arguments.GetValue("parent");
            if (string.IsNullOrWhiteSpace(parentArgument))
            {
                throw new WriterException("--parent で作成先の親ページまたはデータベースを指定してください。");
            }

            bool isConfirmed = arguments.HasFlag("confirm");
            IReadOnlyList<string> assignments = arguments.GetValues("set");
            WriterEnvironment environment = WriterEnvironment.Load();
            string parentId = LocalPageLocator.ResolvePageId(parentArgument, out string? parentMarkdownPath);
            string markdown = MarkdownDiffBuilder.Normalize(await File.ReadAllTextAsync(markdownFilePath));

            using NotionWriteClient client = new(environment.NotionToken);
            WriteScopeGuard guard = new(environment.AllowedRootPageIds, client);

            if (parentMarkdownPath != null)
            {
                IReadOnlyList<string> ancestors =
                    LocalPageLocator.EnumerateAncestorPageIds(parentMarkdownPath, environment.ExportDirectory);
                guard.RejectByLocalMirror(parentId, ancestors);
            }

            NotionDatabaseInfo? database = await TryGetDatabaseAsync(client, parentId);
            return database != null
                ? await CreateDatabaseRowAsync(client, guard, database, markdown, assignments, isConfirmed)
                : await CreateChildPageAsync(client, guard, parentId, markdown, markdownFilePath, assignments, isConfirmed);
        }

        /// <summary>
        ///     指定IDをデータベースとして取得する。ページの場合はnullを返す。
        /// </summary>
        /// <param name="client">APIクライアント。</param>
        /// <param name="parentId">作成先ID。</param>
        /// <returns>データベース情報。ページの場合はnull。</returns>
        private static async Task<NotionDatabaseInfo?> TryGetDatabaseAsync(NotionWriteClient client, string parentId)
        {
            try
            {
                return await client.GetDatabaseAsync(parentId);
            }
            catch (NotionApiException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest)
            {
                return null;
            }
        }

        /// <summary>
        ///     データベースへ行を追加する。
        /// </summary>
        /// <param name="client">APIクライアント。</param>
        /// <param name="guard">スコープ検証。</param>
        /// <param name="database">作成先データベース。</param>
        /// <param name="markdown">本文のMarkdown。</param>
        /// <param name="assignments">--setで指定されたプロパティ。</param>
        /// <param name="isConfirmed">送信が承認されているかどうか。</param>
        /// <returns>正常終了時は0。</returns>
        private static async Task<int> CreateDatabaseRowAsync(
            NotionWriteClient client,
            WriteScopeGuard guard,
            NotionDatabaseInfo database,
            string markdown,
            IReadOnlyList<string> assignments,
            bool isConfirmed)
        {
            string allowedRootId = await guard.AuthorizeCreateAsync(database.Id, database.Parent);
            string? titlePropertyName = database.FindTitlePropertyName();
            if (titlePropertyName == null)
            {
                throw new WriterException($"データベース {database.Title} にタイトルプロパティがありません。");
            }

            Dictionary<string, object> properties = BuildProperties(database, assignments, out Dictionary<string, string> displayValues);
            if (!properties.ContainsKey(titlePropertyName))
            {
                throw new WriterException(
                    $"データベースへ追加するには --set \"{titlePropertyName}=ページ名\" が必要です。" +
                    $"指定できるプロパティ: {string.Join(", ", database.PropertyTypes.Keys)}");
            }

            Console.WriteLine($"作成先データベース: {database.Title}");
            Console.WriteLine($"URL: {database.Url}");
            Console.WriteLine($"許可ルート: {allowedRootId}");
            foreach (KeyValuePair<string, string> value in displayValues)
            {
                Console.WriteLine($"{value.Key}: {value.Value}");
            }

            Console.WriteLine("本文:");
            WritePreview(markdown);

            if (!isConfirmed)
            {
                Console.WriteLine();
                Console.WriteLine("作成していません。内容を確認し、--confirm を付けて再実行してください。");
                return 0;
            }

            NotionPageInfo createdPage = await client.CreatePageAsync(
                new Dictionary<string, string> { ["database_id"] = database.Id },
                markdown,
                properties);
            WriteCreated(createdPage);
            return 0;
        }

        /// <summary>
        ///     ページの子として新しいページを作成する。
        /// </summary>
        /// <param name="client">APIクライアント。</param>
        /// <param name="guard">スコープ検証。</param>
        /// <param name="parentPageId">親ページID。</param>
        /// <param name="markdown">本文のMarkdown。</param>
        /// <param name="markdownFilePath">エラー表示に使うファイルパス。</param>
        /// <param name="assignments">--setで指定されたプロパティ。ページ直下では使用できない。</param>
        /// <param name="isConfirmed">送信が承認されているかどうか。</param>
        /// <returns>正常終了時は0。</returns>
        private static async Task<int> CreateChildPageAsync(
            NotionWriteClient client,
            WriteScopeGuard guard,
            string parentPageId,
            string markdown,
            string markdownFilePath,
            IReadOnlyList<string> assignments,
            bool isConfirmed)
        {
            if (assignments.Count > 0)
            {
                throw new WriterException("--set はデータベースへ追加する場合にだけ使用できます。");
            }

            NotionPageInfo parentPage = await client.GetPageAsync(parentPageId);
            string allowedRootId = await guard.AuthorizeCreateAsync(parentPage.Id, parentPage.Parent);
            string title = ReadTitle(markdown, markdownFilePath);

            Console.WriteLine($"作成先: {parentPage.Title}");
            Console.WriteLine($"URL: {parentPage.Url}");
            Console.WriteLine($"許可ルート: {allowedRootId}");
            Console.WriteLine($"新規ページ名: {title}");
            Console.WriteLine("本文:");
            WritePreview(markdown);

            if (!isConfirmed)
            {
                Console.WriteLine();
                Console.WriteLine("作成していません。内容を確認し、--confirm を付けて再実行してください。");
                return 0;
            }

            NotionPageInfo createdPage = await client.CreatePageAsync(
                new Dictionary<string, string> { ["page_id"] = parentPageId },
                markdown,
                null);
            WriteCreated(createdPage);
            return 0;
        }

        /// <summary>
        ///     --setの指定をNotionのプロパティ値へ変換する。
        /// </summary>
        /// <param name="database">作成先データベース。</param>
        /// <param name="assignments">「プロパティ名=値」形式の指定。</param>
        /// <param name="displayValues">確認表示用の値。</param>
        /// <returns>APIへ送るプロパティ。</returns>
        private static Dictionary<string, object> BuildProperties(
            NotionDatabaseInfo database,
            IReadOnlyList<string> assignments,
            out Dictionary<string, string> displayValues)
        {
            Dictionary<string, object> properties = new();
            displayValues = new Dictionary<string, string>();
            foreach (string assignment in assignments)
            {
                int separatorIndex = assignment.IndexOf('=', StringComparison.Ordinal);
                if (separatorIndex <= 0)
                {
                    throw new WriterException($"--set は「プロパティ名=値」の形式で指定してください: {assignment}");
                }

                string name = assignment[..separatorIndex].Trim();
                string value = assignment[(separatorIndex + 1)..].Trim();
                if (!database.PropertyTypes.TryGetValue(name, out string? type))
                {
                    throw new WriterException(
                        $"データベースに存在しないプロパティです: {name}。" +
                        $"指定できるプロパティ: {string.Join(", ", database.PropertyTypes.Keys)}");
                }

                properties[name] = CreatePropertyValue(name, type, value);
                displayValues[name] = value;
            }

            return properties;
        }

        /// <summary>
        ///     プロパティ型に応じたNotionの値表現を生成する。
        /// </summary>
        /// <param name="name">プロパティ名。</param>
        /// <param name="type">Notion上の型。</param>
        /// <param name="value">指定された値。</param>
        /// <returns>APIへ送る値。</returns>
        private static object CreatePropertyValue(string name, string type, string value)
        {
            switch (type)
            {
                case "title":
                    return new Dictionary<string, object> { ["title"] = CreateTextRuns(value) };
                case "rich_text":
                    return new Dictionary<string, object> { ["rich_text"] = CreateTextRuns(value) };
                case "select":
                    return new Dictionary<string, object>
                    {
                        ["select"] = new Dictionary<string, string> { ["name"] = value }
                    };
                case "multi_select":
                    return new Dictionary<string, object>
                    {
                        ["multi_select"] = value
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(item => new Dictionary<string, string> { ["name"] = item.Trim() })
                            .ToList()
                    };
                case "url":
                case "email":
                case "phone_number":
                    return new Dictionary<string, object> { [type] = value };
                case "checkbox":
                    // 解釈できない値を黙ってfalseにすると、指定の取り違えに気付けない。
                    if (!bool.TryParse(value, out bool isChecked))
                    {
                        throw new WriterException($"true か false で指定してください: {name}={value}");
                    }

                    return new Dictionary<string, object> { ["checkbox"] = isChecked };
                case "number":
                    // 実行環境のカルチャに依存すると、小数点の解釈が変わって別の値が書き込まれる。
                    if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
                    {
                        throw new WriterException($"数値として解釈できません: {name}={value}");
                    }

                    return new Dictionary<string, object> { ["number"] = number };
                case "date":
                    return new Dictionary<string, object>
                    {
                        ["date"] = new Dictionary<string, string> { ["start"] = value }
                    };
                default:
                    throw new WriterException($"このツールが未対応のプロパティ型です: {name}（{type}）");
            }
        }

        /// <summary>
        ///     テキスト系プロパティのリッチテキスト表現を生成する。
        /// </summary>
        /// <param name="value">文字列。</param>
        /// <returns>リッチテキストの配列。</returns>
        private static List<Dictionary<string, object>> CreateTextRuns(string value)
        {
            return new List<Dictionary<string, object>>
            {
                new()
                {
                    ["type"] = "text",
                    ["text"] = new Dictionary<string, string> { ["content"] = value }
                }
            };
        }

        /// <summary>
        ///     本文先頭の見出しからページタイトルを読み取る。
        /// </summary>
        /// <param name="markdown">本文のMarkdown。</param>
        /// <param name="markdownFilePath">エラー表示に使うファイルパス。</param>
        /// <returns>ページタイトル。</returns>
        private static string ReadTitle(string markdown, string markdownFilePath)
        {
            string? firstContentLine = markdown
                .Split('\n')
                .FirstOrDefault(line => !string.IsNullOrWhiteSpace(line));
            if (firstContentLine == null || !firstContentLine.TrimStart().StartsWith("# ", StringComparison.Ordinal))
            {
                throw new WriterException(
                    $"本文の最初の行を「# ページ名」にしてください（この見出しがページタイトルになります）: {markdownFilePath}");
            }

            return firstContentLine.TrimStart()[2..].Trim();
        }

        /// <summary>
        ///     作成結果を表示する。
        /// </summary>
        /// <param name="createdPage">作成されたページ。</param>
        private static void WriteCreated(NotionPageInfo createdPage)
        {
            Console.WriteLine();
            Console.WriteLine($"ページを作成しました: {createdPage.Url}");
            Console.WriteLine($"ページID: {createdPage.Id}");
        }

        /// <summary>
        ///     本文の先頭部分を表示する。
        /// </summary>
        /// <param name="markdown">本文のMarkdown。</param>
        private static void WritePreview(string markdown)
        {
            string[] lines = markdown.Split('\n');
            int printedCount = Math.Min(lines.Length, MAX_PREVIEW_LINE_COUNT);
            for (int index = 0; index < printedCount; index++)
            {
                Console.WriteLine($"| {lines[index]}");
            }

            if (lines.Length > printedCount)
            {
                Console.WriteLine($"| ... 他{lines.Length - printedCount}行");
            }
        }
    }
}
