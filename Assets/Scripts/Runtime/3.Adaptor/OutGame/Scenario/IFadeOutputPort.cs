using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    /// IFadeOutputPort の契約を定義します。
    /// </summary>
    public interface IFadeOutputPort
    {
        ValueTask FadeAsync(float start, float end, float duration, CancellationToken ct);
    }
}
