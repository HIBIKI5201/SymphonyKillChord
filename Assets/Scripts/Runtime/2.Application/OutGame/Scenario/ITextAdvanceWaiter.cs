
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// テキスト送り入力を待機する契約を定義する。
    /// </summary>
    public interface ITextAdvanceWaiter
    {
        /// <summary>
        ///     現在のテキスト用の入力受付を開き、一回の送りまたはキャンセルまで待機する。
        ///     表示開始前に呼び、テキスト終了時にキャンセルして次の受付へ入力を持ち越さない。
        /// </summary>
        ValueTask WaitNextAsync(CancellationToken ct);
    }
}