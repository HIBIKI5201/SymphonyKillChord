using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Fade の出力契約を定義する。
    /// </summary>
    public interface IFadeOutputPort
    {
        ValueTask FadeAsync(float start, float end, float duration, CancellationToken ct);
    }
}