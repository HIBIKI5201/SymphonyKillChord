using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// IPortraitOutputPort の契約を定義します。
    /// </summary>
    public interface IPortraitOutputPort
    {
        /// <summary>
        /// 立ち絵を指定スロットへ表示または更新します。
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
