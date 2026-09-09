using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator.SpecSearch
{
    /// <summary>
    ///     文字列を埋め込みベクトルへ変換するモデルを表す。
    /// </summary>
    public interface IEmbeddingModel
    {
        /// <summary>
        ///     文字列の埋め込みベクトルを生成する。
        /// </summary>
        /// <param name="text">埋め込み対象の文字列。</param>
        /// <returns>生成された埋め込みベクトル。</returns>
        public Task<float[]> EmbedAsync(string text);
    }
}
