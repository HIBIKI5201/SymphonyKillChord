using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SinfoniaStudio.SinfoniaOperator.SpecSearch
{
    /// <summary>
    ///     日本語の文字bigramと英数字の単語を使ってBM25スコアを計算する。
    /// </summary>
    internal sealed partial class SpecLexicalIndex
    {
        /// <summary>
        ///     見出しを含むチャンクから語頻度と文書頻度を事前計算する。
        /// </summary>
        internal SpecLexicalIndex(IReadOnlyList<SpecChunkRecord> records)
        {
            _termCounts = new Dictionary<string, int>[records.Count];
            _lengths = new int[records.Count];
            for (int index = 0; index < records.Count; index++)
            {
                SpecChunkRecord record = records[index];
                string[] terms = Tokenize($"{record.HeadingBreadcrumb}\n{record.HeadingBreadcrumb}\n{record.Text}");
                _lengths[index] = terms.Length;
                Dictionary<string, int> counts = new(StringComparer.Ordinal);
                foreach (string term in terms) { counts[term] = counts.GetValueOrDefault(term) + 1; }
                _termCounts[index] = counts;
                foreach (string term in counts.Keys) { _documentFrequencies[term] = _documentFrequencies.GetValueOrDefault(term) + 1; }
            }
            _averageLength = records.Count == 0 ? 1.0D : Math.Max(1.0D, _lengths.Average());
        }

        /// <summary>
        ///     全チャンクのBM25値とクエリ語の一致率を返す。
        /// </summary>
        internal double[] CalculateScores(string query, out double[] coverage)
        {
            string[] terms = Tokenize(query).Distinct(StringComparer.Ordinal).ToArray();
            double[] scores = new double[_termCounts.Length];
            coverage = new double[_termCounts.Length];
            if (terms.Length == 0) { return scores; }
            for (int index = 0; index < _termCounts.Length; index++)
            {
                int matched = 0;
                foreach (string term in terms)
                {
                    if (!_termCounts[index].TryGetValue(term, out int frequency)) { continue; }
                    matched++;
                    double inverseFrequency = Math.Log(1.0D
                        + (_termCounts.Length - _documentFrequencies[term] + 0.5D) / (_documentFrequencies[term] + 0.5D));
                    double normalizer = frequency + K1 * (1.0D - B + B * _lengths[index] / _averageLength);
                    scores[index] += inverseFrequency * frequency * (K1 + 1.0D) / normalizer;
                }
                coverage[index] = (double)matched / terms.Length;
            }
            return scores;
        }

        private const double K1 = 1.2D;
        private const double B = 0.75D;
        private readonly Dictionary<string, int>[] _termCounts;
        private readonly int[] _lengths;
        private readonly Dictionary<string, int> _documentFrequencies = new(StringComparer.Ordinal);
        private readonly double _averageLength;

        /// <summary>
        ///     表記を正規化し、日本語の連続文字列と英数字から検索語を生成する。
        /// </summary>
        private static string[] Tokenize(string text)
        {
            string normalized = UrlRegex().Replace(text, string.Empty).Normalize(NormalizationForm.FormKC).ToLowerInvariant();
            List<string> terms = new();
            foreach (Match match in TermRegex().Matches(normalized))
            {
                string value = match.Value;
                if (!match.Groups["japanese"].Success || value.Length == 1)
                {
                    terms.Add(value);
                    continue;
                }
                for (int index = 0; index < value.Length - 1; index++) { terms.Add(value.Substring(index, 2)); }
            }
            return terms.ToArray();
        }

        /// <summary>
        ///     リンク先URLを語彙として数えない。
        /// </summary>
        [GeneratedRegex(@"https?://[^\s)<>""']+", RegexOptions.IgnoreCase)]
        private static partial Regex UrlRegex();

        /// <summary>
        ///     日本語と英数字を別の単位で認識する。
        /// </summary>
        [GeneratedRegex(@"(?<japanese>[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}ー]+)|[a-z0-9_]+")]
        private static partial Regex TermRegex();
    }
}
