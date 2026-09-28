using System;
using System.Collections.Generic;
using System.Linq;

namespace SinfoniaStudio.SinfoniaOperator.SpecSearch
{
    /// <summary>
    ///     根拠にできる資料を選別し、BM25と意味検索の順位を重み付きRRFで統合する。
    /// </summary>
    public sealed class SpecSearchEngine
    {
        /// <summary>
        ///     検索インデックスとenv設定から再利用可能な検索器を生成する。
        /// </summary>
        public SpecSearchEngine(SpecIndex index, SpecSearchSettings settings)
        {
            _index = index ?? throw new ArgumentNullException(nameof(index));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _priorities = new SpecPriorityTable(settings.SourceRules);
            _lexicalIndex = new SpecLexicalIndex(index.Records);
        }

        /// <summary> 資料ルールに一致しなかったチャンク数。 </summary>
        public int UnclassifiedCount => _index.Records.Count(record => _priorities.Resolve(record) == null);

        /// <summary>
        ///     関連する候補だけを融合し、同じページと重複本文による結果の占有を抑える。
        /// </summary>
        public SpecChunkRecord[] Search(string query, float[] queryVector, int k, bool includeHistory = false)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(query);
            ArgumentNullException.ThrowIfNull(queryVector);
            if (k <= 0 || k > _settings.CandidateCount) { throw new ArgumentOutOfRangeException(nameof(k)); }
            if (queryVector.Any(value => !float.IsFinite(value)))
            {
                throw new ArgumentException("検索ベクトルには有限の数値を指定してください。", nameof(queryVector));
            }
            if (_index.Records.Count == 0) { return Array.Empty<SpecChunkRecord>(); }
            if (queryVector.Length != _index.Records[0].Vector.Length)
            {
                throw new ArgumentException("クエリとインデックスのベクトル次元が一致しません。", nameof(queryVector));
            }

            double[] lexicalScores = _lexicalIndex.CalculateScores(query, out double[] coverage);
            List<Candidate> candidates = new();
            for (int index = 0; index < _index.Records.Count; index++)
            {
                SpecChunkRecord record = _index.Records[index];
                double weight = ResolveWeight(record, includeHistory);
                if (weight <= 0) { continue; }
                double similarity = SpecIndex.CalculateCosineSimilarity(queryVector, record.Vector);
                double lexical = coverage[index] >= _settings.MinimumLexicalCoverage ? lexicalScores[index] : 0;
                candidates.Add(new Candidate(index, record, weight, similarity, lexical));
            }

            Dictionary<int, double> fusedScores = new();
            AccumulateRanks(candidates.Where(candidate => candidate.Similarity >= _settings.MinimumSimilarity)
                .OrderByDescending(candidate => candidate.Similarity).ThenBy(candidate => candidate.Index),
                _settings.SemanticWeight, fusedScores);
            AccumulateRanks(candidates.Where(candidate => candidate.Lexical > 0)
                .OrderByDescending(candidate => candidate.Lexical).ThenBy(candidate => candidate.Index),
                _settings.LexicalWeight, fusedScores);

            Dictionary<string, int> pageCounts = new(StringComparer.Ordinal);
            HashSet<string> texts = new(StringComparer.Ordinal);
            List<SpecChunkRecord> results = new();
            foreach (Candidate candidate in candidates.Where(candidate => fusedScores.ContainsKey(candidate.Index))
                .OrderByDescending(candidate => fusedScores[candidate.Index] * candidate.Weight)
                .ThenBy(candidate => candidate.Index))
            {
                SpecChunkRecord record = candidate.Record;
                string pageKey = record.Metadata.PageId.Length > 0 ? record.Metadata.PageId : record.SourceFile;
                if (pageCounts.GetValueOrDefault(pageKey) >= _settings.MaxChunksPerPage || !texts.Add($"{record.Metadata.AppliesTo}\0{record.Metadata.SpecificationStatus}\0{record.Text.Trim()}"))
                {
                    continue;
                }
                pageCounts[pageKey] = pageCounts.GetValueOrDefault(pageKey) + 1;
                SpecPriorityRule rule = _priorities.Resolve(record)!;
                SpecPageMetadata metadata = record.Metadata;
                if (metadata.DocumentKind.Length == 0)
                {
                    metadata = new SpecPageMetadata(metadata.PageId,
                        rule.IsPrimary ? "仕様資料（設定による分類）" : "記録・参考資料（設定による分類）",
                        metadata.SpecificationStatus, metadata.ImplementationStatus, metadata.AppliesTo, metadata.LastEdited);
                }
                results.Add(new SpecChunkRecord(record.SourceFile, record.HeadingBreadcrumb, record.NotionUrl,
                    record.Text, record.Vector, metadata));
                if (results.Count == k) { break; }
            }
            return results.ToArray();
        }

        private readonly SpecIndex _index;
        private readonly SpecSearchSettings _settings;
        private readonly SpecPriorityTable _priorities;
        private readonly SpecLexicalIndex _lexicalIndex;

        /// <summary>
        ///     資料と節の重みを合成する。未分類や未採用を通常の根拠へ自動昇格させない。
        /// </summary>
        private double ResolveWeight(SpecChunkRecord record, bool includeHistory)
        {
            SpecPriorityRule? rule = _priorities.Resolve(record);
            if (rule == null || rule.Weight <= 0 || (!includeHistory && !rule.IsPrimary)) { return 0; }
            if (!includeHistory && record.Metadata.SpecificationStatus
                is "検討中" or "草案" or "未承認" or "要確認" or "反映待ち" or "保留" or "却下" or "廃止" or "アーカイブ")
            {
                return 0;
            }

            // より深い見出しで親の除外を覆さない。複数条件に一致したら最も小さい重みを使う。
            double sectionWeight = _settings.SectionRules
                .Where(section => record.HeadingBreadcrumb.Contains(section.HeadingContains, StringComparison.Ordinal))
                .Select(section => includeHistory ? section.HistoryWeight : section.Weight)
                .DefaultIfEmpty(1.0D)
                .Min();
            return rule.Weight * sectionWeight;
        }

        /// <summary>
        ///     各方式の上位候補の順位を、比較可能なRRF値へ変換する。
        /// </summary>
        private void AccumulateRanks(IEnumerable<Candidate> ordered, double weight, IDictionary<int, double> scores)
        {
            if (weight <= 0) { return; }
            int rank = 0;
            foreach (Candidate candidate in ordered.Take(_settings.CandidateCount))
            {
                rank++;
                scores.TryGetValue(candidate.Index, out double current);
                scores[candidate.Index] = current + weight / (_settings.RrfConstant + (double)rank);
            }
        }

        /// <summary>
        ///     1回の検索中だけ利用する候補と各方式のスコア。
        /// </summary>
        private sealed class Candidate
        {
            /// <summary>
            ///     候補の識別子と順位付けに必要な値を保持する。
            /// </summary>
            internal Candidate(int index, SpecChunkRecord record, double weight, double similarity, double lexical)
            {
                Index = index;
                Record = record;
                Weight = weight;
                Similarity = similarity;
                Lexical = lexical;
            }

            /// <summary> インデックス内の位置。 </summary>
            internal int Index { get; }
            /// <summary> 検索対象のチャンク。 </summary>
            internal SpecChunkRecord Record { get; }
            /// <summary> 資料と節を合わせた重み。 </summary>
            internal double Weight { get; }
            /// <summary> コサイン類似度。 </summary>
            internal double Similarity { get; }
            /// <summary> 語の一致率を満たしたBM25値。 </summary>
            internal double Lexical { get; }
        }
    }
}
