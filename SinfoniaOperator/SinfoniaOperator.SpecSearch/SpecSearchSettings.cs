using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SinfoniaStudio.SinfoniaOperator.SpecSearch
{
    /// <summary>
    ///     env内の検索設定を検証し、検索順位と候補の選別条件を保持する。
    /// </summary>
    public sealed class SpecSearchSettings
    {
        /// <summary>
        ///     重み、候補件数、関連度の下限を検証して設定を生成する。
        /// </summary>
        [JsonConstructor]
        public SpecSearchSettings(
            SpecPriorityRule[] sourceRules,
            SpecSectionRule[] sectionRules,
            int? candidateCount = null,
            int? maxChunksPerPage = null,
            double? minimumSimilarity = null,
            double? minimumLexicalCoverage = null,
            double? semanticWeight = null,
            double? lexicalWeight = null,
            int? rrfConstant = null)
        {
            ArgumentNullException.ThrowIfNull(sourceRules);
            ArgumentNullException.ThrowIfNull(sectionRules);
            if (sourceRules.Length == 0 || sourceRules.Any(rule => rule == null)
                || sectionRules.Any(rule => rule == null))
            {
                throw new ArgumentException("SourceRulesは1件以上必要です。ルールにnullは指定できません。");
            }

            SourceRules = Array.AsReadOnly(sourceRules.ToArray());
            SectionRules = Array.AsReadOnly(sectionRules.ToArray());
            CandidateCount = candidateCount ?? DEFAULT_CANDIDATE_COUNT;
            MaxChunksPerPage = maxChunksPerPage ?? DEFAULT_MAX_CHUNKS_PER_PAGE;
            MinimumSimilarity = minimumSimilarity ?? DEFAULT_MINIMUM_SIMILARITY;
            MinimumLexicalCoverage = minimumLexicalCoverage ?? DEFAULT_MINIMUM_LEXICAL_COVERAGE;
            SemanticWeight = semanticWeight ?? DEFAULT_SEARCH_WEIGHT;
            LexicalWeight = lexicalWeight ?? DEFAULT_SEARCH_WEIGHT;
            RrfConstant = rrfConstant ?? DEFAULT_RRF_CONSTANT;
            if (CandidateCount < 1 || CandidateCount > MAXIMUM_CANDIDATE_COUNT
                || MaxChunksPerPage < 1 || MaxChunksPerPage > CandidateCount || RrfConstant < 1
                || !double.IsFinite(MinimumSimilarity) || MinimumSimilarity < 0 || MinimumSimilarity > 1
                || !double.IsFinite(MinimumLexicalCoverage) || MinimumLexicalCoverage <= 0 || MinimumLexicalCoverage > 1
                || !double.IsFinite(SemanticWeight) || SemanticWeight < 0
                || !double.IsFinite(LexicalWeight) || LexicalWeight < 0
                || !double.IsFinite(SemanticWeight + LexicalWeight) || SemanticWeight + LexicalWeight <= 0)
            {
                throw new ArgumentException("SPEC_SEARCHの件数・関連度・検索重みが範囲外です。");
            }
        }

        /// <summary> 資料の識別条件と重み。 </summary>
        public IReadOnlyList<SpecPriorityRule> SourceRules { get; }
        /// <summary> 節ごとの通常検索・履歴検索の重み。 </summary>
        public IReadOnlyList<SpecSectionRule> SectionRules { get; }
        /// <summary> 各検索方式から融合する最大候補数。 </summary>
        public int CandidateCount { get; }
        /// <summary> 同一ページから採用する最大チャンク数。 </summary>
        public int MaxChunksPerPage { get; }
        /// <summary> 意味検索だけで候補にする際のコサイン類似度の下限。 </summary>
        public double MinimumSimilarity { get; }
        /// <summary> キーワード候補に必要な検索語の一致割合。 </summary>
        public double MinimumLexicalCoverage { get; }
        /// <summary> 意味検索のRRF重み。 </summary>
        public double SemanticWeight { get; }
        /// <summary> キーワード検索のRRF重み。 </summary>
        public double LexicalWeight { get; }
        /// <summary> RRFの順位平滑化定数。 </summary>
        public int RrfConstant { get; }

        /// <summary>
        ///     envのオブジェクトを読み込み、不明な設定名をエラーにする。
        /// </summary>
        public static SpecSearchSettings Parse(string json)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(json);
            return JsonConvert.DeserializeObject<SpecSearchSettings>(json, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            }) ?? throw new InvalidDataException("SPEC_SEARCHに検索設定オブジェクトが必要です。");
        }

        private const int DEFAULT_CANDIDATE_COUNT = 40;
        private const int MAXIMUM_CANDIDATE_COUNT = 1000;
        private const int DEFAULT_MAX_CHUNKS_PER_PAGE = 2;
        private const double DEFAULT_MINIMUM_SIMILARITY = 0.7D;
        private const double DEFAULT_MINIMUM_LEXICAL_COVERAGE = 0.5D;
        private const double DEFAULT_SEARCH_WEIGHT = 1.0D;
        private const int DEFAULT_RRF_CONSTANT = 60;
    }
}
