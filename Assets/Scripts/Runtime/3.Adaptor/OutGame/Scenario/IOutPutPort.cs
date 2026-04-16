using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor
{
    public interface IOutPutPort
    {
        public ValueTask ShowTextAsync(CancellationToken ct);
    }
}
