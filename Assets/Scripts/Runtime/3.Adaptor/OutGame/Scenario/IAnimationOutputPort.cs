using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor
{
    public interface IAnimationOutputPort
    {
        ValueTask PlayAnimationAsync(string animationKey, CancellationToken ct);
    }
}
