using System.Threading;
using System.Threading.Tasks;
namespace KillChord.Runtime.Adaptor
{
    public interface IBackgroundOutputPort
    {
        ValueTask ShowBackgroundAsync(string assetKey, CancellationToken ct);
    }
}
