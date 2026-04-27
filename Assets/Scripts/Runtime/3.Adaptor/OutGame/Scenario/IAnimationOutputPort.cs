using System.Threading;
using System.Threading.Tasks;
namespace KillChord.Runtime.Adaptor
{
    public interface IAnimationOutputPort
    {
        ValueTask PlayAnimationAsync(string animationId, CancellationToken ct);
    }
}
