using System;

namespace SinfoniaStudio.SinfoniaOperator.SpecSearch
{
    /// <summary>
    ///     仕様書の検索単位と埋め込みベクトルを保持する。
    /// </summary>
    public sealed class SpecChunkRecord
    {
        /// <summary>
        ///     仕様書の検索単位を生成する。
        /// </summary>
        /// <param name="sourceFile">リポジトリルートからの相対パス。</param>
        /// <param name="headingBreadcrumb">見出しのパンくず。</param>
        /// <param name="notionUrl">対応するNotionページのURL。</param>
        /// <param name="text">検索対象の本文。</param>
        /// <param name="vector">本文の埋め込みベクトル。</param>
        public SpecChunkRecord(
            string sourceFile,
            string headingBreadcrumb,
            string notionUrl,
            string text,
            float[] vector)
        {
            SourceFile = sourceFile ?? throw new ArgumentNullException(nameof(sourceFile));
            HeadingBreadcrumb = headingBreadcrumb ?? throw new ArgumentNullException(nameof(headingBreadcrumb));
            NotionUrl = notionUrl ?? throw new ArgumentNullException(nameof(notionUrl));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Vector = vector ?? throw new ArgumentNullException(nameof(vector));
        }

        /// <summary> リポジトリルートからの相対パス。 </summary>
        public string SourceFile { get; }

        /// <summary> 見出しのパンくず。 </summary>
        public string HeadingBreadcrumb { get; }

        /// <summary> 対応するNotionページのURL。 </summary>
        public string NotionUrl { get; }

        /// <summary> 検索対象の本文。 </summary>
        public string Text { get; }

        /// <summary> 本文の埋め込みベクトル。 </summary>
        public float[] Vector { get; }
    }
}
