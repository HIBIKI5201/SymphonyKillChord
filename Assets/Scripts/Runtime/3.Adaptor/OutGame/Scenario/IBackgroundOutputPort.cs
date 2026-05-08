using System.Threading;
using System.Threading.Tasks;
namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// IBackgroundOutputPort の契約を定義します。
    /// </summary>
    public interface IBackgroundOutputPort
    {
        ValueTask ShowBackgroundAsync(string assetKey, CancellationToken ct);
    }
}
