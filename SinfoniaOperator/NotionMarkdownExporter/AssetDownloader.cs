using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SinfoniaStudio.NotionMarkdownExporter
{
    /// <summary>
    ///     Enhanced Markdownの画像と添付ファイルをローカルへ保存するクラス。
    /// </summary>
    internal sealed class AssetDownloader : IDisposable
    {
        private static readonly Regex _imagePattern = new(
            @"!\[[^\]]*\]\((?<url>https?://[^)\s]+)\)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex _mediaPattern = new(
            "<(?:audio|video|file|pdf)\\b[^>]*\\bsrc=\"(?<url>https?://[^\"]+)\"",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromMinutes(10)
        };
        private bool _isDisposed;

        /// <summary>
        ///     Markdown内のメディアURLをダウンロードし、ローカル相対パスへ書き換える。
        /// </summary>
        /// <param name="pageId">メディアを所有するページID。</param>
        /// <param name="markdown">対象Markdown。</param>
        /// <param name="pageFilePath">Markdownファイルの保存先。</param>
        /// <param name="outputDirectory">エクスポート先ルート。</param>
        /// <param name="generatedFiles">生成ファイル一覧。</param>
        /// <param name="warning">警告出力。</param>
        /// <returns>URLを書き換えたMarkdownとダウンロード件数。</returns>
        internal async Task<AssetDownloadResult> DownloadAndRewriteAsync(
            string pageId,
            string markdown,
            string pageFilePath,
            string outputDirectory,
            ISet<string> generatedFiles,
            Action<string> warning)
        {
            List<AssetUrl> urls = FindAssetUrls(markdown);
            if (urls.Count == 0) { return new AssetDownloadResult(markdown, 0); }

            string assetsDirectory = Path.Combine(outputDirectory, "assets", NotionIdentifier.ToShortId(pageId));
            string rewritten = markdown;
            int downloadedCount = 0;
            int assetIndex = 1;

            foreach (AssetUrl assetUrl in urls)
            {
                try
                {
                    using HttpRequestMessage request = new(HttpMethod.Get, assetUrl.DecodedUrl);
                    using HttpResponseMessage response = await _httpClient.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    Directory.CreateDirectory(assetsDirectory);
                    string fileName = CreateAssetFileName(assetUrl.DecodedUrl, response, assetIndex++);
                    string filePath = Path.Combine(assetsDirectory, fileName);
                    await using (Stream input = await response.Content.ReadAsStreamAsync())
                    await using (FileStream output = new(
                        filePath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        81920,
                        true))
                    {
                        await input.CopyToAsync(output);
                    }

                    string relativePath = PathUtility.GetRelativeMarkdownPath(pageFilePath, filePath);
                    rewritten = rewritten.Replace(assetUrl.OriginalUrl, relativePath, StringComparison.Ordinal);
                    if (!string.Equals(assetUrl.OriginalUrl, assetUrl.DecodedUrl, StringComparison.Ordinal))
                    {
                        rewritten = rewritten.Replace(assetUrl.DecodedUrl, relativePath, StringComparison.Ordinal);
                    }

                    generatedFiles.Add(Path.GetRelativePath(outputDirectory, filePath));
                    downloadedCount++;
                }
                catch (Exception ex)
                {
                    string host = Uri.TryCreate(assetUrl.DecodedUrl, UriKind.Absolute, out Uri? uri)
                        ? uri.Host
                        : "不明なホスト";
                    warning($"ページ {pageId} の添付ファイルを取得できませんでした（{host}）: {ex.Message}");
                }
            }

            return new AssetDownloadResult(rewritten, downloadedCount);
        }

        /// <summary>
        ///     添付ファイル取得用HTTPクライアントを解放する。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) { return; }

            _httpClient.Dispose();
            _isDisposed = true;
        }

        /// <summary>
        ///     Markdownからダウンロード対象のメディアURLを重複なく抽出する。
        /// </summary>
        /// <param name="markdown">Enhanced Markdown。</param>
        /// <returns>メディアURL一覧。</returns>
        private static List<AssetUrl> FindAssetUrls(string markdown)
        {
            Dictionary<string, AssetUrl> urls = new(StringComparer.Ordinal);
            foreach (Regex pattern in new[] { _imagePattern, _mediaPattern })
            {
                foreach (Match match in pattern.Matches(markdown))
                {
                    string original = match.Groups["url"].Value;
                    string decoded = WebUtility.HtmlDecode(original);
                    urls.TryAdd(decoded, new AssetUrl(original, decoded));
                }
            }

            return urls.Values.ToList();
        }

        /// <summary>
        ///     URL、レスポンスヘッダー、Content-Typeから安全な保存ファイル名を生成する。
        /// </summary>
        /// <param name="url">取得元URL。</param>
        /// <param name="response">HTTPレスポンス。</param>
        /// <param name="index">ページ内の連番。</param>
        /// <returns>保存ファイル名。</returns>
        private static string CreateAssetFileName(string url, HttpResponseMessage response, int index)
        {
            string? headerName = response.Content.Headers.ContentDisposition?.FileNameStar ??
                                 response.Content.Headers.ContentDisposition?.FileName;
            string name = string.IsNullOrWhiteSpace(headerName)
                ? GetFileNameFromUrl(url)
                : headerName.Trim('"');
            name = PathUtility.CreateSafeName(name);

            if (string.IsNullOrWhiteSpace(Path.GetExtension(name)))
            {
                string extension = GetExtensionFromMediaType(response.Content.Headers.ContentType?.MediaType);
                name += extension;
            }

            return $"{index:000}-{name}";
        }

        /// <summary>
        ///     URLパスからファイル名を取得する。
        /// </summary>
        /// <param name="url">取得元URL。</param>
        /// <returns>ファイル名。取得できない場合はasset。</returns>
        private static string GetFileNameFromUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)) { return "asset"; }

            string name = Uri.UnescapeDataString(Path.GetFileName(uri.AbsolutePath));
            return string.IsNullOrWhiteSpace(name) ? "asset" : name;
        }

        /// <summary>
        ///     Content-Typeに対応する代表的な拡張子を取得する。
        /// </summary>
        /// <param name="mediaType">Content-Type。</param>
        /// <returns>先頭にピリオドを含む拡張子。未知の場合は.bin。</returns>
        private static string GetExtensionFromMediaType(string? mediaType)
        {
            return mediaType?.ToLowerInvariant() switch
            {
                "image/png" => ".png",
                "image/jpeg" => ".jpg",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                "image/svg+xml" => ".svg",
                "application/pdf" => ".pdf",
                "audio/mpeg" => ".mp3",
                "audio/wav" => ".wav",
                "video/mp4" => ".mp4",
                "text/plain" => ".txt",
                _ => ".bin"
            };
        }

        /// <summary>
        ///     エンコード前後の添付ファイルURLを保持するクラス。
        /// </summary>
        private sealed class AssetUrl
        {
            /// <summary>
            ///     添付ファイルURLを生成する。
            /// </summary>
            /// <param name="originalUrl">Markdown内のURL。</param>
            /// <param name="decodedUrl">HTTP取得用URL。</param>
            internal AssetUrl(string originalUrl, string decodedUrl)
            {
                OriginalUrl = originalUrl;
                DecodedUrl = decodedUrl;
            }

            internal string OriginalUrl { get; }
            internal string DecodedUrl { get; }
        }
    }

    /// <summary>
    ///     添付ファイルのダウンロード結果を保持するクラス。
    /// </summary>
    internal sealed class AssetDownloadResult
    {
        /// <summary>
        ///     添付ファイルのダウンロード結果を生成する。
        /// </summary>
        /// <param name="markdown">URL書き換え後のMarkdown。</param>
        /// <param name="downloadedCount">ダウンロード件数。</param>
        internal AssetDownloadResult(string markdown, int downloadedCount)
        {
            Markdown = markdown;
            DownloadedCount = downloadedCount;
        }

        internal string Markdown { get; }
        internal int DownloadedCount { get; }
    }
}
