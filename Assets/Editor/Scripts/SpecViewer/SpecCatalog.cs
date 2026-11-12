using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace KillChord.Editor.SpecViewer
{
    /// <summary>
    ///     プロジェクト ドキュメントの写しにあるページの一覧。
    ///     ページは写しの Markdown ファイルから集め、同期CIが生成する目録（_catalog.jsonl）があれば、タイトル・ページID・見出し・プロパティを足す。
    /// </summary>
    public sealed class SpecCatalog
    {
        /// <summary>
        ///     ページの一覧を生成する。
        /// </summary>
        /// <param name="entries">ページ。</param>
        /// <param name="warning">読み込み時の警告。無ければ空文字。</param>
        public SpecCatalog(IReadOnlyList<SpecCatalogEntry> entries, string warning)
        {
            Entries = entries;
            Warning = warning ?? string.Empty;
            foreach (SpecCatalogEntry entry in entries)
            {
                _byPath[entry.Path] = entry;
                if (!string.IsNullOrEmpty(entry.PageId)) { _byPageId[entry.PageId] = entry; }
            }
        }

        /// <summary> ページ。 </summary>
        public IReadOnlyList<SpecCatalogEntry> Entries { get; }

        /// <summary> 読み込み時の警告（目録が無い・形式が違うなど）。無ければ空文字。 </summary>
        public string Warning { get; }

        /// <summary>
        ///     写しのフォルダからページの一覧を読む。重いので、メインスレッド以外から呼んでよい。
        /// </summary>
        /// <param name="rootDirectory">写しのルート。</param>
        /// <returns>ページの一覧。</returns>
        public static SpecCatalog Load(string rootDirectory)
        {
            string warning = string.Empty;
            Dictionary<string, JObject> catalog = new(StringComparer.Ordinal);
            string catalogPath = System.IO.Path.Combine(rootDirectory, CATALOG_JSONL_FILE_NAME);
            if (!File.Exists(catalogPath))
            {
                warning = "目録（_catalog.jsonl）が無いため、タイトルはファイル名で表示し、検索はタイトルとパスだけを対象にします。";
            }
            else if (!IsSupportedCatalogVersion(rootDirectory))
            {
                warning = $"目録の形式バージョンが {SUPPORTED_CATALOG_VERSION} ではないため、目録を使わずに表示します。ビューアの更新が必要です。";
            }
            else
            {
                ReadCatalogLines(catalogPath, catalog);
            }

            List<SpecCatalogEntry> entries = new();
            foreach (string relativePath in EnumerateMarkdownFiles(rootDirectory))
            {
                entries.Add(catalog.TryGetValue(relativePath, out JObject item)
                    ? CreateEntry(relativePath, item)
                    : CreateFallbackEntry(relativePath));
            }

            return new SpecCatalog(entries, warning);
        }

        /// <summary>
        ///     パスからページを引く。
        /// </summary>
        /// <param name="path">写しのルートからの相対パス（/ 区切り）。</param>
        /// <param name="entry">見つかったページ。</param>
        /// <returns>見つかれば true。</returns>
        public bool TryGetByPath(string path, out SpecCatalogEntry entry)
        {
            return _byPath.TryGetValue(path ?? string.Empty, out entry);
        }

        /// <summary>
        ///     NotionのページIDからページを引く。
        /// </summary>
        /// <param name="pageId">ページID（ハイフンの有無・大文字小文字は問わない）。</param>
        /// <param name="entry">見つかったページ。</param>
        /// <returns>見つかれば true。</returns>
        public bool TryGetByPageId(string pageId, out SpecCatalogEntry entry)
        {
            return _byPageId.TryGetValue(NormalizePageId(pageId), out entry);
        }

        /// <summary>
        ///     タイトル・パス・プロパティ・見出しから検索する。空白で区切った語をすべて含むページを、タイトルに含むものから順に返す。
        /// </summary>
        /// <param name="query">検索語。</param>
        /// <param name="maxCount">返す最大件数。</param>
        /// <returns>見つかったページ。</returns>
        public IReadOnlyList<SpecCatalogEntry> Search(string query, int maxCount)
        {
            string[] terms = SplitTerms(query);
            if (terms.Length == 0) { return Array.Empty<SpecCatalogEntry>(); }

            return Entries
                .Where(entry => entry.ContainsAll(terms))
                .OrderBy(entry => entry.TitleContainsAll(terms) ? 0 : 1)
                .ThenBy(entry => entry.Title.Length)
                .Take(maxCount)
                .ToList();
        }

        /// <summary>
        ///     検索語を空白で区切り、小文字にする。
        /// </summary>
        /// <param name="query">検索語。</param>
        /// <returns>小文字にした語。</returns>
        public static string[] SplitTerms(string query)
        {
            return (query ?? string.Empty)
                .Split(new[] { ' ', '　', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(term => term.ToLowerInvariant())
                .ToArray();
        }

        /// <summary>
        ///     NotionのページIDを、ハイフン無し・小文字にそろえる。
        /// </summary>
        /// <param name="pageId">ページID。</param>
        /// <returns>そろえたページID。</returns>
        public static string NormalizePageId(string pageId)
        {
            return (pageId ?? string.Empty).Replace("-", string.Empty).ToLowerInvariant();
        }

        private const string CATALOG_JSONL_FILE_NAME = "_catalog.jsonl";
        private const string CATALOG_MARKDOWN_FILE_NAME = "_catalog.md";
        private const string DATABASE_FILE_NAME = "_database.md";
        private const string MARKDOWN_EXTENSION = ".md";
        private const string SUPPORTED_CATALOG_VERSION = "1";
        private const int CATALOG_HEADER_LINE_COUNT = 10;

        private static readonly Regex CATALOG_VERSION = new(@"形式バージョン:\s*(\d+)");
        private static readonly string[] EXCLUDED_TOP_DIRECTORIES = { ".git", "assets" };

        private readonly Dictionary<string, SpecCatalogEntry> _byPath = new(StringComparer.Ordinal);
        private readonly Dictionary<string, SpecCatalogEntry> _byPageId = new(StringComparer.Ordinal);

        /// <summary>
        ///     目録の形式バージョンが、このビューアの対応するものかを返す。
        /// </summary>
        /// <param name="rootDirectory">写しのルート。</param>
        /// <returns>対応していれば true。バージョンの記載が無ければ対応しているとみなす。</returns>
        private static bool IsSupportedCatalogVersion(string rootDirectory)
        {
            string path = System.IO.Path.Combine(rootDirectory, CATALOG_MARKDOWN_FILE_NAME);
            if (!File.Exists(path)) { return true; }

            foreach (string line in File.ReadLines(path).Take(CATALOG_HEADER_LINE_COUNT))
            {
                Match match = CATALOG_VERSION.Match(line);
                if (match.Success) { return match.Groups[1].Value == SUPPORTED_CATALOG_VERSION; }
            }

            return true;
        }

        /// <summary>
        ///     目録を1行ずつ読み、パスをキーにして積む。読めない行は飛ばす。
        /// </summary>
        /// <param name="catalogPath">目録のパス。</param>
        /// <param name="catalog">積む先。</param>
        private static void ReadCatalogLines(string catalogPath, Dictionary<string, JObject> catalog)
        {
            foreach (string line in File.ReadLines(catalogPath))
            {
                if (string.IsNullOrWhiteSpace(line)) { continue; }

                try
                {
                    JObject item = JObject.Parse(line);
                    string path = (string)item["path"];
                    if (!string.IsNullOrEmpty(path)) { catalog[path] = item; }
                }
                catch (Newtonsoft.Json.JsonException)
                {
                    // 壊れた行があっても、ほかのページは表示する。
                }
            }
        }

        /// <summary>
        ///     写しの Markdown ファイルを、ルートからの相対パス（/ 区切り）で列挙する。
        /// </summary>
        /// <param name="rootDirectory">写しのルート。</param>
        /// <returns>相対パス。</returns>
        private static IEnumerable<string> EnumerateMarkdownFiles(string rootDirectory)
        {
            string root = System.IO.Path.GetFullPath(rootDirectory);
            foreach (string file in Directory.EnumerateFiles(root, "*" + MARKDOWN_EXTENSION, SearchOption.AllDirectories))
            {
                string relative = file.Substring(root.Length).TrimStart('\\', '/').Replace('\\', '/');
                string top = relative.Split('/')[0];
                if (EXCLUDED_TOP_DIRECTORIES.Contains(top)) { continue; }
                if (relative.StartsWith("_catalog", StringComparison.Ordinal)) { continue; }

                yield return relative;
            }
        }

        /// <summary>
        ///     目録の1行からページを作る。
        /// </summary>
        /// <param name="path">相対パス。</param>
        /// <param name="item">目録の1行。</param>
        /// <returns>ページ。</returns>
        private static SpecCatalogEntry CreateEntry(string path, JObject item)
        {
            Dictionary<string, string> properties = new();
            if (item["properties"] is JObject propertyObject)
            {
                foreach (JProperty property in propertyObject.Properties())
                {
                    properties[property.Name] = property.Value.Type == JTokenType.String
                        ? (string)property.Value
                        : property.Value.ToString(Newtonsoft.Json.Formatting.None);
                }
            }

            List<string> headings = new();
            if (item["headings"] is JArray headingArray)
            {
                headings.AddRange(headingArray.OfType<JObject>().Select(heading => (string)heading["text"] ?? string.Empty));
            }

            string title = (string)item["title"];
            return new SpecCatalogEntry(
                path,
                string.IsNullOrEmpty(title) ? GetFallbackTitle(path) : title,
                NormalizePageId((string)item["page_id"]),
                (string)item["url"],
                (string)item["last_edited"],
                (string)item["database"],
                properties,
                headings);
        }

        /// <summary>
        ///     目録に無いページ（目録が無いとき・データベースの一覧ページ）を、パスだけから作る。
        /// </summary>
        /// <param name="path">相対パス。</param>
        /// <returns>ページ。</returns>
        private static SpecCatalogEntry CreateFallbackEntry(string path)
        {
            return new SpecCatalogEntry(path, GetFallbackTitle(path), null, null, null, null, null, null);
        }

        /// <summary>
        ///     パスから表示名を作る。データベースの一覧ページはフォルダ名にする。
        /// </summary>
        /// <param name="path">相対パス。</param>
        /// <returns>表示名。</returns>
        private static string GetFallbackTitle(string path)
        {
            string fileName = path.Substring(path.LastIndexOf('/') + 1);
            if (fileName == DATABASE_FILE_NAME)
            {
                string directory = path.Substring(0, Math.Max(0, path.LastIndexOf('/')));
                return directory.Substring(directory.LastIndexOf('/') + 1);
            }

            return fileName.EndsWith(MARKDOWN_EXTENSION, StringComparison.Ordinal)
                ? fileName.Substring(0, fileName.Length - MARKDOWN_EXTENSION.Length)
                : fileName;
        }
    }
}
