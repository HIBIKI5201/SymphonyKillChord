using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public interface IAnimationRepository
    {
        bool TryFindById(string id, out AnimationDefinition definition);
    }
}
