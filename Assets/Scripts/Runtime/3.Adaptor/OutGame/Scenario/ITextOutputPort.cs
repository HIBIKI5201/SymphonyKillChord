using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Text の出力契約を定義する。
    /// </summary>
    public interface ITextOutputPort
    {
        ValueTask ShowTextAsync(string message, CancellationToken ct);
    }
}