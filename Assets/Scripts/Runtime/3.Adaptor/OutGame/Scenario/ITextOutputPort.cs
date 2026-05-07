using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    /// ITextOutputPort の契約を定義します。
    /// </summary>
    public interface ITextOutputPort
    {
        ValueTask ShowTextAsync(string message, CancellationToken ct);
    }
}
