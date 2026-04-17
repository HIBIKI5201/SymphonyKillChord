using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Adaptor
{
    public interface IOutPutPort
    {
        public ValueTask ShowTextAsync(string message, CancellationToken ct);
    }
}
