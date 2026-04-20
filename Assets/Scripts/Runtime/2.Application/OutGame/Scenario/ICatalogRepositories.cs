using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public interface IBackgroundRepository
    {
        bool TryFindById(string id, out BackgroundDefinition definition);
    }

    public interface IAnimationRepository
    {
        bool TryFindById(string id, out AnimationDefinition definition);
    }
}
