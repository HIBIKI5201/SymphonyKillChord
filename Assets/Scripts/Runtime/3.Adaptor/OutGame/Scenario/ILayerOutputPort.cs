using System.Threading;
using System.Threading.Tasks;
namespace KillChord.Runtime.Adaptor
{
    public interface ILayerOutputPort
    {
        ValueTask SetLayerOrderAsync(string target, int order, CancellationToken ct);
    }
}
