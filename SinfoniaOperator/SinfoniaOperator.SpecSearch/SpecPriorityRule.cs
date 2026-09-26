using Newtonsoft.Json;
using System;

namespace SinfoniaStudio.SinfoniaOperator.SpecSearch
{
    /// <summary>
    ///     パス、ページID、文書用途のいずれかに一致する資料の重みを表す。
    /// </summary>
    public sealed class SpecPriorityRule
    {
        /// <summary>
        ///     資料の識別条件、検索重み、通常回答の根拠にできるかを設定する。
        /// </summary>
        [JsonConstructor]
        public SpecPriorityRule(
            string? pathContains, double weight, string? pageId = null,
            string? documentKind = null, bool? isPrimary = null)
        {
            int selectors = (string.IsNullOrWhiteSpace(pathContains) ? 0 : 1)
                + (string.IsNullOrWhiteSpace(pageId) ? 0 : 1)
                + (string.IsNullOrWhiteSpace(documentKind) ? 0 : 1);
            if (selectors != 1)
            {
                throw new ArgumentException("PathContains・PageId・DocumentKindのいずれか1つを指定してください。");
            }

            if (!double.IsFinite(weight) || weight < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(weight), "資料の重みは有限の非負数にしてください。");
            }

            PathContains = (pathContains ?? string.Empty).Replace('\\', '/');
            PageId = SpecPageMetadata.NormalizePageId(pageId);
            if (!string.IsNullOrWhiteSpace(pageId) && PageId.Length == 0)
            {
                throw new ArgumentException("PageIdの形式が不正です。", nameof(pageId));
            }

            DocumentKind = documentKind ?? string.Empty;
            Weight = weight;
            IsPrimary = isPrimary ?? throw new ArgumentException("IsPrimaryを明示してください。", nameof(isPrimary));
        }

        /// <summary> ソースファイルのパスの部分一致条件。 </summary>
        public string PathContains { get; }
        /// <summary> ページの改名・移動に影響されない識別子。 </summary>
        public string PageId { get; }
        /// <summary> DBプロパティ「文書用途」の完全一致条件。 </summary>
        public string DocumentKind { get; }
        /// <summary> 候補の順位に適用する重み。0は履歴検索でも除外する。 </summary>
        [JsonProperty(Required = Required.Always)]
        public double Weight { get; }
        /// <summary> 通常の仕様回答の根拠として利用できるか。 </summary>
        public bool IsPrimary { get; }

    }
}
