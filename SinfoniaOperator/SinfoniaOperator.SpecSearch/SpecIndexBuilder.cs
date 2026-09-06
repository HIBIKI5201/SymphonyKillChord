using System;
using System.Threading;
using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator.SpecSearch
{
    /// <summary>
    ///     Markdownの分割、埋め込み生成、インデックス保存を調停する。
    /// </summary>
    public sealed class SpecIndexBuilder
    {
        /// <summary>
        ///     チャンク生成器と埋め込みモデルを受け取る。
        /// </summary>
        /// <param name="chunker">仕様書のチャンク生成器。</param>
        /// <param name="embeddingModel">埋め込みモデル。</param>
        public SpecIndexBuilder(MarkdownChunker chunker, IEmbeddingModel embeddingModel)
        {
            _chunker = chunker ?? throw new ArgumentNullException(nameof(chunker));
            _embeddingModel = embeddingModel ?? throw new ArgumentNullException(nameof(embeddingModel));
        }

        /// <summary>
        ///     すべての仕様書チャンクを埋め込み、インデックスへ保存する。
        /// </summary>
        /// <param name="outputPath">インデックスの保存先パス。</param>
        /// <param name="cancellationToken">処理のキャンセルトークン。</param>
        /// <returns>生成された検索インデックス。</returns>
        public async Task<SpecIndex> BuildAndSaveAsync(string outputPath, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
            SpecChunkRecord[] chunks = _chunker.ChunkAll();
            SpecChunkRecord[] embeddedChunks = new SpecChunkRecord[chunks.Length];
            for (int index = 0; index < chunks.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                SpecChunkRecord chunk = chunks[index];
                string embeddingText = $"{PASSAGE_PREFIX}{chunk.HeadingBreadcrumb}\n{chunk.Text}";
                float[] vector = await _embeddingModel.EmbedAsync(embeddingText);
                embeddedChunks[index] = new SpecChunkRecord(
                    chunk.SourceFile,
                    chunk.HeadingBreadcrumb,
                    chunk.NotionUrl,
                    chunk.Text,
                    vector);
            }

            SpecIndex specIndex = new(embeddedChunks);
            specIndex.Save(outputPath);
            return specIndex;
        }

        private const string PASSAGE_PREFIX = "passage: ";

        private readonly MarkdownChunker _chunker;
        private readonly IEmbeddingModel _embeddingModel;
    }
}
