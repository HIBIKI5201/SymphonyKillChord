using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace SinfoniaStudio.SinfoniaOperator.SpecSearch
{
    /// <summary>
    ///     検索チャンクが継承するNotionページの識別情報と仕様の状態。
    /// </summary>
    public sealed partial class SpecPageMetadata
    {
        /// <summary>
        ///     ページの分類・採用状態・適用範囲を保持する。
        /// </summary>
        [JsonConstructor]
        public SpecPageMetadata(
            string pageId = "", string documentKind = "", string specificationStatus = "",
            string implementationStatus = "", string appliesTo = "", string lastEdited = "")
        {
            PageId = NormalizePageId(pageId);
            DocumentKind = documentKind ?? string.Empty;
            SpecificationStatus = specificationStatus ?? string.Empty;
            ImplementationStatus = implementationStatus ?? string.Empty;
            AppliesTo = appliesTo ?? string.Empty;
            LastEdited = lastEdited ?? string.Empty;
        }

        /// <summary> ハイフンを除いたNotionページID。 </summary>
        public string PageId { get; }
        /// <summary> 文書用途。概要・詳細という粒度とは別の分類。 </summary>
        public string DocumentKind { get; }
        /// <summary> 採用済み・検討中・廃止などの仕様の状態。 </summary>
        public string SpecificationStatus { get; }
        /// <summary> 実装済み・未実装などの実装の状態。 </summary>
        public string ImplementationStatus { get; }
        /// <summary> 製品版・体験版などの適用範囲。 </summary>
        public string AppliesTo { get; }
        /// <summary> Notion側の最終更新日時。 </summary>
        public string LastEdited { get; }

        /// <summary>
        ///     IDまたはNotion URLから安定したページ識別子を取り出す。
        /// </summary>
        public static string NormalizePageId(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) { return string.Empty; }
            if (Guid.TryParse(value.Trim(), out Guid id)) { return id.ToString("N"); }
            if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? uri)) { return string.Empty; }
            Match match = PageIdRegex().Match(uri.AbsolutePath);
            return match.Success && Guid.TryParse(match.Value.TrimEnd('/'), out id) ? id.ToString("N") : string.Empty;
        }

        /// <summary>
        ///     Notion URL末尾のページIDを抽出する。
        /// </summary>
        [GeneratedRegex(@"(?:[0-9a-f]{32}|[0-9a-f]{8}(?:-[0-9a-f]{4}){3}-[0-9a-f]{12})/?$", RegexOptions.IgnoreCase)]
        private static partial Regex PageIdRegex();
    }
}
