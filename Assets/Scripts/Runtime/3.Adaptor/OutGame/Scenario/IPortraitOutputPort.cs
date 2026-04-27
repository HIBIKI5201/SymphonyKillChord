using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor
{
    public interface IPortraitOutputPort
    {
        ValueTask ShowPortraitAsync(string slotId, string assetKey, CancellationToken ct);
    }
}
