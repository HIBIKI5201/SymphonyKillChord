using Newtonsoft.Json;
using System;

namespace SinfoniaStudio.SinfoniaOperator.SpecSearch
{
    /// <summary>
    ///     見出しに応じて仕様本文と背景・履歴の検索重みを分ける。
    /// </summary>
    public sealed class SpecSectionRule
    {
        /// <summary>
        ///     見出し条件と、通常検索および履歴検索の重みを設定する。
        /// </summary>
        [JsonConstructor]
        public SpecSectionRule(string headingContains, double weight, double historyWeight)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(headingContains);
            if (!double.IsFinite(weight) || weight < 0 || !double.IsFinite(historyWeight) || historyWeight < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(weight), "節の重みは有限の非負数にしてください。");
            }

            HeadingContains = headingContains;
            Weight = weight;
            HistoryWeight = historyWeight;
        }

        /// <summary> 見出しのパンくずに含まれる文字列。 </summary>
        public string HeadingContains { get; }
        /// <summary> 通常検索の重み。0は候補から除外する。 </summary>
        [JsonProperty(Required = Required.Always)]
        public double Weight { get; }
        /// <summary> 履歴を含む検索の重み。0は候補から除外する。 </summary>
        [JsonProperty(Required = Required.Always)]
        public double HistoryWeight { get; }
    }
}
