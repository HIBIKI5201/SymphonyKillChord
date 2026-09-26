using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Background の出力契約を定義する。
    /// </summary>
    public interface IBackgroundOutputPort
    {
        /// <summary>
        ///     背景を表示する。
        /// </summary>
        ValueTask ShowBackgroundAsync(string assetKey, CancellationToken ct);
    }
}