using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// シナリオ再生完了通知の契約を定義する。
    /// </summary>
    public interface IScenarioCompletionNotifier
    {
        /// <summary>
        ///     シナリオの再生完了を通知する。
        /// </summary>
        ValueTask NotifyCompletedAsync(bool skipped, CancellationToken ct);
    }
}