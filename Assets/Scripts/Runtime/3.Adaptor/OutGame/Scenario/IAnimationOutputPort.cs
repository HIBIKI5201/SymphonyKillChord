using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Animation の出力契約を定義する。
    /// </summary>
    public interface IAnimationOutputPort
    {
        /// <summary>
        /// 指定された assetKey のアニメーション再生を開始する。
        /// </summary>
        /// <param name="assetKey">再生対象のアニメーションアセット識別子。</param>
        /// <param name="ct">処理を取り消すためのキャンセルトークン。</param>
        /// <returns>アニメーション再生開始処理を表す非同期タスク。</returns>
        /// <exception cref="OperationCanceledException">処理がキャンセルされた場合に発生する。</exception>
        ValueTask PlayAnimationAsync(string assetKey, CancellationToken ct);
    }
}
