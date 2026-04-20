using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public interface IAnimationOutputPort
    {
        ValueTask PlayAnimationAsync(AnimationClip animationClip, CancellationToken ct);
    }
}
