using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application
{
    /// <summary>
    /// シナリオ再生完了通知の契約を定義します。
    /// </summary>
    public interface IScenarioCompletionNotifier
    {
        ValueTask NotifyCompletedAsync(bool skipped, CancellationToken ct);
    }
}
