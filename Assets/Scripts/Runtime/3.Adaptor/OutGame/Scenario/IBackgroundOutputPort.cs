using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public interface IBackgroundOutputPort
    {
        ValueTask ShowBackgroundAsync(Sprite background, CancellationToken ct);
    }
}
