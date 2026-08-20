using System;
using System.Text;
using System.Threading.Tasks;
using SinfoniaStudio.NotionMarkdownExporter;
using SinfoniaStudio.SinfoniaOperator;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    internal static class Program
    {
        /// <summary>
        ///     Notion Markdownライターを起動する。
        /// </summary>
        /// <param name="args">コマンドライン引数。</param>
        /// <returns>正常終了時は0、それ以外は1。</returns>
        public static async Task<int> Main(string[] args)
        {
            Console.OutputEncoding = new UTF8Encoding(false);
            if (args.Length == 0)
            {
                WriteHelp();
                return 1;
            }

            string command = args[0];
            if (command is "--help" or "-h" or "help")
            {
                WriteHelp();
                return 0;
            }

            try
            {
                string[] commandArguments = args[1..];
                return command.ToLowerInvariant() switch
                {
                    "pull" => await PullCommand.RunAsync(commandArguments),
                    "push" => await PushCommand.RunAsync(commandArguments),
                    "create" => await CreateCommand.RunAsync(commandArguments),
                    _ => WriteUnknownCommand(command)
                };
            }
            catch (WriterException ex)
            {
                Console.Error.WriteLine($"エラー: {ex.Message}");
                return 1;
            }
            catch (NotionApiException ex)
            {
                Console.Error.WriteLine($"エラー: {ex.Message}");
                return 1;
            }
        }

        /// <summary>
        ///     不明なコマンドを通知する。
        /// </summary>
        /// <param name="command">指定されたコマンド。</param>
        /// <returns>終了コード。</returns>
        private static int WriteUnknownCommand(string command)
        {
            Console.Error.WriteLine($"エラー: 不明なコマンドです: {command}");
            WriteHelp();
            return 1;
        }

        /// <summary>
        ///     使用方法を標準出力へ表示する。
        /// </summary>
        private static void WriteHelp()
        {
            Console.WriteLine("Notion Markdown Writer");
            Console.WriteLine("仕様書ページへMarkdownで書き込むツールです。書き込みは部分置換のみで、全文置換は行いません。");
            Console.WriteLine();
            Console.WriteLine("使用方法:");
            Console.WriteLine("  NotionMarkdownWriter.exe pull <Markdownパス|URL|ID> [--out <ファイル>]");
            Console.WriteLine("      編集の基準になるMarkdown原文を作業ファイルへ取得する。");
            Console.WriteLine("  NotionMarkdownWriter.exe push <作業ファイル> [--whole] [--confirm] [--quiet]");
            Console.WriteLine("      作業ファイルの変更を差分として反映する。--confirm が無い場合は表示のみ。");
            Console.WriteLine("      --whole は本文全体の置換1件として送る。全面刷新で部分適用を避けたいときに使う。");
            Console.WriteLine("  NotionMarkdownWriter.exe create <Markdownファイル> --parent <Markdownパス|URL|ID> [--set <名前=値>] [--confirm]");
            Console.WriteLine("      許可ルート配下にページを作成する。");
            Console.WriteLine("      親がページの場合は本文の先頭h1見出しがページ名になる。");
            Console.WriteLine("      親がデータベースの場合は --set でタイトルを含むプロパティを指定する（複数可）。");
            Console.WriteLine();
            Console.WriteLine("設定キー:");
            Console.WriteLine($"  {OperatorConfigKeys.NOTION_TOKEN}                必須。秘密設定または環境変数に置くNotionトークン。");
            Console.WriteLine($"  {OperatorConfigKeys.NOTION_WRITE_ALLOWED_ROOTS}  必須。書き込みを許可するページID・データベースIDの配列。");
            Console.WriteLine($"  {OperatorConfigKeys.NOTION_EXPORT_OUTPUT}        任意。エクスポート出力先。ローカルパス指定に使う。");
        }
    }
}
