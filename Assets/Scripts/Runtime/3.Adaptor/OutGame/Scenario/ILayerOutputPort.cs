using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Layer の出力契約を定義する。
    /// </summary>
    public interface ILayerOutputPort
    {
        /// <summary>
        /// </summary>
        ValueTask SetLayerOrderAsync(string target, int order, CancellationToken ct);
    }
}