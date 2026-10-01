using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Text の出力契約を定義する。
    /// </summary>
    public interface ITextOutputPort
    {
        /// <summary>
        ///     話者と本文を表示する。
        /// </summary>
        ValueTask ShowTextAsync(string speaker, string message, CancellationToken ct);
    }
}
