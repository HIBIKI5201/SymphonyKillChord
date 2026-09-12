using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     pull時点のページ状態を保存するサイドカー。
    ///     push時の差分の基準と、Notion側が更新されていないかの判定に使う。
    /// </summary>
    internal sealed class PullSnapshot
    {
        private const int CURRENT_VERSION = 1;
        private const string SIDECAR_EXTENSION = ".notion-pull.json";

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary> サイドカー形式のバージョン。 </summary>
        public int Version { get; set; } = CURRENT_VERSION;

        /// <summary> 対象ページID。 </summary>
        public string PageId { get; set; } = string.Empty;

        /// <summary> 対象ページのURL。 </summary>
        public string PageUrl { get; set; } = string.Empty;

        /// <summary> 対象ページのタイトル。 </summary>
        public string PageTitle { get; set; } = string.Empty;

        /// <summary> pull時点のNotion側の最終更新日時。競合検出に使う。 </summary>
        public string LastEditedTime { get; set; } = string.Empty;

        /// <summary> pullした日時。 </summary>
        public DateTimeOffset PulledAtUtc { get; set; }

        /// <summary> pull時点のMarkdown原文。差分の基準になる。 </summary>
        public string Baseline { get; set; } = string.Empty;

        /// <summary>
        ///     作業ファイルに対応するサイドカーのパスを取得する。
        /// </summary>
        /// <param name="workFilePath">作業ファイルのパス。</param>
        /// <returns>サイドカーのパス。</returns>
        internal static string GetSidecarPath(string workFilePath)
        {
            return workFilePath + SIDECAR_EXTENSION;
        }

        /// <summary>
        ///     作業ファイルに対応するサイドカーを読み込む。
        /// </summary>
        /// <param name="workFilePath">作業ファイルのパス。</param>
        /// <returns>読み込んだサイドカー。</returns>
        internal static PullSnapshot Load(string workFilePath)
        {
            string sidecarPath = GetSidecarPath(workFilePath);
            if (!File.Exists(sidecarPath))
            {
                throw new WriterException(
                    $"pull情報が見つかりません: {sidecarPath}{Environment.NewLine}" +
                    "編集する前に pull コマンドで作業ファイルを取得してください。");
            }

            PullSnapshot? snapshot;
            try
            {
                snapshot = JsonSerializer.Deserialize<PullSnapshot>(
                    File.ReadAllText(sidecarPath, Encoding.UTF8));
            }
            catch (JsonException ex)
            {
                throw new WriterException(
                    $"pull情報が壊れています: {sidecarPath}{Environment.NewLine}" +
                    $"{ex.Message}{Environment.NewLine}" +
                    "pullし直してから編集内容を作り直してください。");
            }

            if (snapshot == null) { throw new WriterException($"pull情報を読み取れませんでした: {sidecarPath}"); }
            if (snapshot.Version != CURRENT_VERSION)
            {
                throw new WriterException($"未対応のpull情報バージョンです: {snapshot.Version}");
            }

            if (string.IsNullOrWhiteSpace(snapshot.PageId))
            {
                throw new WriterException($"pull情報にページIDがありません: {sidecarPath}");
            }

            return snapshot;
        }

        /// <summary>
        ///     サイドカーを保存する。
        /// </summary>
        /// <param name="workFilePath">作業ファイルのパス。</param>
        internal void Save(string workFilePath)
        {
            string json = JsonSerializer.Serialize(this, _jsonOptions);
            File.WriteAllText(GetSidecarPath(workFilePath), json, new UTF8Encoding(false));
        }
    }
}
