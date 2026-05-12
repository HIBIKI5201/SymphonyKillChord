using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Animation の出力契約を定義する。
    /// </summary>
    public interface IAnimationOutputPort
    {
        ValueTask PlayAnimationAsync(string assetKey, CancellationToken ct);
    }
}