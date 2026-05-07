using System.Threading;
using System.Threading.Tasks;
namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    /// ILayerOutputPort の契約を定義します。
    /// </summary>
    public interface ILayerOutputPort
    {
        /// <summary>
        /// 蟇ｾ雎｡UI縺ｮ繝ｬ繧､繝､繝ｼ鬆・ｒ螟画峩縺励∪縺吶・        /// </summary>
        ValueTask SetLayerOrderAsync(string target, int order, CancellationToken ct);
    }
}
