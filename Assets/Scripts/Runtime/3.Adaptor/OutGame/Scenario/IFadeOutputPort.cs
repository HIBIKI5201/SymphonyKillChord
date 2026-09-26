using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Fade の出力契約を定義する。
    /// </summary>
    public interface IFadeOutputPort
    {
        /// <summary>
        ///     対象を指定した範囲と時間でフェードさせる。
        /// </summary>
        ValueTask FadeAsync(
            FadeTarget target,
            FadeMode mode,
            float start,
            float end,
            float duration,
            CancellationToken ct);
    }
}
