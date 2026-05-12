
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// テキスト送り待機の契約を定義します。
    /// </summary>
    public interface ITextAdvanceWaiter
    {
        ValueTask WaitNextAsync(CancellationToken ct);
    }
}
