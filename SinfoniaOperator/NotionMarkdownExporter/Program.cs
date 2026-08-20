using System;
using System.Text;
using System.Threading.Tasks;

namespace SinfoniaStudio.NotionMarkdownExporter
{
    internal static class Program
    {
        /// <summary>
        ///     Notion Markdownエクスポーターを起動する。
        /// </summary>
        /// <param name="args">コマンドライン引数。</param>
        /// <returns>正常終了時は0、それ以外は1。</returns>
        public static async Task<int> Main(string[] args)
        {
            Console.OutputEncoding = new UTF8Encoding(false);
            Console.WriteLine("Notion Markdown Exporter");
            Console.WriteLine();

            ExporterOptionsResult optionsResult = ExporterOptions.Create(args);
            if (optionsResult.IsHelpRequested)
            {
                ExporterOptions.WriteHelp();
                return 0;
            }

            if (optionsResult.Options == null)
            {
                Console.Error.WriteLine($"エラー: {optionsResult.ErrorMessage}");
                Console.Error.WriteLine("使用方法は --help で確認できます。");
                WaitForExit(optionsResult.IsInteractive);
                return 1;
            }

            try
            {
                using NotionApiClient apiClient = new(optionsResult.Options.NotionToken);
                NotionExporter exporter = new(apiClient, optionsResult.Options);
                ExportSummary summary = await exporter.ExportAsync();

                Console.WriteLine();
                Console.WriteLine("エクスポートが完了しました。");
                Console.WriteLine($"  ページ: {summary.PageCount}");
                Console.WriteLine($"  データベース: {summary.DatabaseCount}");
                Console.WriteLine($"  添付ファイル: {summary.AssetCount}");
                Console.WriteLine($"  出力先: {summary.OutputDirectory}");
                if (summary.WarningCount > 0)
                {
                    Console.WriteLine($"  警告: {summary.WarningCount}（上記ログを確認してください）");
                }

                WaitForExit(optionsResult.IsInteractive);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine($"エクスポートに失敗しました: {ex.Message}");
                WaitForExit(optionsResult.IsInteractive);
                return 1;
            }
        }

        /// <summary>
        ///     対話実行時に結果を確認できるよう、Enterキーの入力を待つ。
        /// </summary>
        /// <param name="isInteractive">対話実行かどうか。</param>
        private static void WaitForExit(bool isInteractive)
        {
            if (!isInteractive || Console.IsInputRedirected) { return; }

            Console.WriteLine();
            Console.Write("Enterキーで終了します...");
            Console.ReadLine();
        }
    }
}
