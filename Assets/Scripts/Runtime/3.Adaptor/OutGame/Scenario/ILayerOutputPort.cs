using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// ILayerOutputPort の契約を定義します。
    /// </summary>
    public interface ILayerOutputPort
    {
        /// <summary>
        /// 対象 UI のレイヤー順を変更します。
        /// </summary>
        ValueTask SetLayerOrderAsync(string target, int order, CancellationToken ct);
    }
}
