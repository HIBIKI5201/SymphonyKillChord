using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Portrait の出力契約を定義する。
    /// </summary>
    public interface IPortraitOutputPort
    {
        /// <summary>
        /// </summary>
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