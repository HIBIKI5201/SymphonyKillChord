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
        ValueTask FadeAsync(
            FadeTarget target,
            FadeMode mode,
            float start,
            float end,
            float duration,
            CancellationToken ct);
    }
}
