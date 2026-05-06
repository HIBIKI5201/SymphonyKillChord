using System.Threading;
using System.Threading.Tasks;
namespace KillChord.Runtime.Adaptor
{
    public interface IPortraitOutputPort
    {
        ValueTask ShowPortraitAsync(
            string slot,
            string assetKey,
            float positionX,
            float positionY,
            float scale,
            bool visible,
            CancellationToken ct);
    }
}
