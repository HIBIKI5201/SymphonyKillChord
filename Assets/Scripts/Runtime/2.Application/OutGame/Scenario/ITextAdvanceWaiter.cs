
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// テキスト送り入力を待機する契約を定義する。
    /// </summary>
    public interface ITextAdvanceWaiter
    {
        ValueTask WaitNextAsync(CancellationToken ct);
    }
}