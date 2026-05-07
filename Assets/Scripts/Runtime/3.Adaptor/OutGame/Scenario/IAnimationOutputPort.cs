using System.Threading;
using System.Threading.Tasks;
namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    /// IAnimationOutputPort の契約を定義します。
    /// </summary>
    public interface IAnimationOutputPort
    {
        ValueTask PlayAnimationAsync(string assetKey, CancellationToken ct);
    }
}
