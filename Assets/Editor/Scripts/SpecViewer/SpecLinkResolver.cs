using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace KillChord.Editor.SpecViewer
{
    /// <summary>
    ///     写しのページに書かれたリンクの行き先を決める。
    ///     写しの中の相対リンクと、写しにあるページを指す Notion のURLは、ビューアの中で開けるようにする。
    /// </summary>
    public sealed class SpecLinkResolver
    {
        /// <summary>
        ///     行き先の解決器を生成する。
        /// </summary>
        /// <param name="catalog">写しのページの一覧。</param>
        /// <param name="rootDirectory">写しのルート（ファイルの絶対パスを作るのに使う）。</param>
        public SpecLinkResolver(SpecCatalog catalog, string rootDirectory)
        {
            _catalog = catalog;
            _rootDirectory = rootDirectory;
        }

        /// <summary>
        ///     リンクの行き先を決める。
        /// </summary>
        /// <param name="currentPagePath">リンクが書かれたページの相対パス（/ 区切り）。</param>
        /// <param name="href">リンク先。</param>
        /// <returns>行き先。</returns>
        public SpecLinkTarget Resolve(string currentPagePath, string href)
        {
            if (string.IsNullOrWhiteSpace(href) || href.StartsWith("#", StringComparison.Ordinal))
            {
                return new SpecLinkTarget(SpecLinkTarget.TargetKind.None, string.Empty);
            }

            if (href.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                href.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                // 写しにある Notion のページなら、ブラウザではなくビューアで開く。
                if (IsNotionUrl(href) && _catalog.TryGetByPageId(ExtractPageId(href), out SpecCatalogEntry entry))
                {
                    return new SpecLinkTarget(SpecLinkTarget.TargetKind.Page, entry.Path);
                }

                return new SpecLinkTarget(SpecLinkTarget.TargetKind.External, href);
            }

            if (href.Contains("://") || href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            {
                return new SpecLinkTarget(SpecLinkTarget.TargetKind.External, href);
            }

            string relative = CombineRelative(currentPagePath, href);
            if (relative == null) { return new SpecLinkTarget(SpecLinkTarget.TargetKind.None, string.Empty); }

            if (_catalog.TryGetByPath(relative, out SpecCatalogEntry page))
            {
                return new SpecLinkTarget(SpecLinkTarget.TargetKind.Page, page.Path);
            }

            return new SpecLinkTarget(SpecLinkTarget.TargetKind.File, ToAbsolutePath(relative));
        }

        /// <summary>
        ///     写しの相対パスを、ファイルの絶対パスにする。
        /// </summary>
        /// <param name="relativePath">写しのルートからの相対パス（/ 区切り）。</param>
        /// <returns>絶対パス。</returns>
        public string ToAbsolutePath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(_rootDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        }

        /// <summary>
        ///     ページからの相対リンクを、写しのルートからの相対パスにする。% エンコードを戻し、. と .. を解決する。
        /// </summary>
        /// <param name="currentPagePath">リンクが書かれたページの相対パス。</param>
        /// <param name="href">相対リンク。</param>
        /// <returns>写しのルートからの相対パス。写しの外を指すなら null。</returns>
        public static string CombineRelative(string currentPagePath, string href)
        {
            string target = Uri.UnescapeDataString(StripFragment(href)).Replace('\\', '/');
            int lastSlash = (currentPagePath ?? string.Empty).LastIndexOf('/');
            string baseDirectory = lastSlash >= 0 ? currentPagePath.Substring(0, lastSlash) : string.Empty;

            List<string> segments = new();
            string combined = target.StartsWith("/", StringComparison.Ordinal) ? target : $"{baseDirectory}/{target}";
            foreach (string segment in combined.Split('/'))
            {
                if (segment.Length == 0 || segment == ".") { continue; }
                if (segment == "..")
                {
                    if (segments.Count == 0) { return null; }
                    segments.RemoveAt(segments.Count - 1);
                    continue;
                }

                segments.Add(segment);
            }

            return string.Join("/", segments);
        }

        /// <summary>
        ///     Notion のURLからページIDを取り出す。
        /// </summary>
        /// <param name="url">Notion のURL。</param>
        /// <returns>ページID（ハイフン無し・小文字）。見つからなければ空文字。</returns>
        public static string ExtractPageId(string url)
        {
            string path = StripFragment(url);
            int query = path.IndexOf('?');
            if (query >= 0) { path = path.Substring(0, query); }

            MatchCollection matches = PAGE_ID.Matches(path);
            if (matches.Count == 0) { return string.Empty; }

            return SpecCatalog.NormalizePageId(matches[matches.Count - 1].Value);
        }

        private static readonly Regex PAGE_ID = new(@"[0-9a-fA-F]{8}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{12}");
        private static readonly string[] NOTION_HOSTS = { "notion.so", "notion.site", "app.notion.com" };

        private readonly SpecCatalog _catalog;
        private readonly string _rootDirectory;

        /// <summary>
        ///     Notion のURLかを返す。
        /// </summary>
        /// <param name="url">URL。</param>
        /// <returns>Notion のURLなら true。</returns>
        private static bool IsNotionUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri)) { return false; }

            foreach (string host in NOTION_HOSTS)
            {
                if (uri.Host == host || uri.Host.EndsWith("." + host, StringComparison.OrdinalIgnoreCase)) { return true; }
            }

            return false;
        }

        /// <summary>
        ///     URLの # 以降を除く。
        /// </summary>
        /// <param name="url">URL・パス。</param>
        /// <returns># 以降を除いた文字列。</returns>
        private static string StripFragment(string url)
        {
            int hash = url.IndexOf('#');
            return hash >= 0 ? url.Substring(0, hash) : url;
        }
    }
}
