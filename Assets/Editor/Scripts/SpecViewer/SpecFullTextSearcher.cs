using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace KillChord.Editor.SpecViewer
{
    /// <summary>
    ///     写しのページの本文を全文検索する。ファイルを1つずつ読むので、メインスレッド以外から呼ぶ。
    /// </summary>
    public sealed class SpecFullTextSearcher
    {
        /// <summary>
        ///     本文の検索器を生成する。
        /// </summary>
        /// <param name="resolver">ページの相対パスを絶対パスにするのに使う。</param>
        public SpecFullTextSearcher(SpecLinkResolver resolver)
        {
            _resolver = resolver;
        }

        /// <summary>
        ///     空白で区切った語をすべて含むページを探す。
        /// </summary>
        /// <param name="entries">探すページ。</param>
        /// <param name="query">検索語。</param>
        /// <param name="maxCount">返す最大件数。</param>
        /// <param name="cancellationToken">中断用のトークン。</param>
        /// <returns>見つかったページと、最初の語に一致した行の抜粋。</returns>
        public IReadOnlyList<SpecSearchHit> Search(
            IReadOnlyList<SpecCatalogEntry> entries,
            string query,
            int maxCount,
            CancellationToken cancellationToken)
        {
            string[] terms = SpecCatalog.SplitTerms(query);
            List<SpecSearchHit> hits = new();
            if (terms.Length == 0) { return hits; }

            foreach (SpecCatalogEntry entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string text;
                try
                {
                    text = File.ReadAllText(_resolver.ToAbsolutePath(entry.Path));
                }
                catch (IOException)
                {
                    // pull 中などで読めないファイルは飛ばす。
                    continue;
                }

                string lower = text.ToLowerInvariant();
                if (!ContainsAll(lower, terms)) { continue; }

                hits.Add(new SpecSearchHit(entry, CreateSnippet(text, lower, terms[0])));
                if (hits.Count >= maxCount) { break; }
            }

            return hits;
        }

        private const int SNIPPET_RADIUS = 40;

        private readonly SpecLinkResolver _resolver;

        /// <summary>
        ///     本文が語をすべて含むかを返す。
        /// </summary>
        /// <param name="lowerText">小文字にした本文。</param>
        /// <param name="terms">小文字にした語。</param>
        /// <returns>すべて含めば true。</returns>
        private static bool ContainsAll(string lowerText, IReadOnlyList<string> terms)
        {
            foreach (string term in terms)
            {
                if (!lowerText.Contains(term)) { return false; }
            }

            return true;
        }

        /// <summary>
        ///     語の前後を1行に収めた抜粋を作る。
        /// </summary>
        /// <param name="text">本文。</param>
        /// <param name="lowerText">小文字にした本文。</param>
        /// <param name="term">小文字にした語。</param>
        /// <returns>抜粋。</returns>
        private static string CreateSnippet(string text, string lowerText, string term)
        {
            int position = lowerText.IndexOf(term, StringComparison.Ordinal);
            if (position < 0) { return string.Empty; }

            int start = Math.Max(0, position - SNIPPET_RADIUS);
            int end = Math.Min(text.Length, position + term.Length + SNIPPET_RADIUS);
            string snippet = text.Substring(start, end - start).Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ');
            return $"{(start > 0 ? "…" : string.Empty)}{snippet.Trim()}{(end < text.Length ? "…" : string.Empty)}";
        }
    }
}
