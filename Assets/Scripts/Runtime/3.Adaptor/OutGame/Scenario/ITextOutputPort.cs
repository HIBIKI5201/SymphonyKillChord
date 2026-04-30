using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor
{
    public interface ITextOutputPort
    {
        ValueTask ShowTextAsync(string message, CancellationToken ct);
    }
}
