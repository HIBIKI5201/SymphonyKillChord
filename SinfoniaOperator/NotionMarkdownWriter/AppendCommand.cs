using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SinfoniaStudio.NotionMarkdownExporter;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     指定した親ブロック（ページ・トグルなど）の子要素に、新しい段落を1件追加するコマンド。
    ///     --after で兄弟ブロックを指定するとその直後へ、指定しなければ末尾へ追加する。
    ///     トグルの中や、巨大な画像ブロックの隣接に依存せず新しい内容を足したい場合に使う。
    /// </summary>
    internal static class AppendCommand
    {
        private static readonly string[] _valueOptions = { "text", "after" };
        private static readonly string[] _flagOptions = { "confirm" };
        private static readonly string[] _repeatableOptions = Array.Empty<string>();

        /// <summary>
        ///     appendコマンドを実行する。
        /// </summary>
        /// <param name="args">サブコマンド名を除いた引数。</param>
        /// <returns>正常終了時は0。</returns>
        internal static async Task<int> RunAsync(string[] args)
        {
            CommandArguments arguments = CommandArguments.Parse(args, _valueOptions, _flagOptions, _repeatableOptions);
            string target = arguments.GetRequiredOperand("追加先（ページ・トグルなどのURLまたはID）");
            string? text = arguments.GetValue("text");
            if (string.IsNullOrEmpty(text))
            {
                throw new WriterException(
                    "--text で追加する段落を指定してください。" +
                    "ページメンションは <mention-page url=\"...\">表示名</mention-page> の形で埋め込めます。");
            }

            bool isConfirmed = arguments.HasFlag("confirm");
            WriterEnvironment environment = WriterEnvironment.Load();
            string parentId = BlockReferenceResolver.Resolve(target);
            string? afterTarget = arguments.GetValue("after");
            string? afterBlockId = string.IsNullOrEmpty(afterTarget) ? null : BlockReferenceResolver.Resolve(afterTarget);
            List<Dictionary<string, object>> richText = RichTextParser.Parse(text);

            using NotionWriteClient client = new(environment.NotionToken);
            WriteScopeGuard guard = new(environment.AllowedRootPageIds, client);

            // 親がページかブロックかで取得方法が違うため、まずページとして試す。
            NotionPageInfo? parentPage = await TryGetPageAsync(client, parentId);
            string allowedRootId;
            string parentDescription;
            if (parentPage != null)
            {
                allowedRootId = await guard.AuthorizeCreateAsync(parentPage.Id, parentPage.Parent);
                parentDescription = $"ページ「{parentPage.Title}」";
            }
            else
            {
                NotionBlockInfo parentBlock = await client.GetBlockAsync(parentId);
                allowedRootId = await guard.AuthorizeCreateAsync(parentBlock.Id, parentBlock.Parent);
                parentDescription = $"{parentBlock.Type}ブロック「{parentBlock.PlainText}」";
            }

            Console.WriteLine($"追加先: {parentDescription}");
            Console.WriteLine($"許可ルート: {allowedRootId}");
            Console.WriteLine($"追加する段落: {text}");
            Console.WriteLine(afterBlockId == null ? "位置: 子要素の末尾" : $"位置: ブロック {afterBlockId} の直後");

            if (!isConfirmed)
            {
                Console.WriteLine();
                Console.WriteLine("追加していません。内容を確認し、--confirm を付けて再実行してください。");
                return 0;
            }

            IReadOnlyList<string> createdIds = await client.AppendParagraphAsync(parentId, richText, afterBlockId);
            Console.WriteLine();
            Console.WriteLine($"追加しました。ブロックID: {string.Join(", ", createdIds)}");
            return 0;
        }

        /// <summary>
        ///     指定IDをページとして取得する。ブロックの場合はnullを返す。
        /// </summary>
        /// <param name="client">APIクライアント。</param>
        /// <param name="id">対象ID。</param>
        /// <returns>ページ情報。ブロックの場合はnull。</returns>
        private static async Task<NotionPageInfo?> TryGetPageAsync(NotionWriteClient client, string id)
        {
            try
            {
                return await client.GetPageAsync(id);
            }
            catch (NotionApiException)
            {
                return null;
            }
        }
    }
}
