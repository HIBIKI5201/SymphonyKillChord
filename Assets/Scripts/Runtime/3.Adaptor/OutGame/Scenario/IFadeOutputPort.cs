using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor
{
    public interface IFadeOutputPort
    {
        ValueTask FadeAsync(float start, float end, float duration, CancellationToken ct);
    }
}
