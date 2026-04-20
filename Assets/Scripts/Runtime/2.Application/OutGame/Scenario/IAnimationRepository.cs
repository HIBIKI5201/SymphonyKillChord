using UnityEngine;

namespace KillChord.Runtime.Application
{
    public interface IAnimationRepository
    {
        bool TryFindById(string id, out AnimationClip animationClip);
    }
}
