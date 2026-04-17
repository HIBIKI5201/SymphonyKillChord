

using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application
{
    public interface ITextAdvanceWaiter
    {
        public ValueTask WaitNextAsync(CancellationToken ct);
    }
}
