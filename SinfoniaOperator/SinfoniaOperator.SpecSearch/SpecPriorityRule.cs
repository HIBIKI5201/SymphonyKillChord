using Newtonsoft.Json;
using System;

namespace SinfoniaStudio.SinfoniaOperator.SpecSearch
{
    /// <summary>
    ///     仕様書のソースファイルに適用する検索優先度ルールを表す。
    /// </summary>
    public sealed class SpecPriorityRule
    {
        /// <summary>
        ///     パスの部分一致条件と検索結果へ適用する重みを設定する。
        /// </summary>
        /// <param name="pathContains">ソースファイルに含まれるパス文字列。</param>
        /// <param name="weight">マッチした検索結果へ適用する重み。</param>
        [JsonConstructor]
        public SpecPriorityRule(string pathContains, double weight)
        {
            if (string.IsNullOrWhiteSpace(pathContains))
            {
                throw new ArgumentException("パスの部分一致条件を指定してください。", nameof(pathContains));
            }

            PathContains = pathContains;
            Weight = weight;
        }

        /// <summary> ソースファイルに含まれるパス文字列。 </summary>
        public string PathContains { get; }

        /// <summary> マッチした検索結果へ適用する重み。 </summary>
        public double Weight { get; }
    }
}
