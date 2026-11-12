using System;
using System.Collections.Generic;
using System.Linq;

namespace KillChord.Editor.SpecViewer
{
    /// <summary>
    ///     プロジェクト ドキュメントの写しの1ページ。目録（_catalog.jsonl）にあればその情報を持つ。
    /// </summary>
    public sealed class SpecCatalogEntry
    {
        /// <summary>
        ///     ページを生成する。
        /// </summary>
        /// <param name="path">写しのルートからの相対パス（/ 区切り）。</param>
        /// <param name="title">タイトル。</param>
        /// <param name="pageId">NotionのページID（ハイフンの有無・大文字小文字は問わない）。無ければ null。</param>
        /// <param name="url">Notionで開くURL。無ければ空文字。</param>
        /// <param name="lastEdited">最終更新日時（文字列のまま）。</param>
        /// <param name="database">所属するデータベース名。</param>
        /// <param name="properties">プロパティ名と値。</param>
        /// <param name="headings">見出し。</param>
        public SpecCatalogEntry(
            string path,
            string title,
            string pageId,
            string url,
            string lastEdited,
            string database,
            IReadOnlyDictionary<string, string> properties,
            IReadOnlyList<string> headings)
        {
            Path = path;
            Title = title;
            PageId = SpecCatalog.NormalizePageId(pageId);
            Url = url ?? string.Empty;
            LastEdited = lastEdited ?? string.Empty;
            Database = database ?? string.Empty;
            Properties = properties ?? new Dictionary<string, string>();
            Headings = headings ?? Array.Empty<string>();

            // 検索のたびに組み立てないよう、検索対象の文字列を先に作っておく。
            _searchText = string.Join("\n", new[] { Title, Path, Database }
                    .Concat(Properties.Select(pair => $"{pair.Key}:{pair.Value}"))
                    .Concat(Headings))
                .ToLowerInvariant();
            _titleText = Title.ToLowerInvariant();
        }

        /// <summary> 写しのルートからの相対パス（/ 区切り）。 </summary>
        public string Path { get; }

        /// <summary> タイトル。 </summary>
        public string Title { get; }

        /// <summary> NotionのページID（ハイフン無し・小文字）。 </summary>
        public string PageId { get; }

        /// <summary> Notionで開くURL。 </summary>
        public string Url { get; }

        /// <summary> 最終更新日時。 </summary>
        public string LastEdited { get; }

        /// <summary> 所属するデータベース名。 </summary>
        public string Database { get; }

        /// <summary> プロパティ名と値。 </summary>
        public IReadOnlyDictionary<string, string> Properties { get; }

        /// <summary> 見出し。 </summary>
        public IReadOnlyList<string> Headings { get; }

        /// <summary>
        ///     検索語をすべて含むかを返す。
        /// </summary>
        /// <param name="lowerTerms">小文字にした検索語。</param>
        /// <returns>すべて含めば true。</returns>
        public bool ContainsAll(IReadOnlyList<string> lowerTerms)
        {
            foreach (string term in lowerTerms)
            {
                if (!_searchText.Contains(term)) { return false; }
            }

            return true;
        }

        /// <summary>
        ///     タイトルが検索語をすべて含むかを返す（検索結果の並べ替え用）。
        /// </summary>
        /// <param name="lowerTerms">小文字にした検索語。</param>
        /// <returns>すべて含めば true。</returns>
        public bool TitleContainsAll(IReadOnlyList<string> lowerTerms)
        {
            foreach (string term in lowerTerms)
            {
                if (!_titleText.Contains(term)) { return false; }
            }

            return true;
        }

        private readonly string _searchText;
        private readonly string _titleText;
    }
}
