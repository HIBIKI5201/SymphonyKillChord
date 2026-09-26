using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Fade の表示反映契約を定義する。
    /// </summary>
    public interface IFadeViewSink
    {
        /// <summary>
        ///     フェードの状態を表示に反映する。
        /// </summary>
        ValueTask SetFadeAsync(in ScenarioFadeViewDTO dto, CancellationToken ct);
    }
}
