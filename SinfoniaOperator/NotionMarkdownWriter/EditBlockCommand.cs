using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     ブロックIDで直接指定して、そのリッチテキストを書き換えるコマンド。
    ///     Markdown Content APIの文字列一致（push）では、巨大な画像ブロックに挟まれた短文や、
    ///     同名のトグルの見出しなどを安全に一意特定できない。ブロックIDならその問題が無い。
    /// </summary>
    internal static class EditBlockCommand
    {
        /// <summary> リッチテキストを持ち、書き換えを許可するブロック型。 </summary>
        private static readonly HashSet<string> _supportedTypes = new(StringComparer.Ordinal)
        {
            "paragraph", "heading_1", "heading_2", "heading_3",
            "bulleted_list_item", "numbered_list_item", "toggle", "quote", "callout", "to_do"
        };

        private static readonly string[] _valueOptions = { "text" };
        private static readonly string[] _flagOptions = { "confirm" };
        private static readonly string[] _repeatableOptions = Array.Empty<string>();

        /// <summary>
        ///     edit-blockコマンドを実行する。
        /// </summary>
        /// <param name="args">サブコマンド名を除いた引数。</param>
        /// <returns>正常終了時は0。</returns>
        internal static async Task<int> RunAsync(string[] args)
        {
            CommandArguments arguments = CommandArguments.Parse(args, _valueOptions, _flagOptions, _repeatableOptions);
            string target = arguments.GetRequiredOperand("対象ブロックのURLまたはID");
            string? text = arguments.GetValue("text");
            if (string.IsNullOrEmpty(text))
            {
                throw new WriterException("--text で新しいテキストを指定してください。空文字にはできません。");
            }

            bool isConfirmed = arguments.HasFlag("confirm");
            WriterEnvironment environment = WriterEnvironment.Load();
            string blockId = BlockReferenceResolver.Resolve(target);

            using NotionWriteClient client = new(environment.NotionToken);
            WriteScopeGuard guard = new(environment.AllowedRootPageIds, client);

            NotionBlockInfo block = await client.GetBlockAsync(blockId);
            if (!_supportedTypes.Contains(block.Type))
            {
                throw new WriterException(
                    $"このブロック型は未対応です: {block.Type}。" +
                    $"対応する型: {string.Join(", ", _supportedTypes)}");
            }

            string allowedRootId = await guard.AuthorizeCreateAsync(block.Id, block.Parent);

            Console.WriteLine($"ブロック型: {block.Type}");
            Console.WriteLine($"許可ルート: {allowedRootId}");
            Console.WriteLine($"変更前: {block.PlainText}");
            Console.WriteLine($"変更後: {text}");

            if (string.Equals(block.PlainText, text, StringComparison.Ordinal))
            {
                Console.WriteLine();
                Console.WriteLine("変更前後で同じ内容のため、何もしていません。");
                return 0;
            }

            if (!isConfirmed)
            {
                Console.WriteLine();
                Console.WriteLine("書き換えていません。内容を確認し、--confirm を付けて再実行してください。");
                return 0;
            }

            NotionBlockInfo updated = await client.UpdateBlockRichTextAsync(block.Id, block.Type, text);
            Console.WriteLine();
            Console.WriteLine($"書き換えました。反映後: {updated.PlainText}");
            return 0;
        }
    }
}
