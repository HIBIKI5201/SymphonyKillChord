using KillChord.Runtime.Application;
using KillChord.Runtime.Domain.InGame;
using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Adaptor
{
    public class TargetEntityRegistryController
    {
        public TargetEntityRegistryController(TargetEntityRegistry registry)
        {
            _registry = registry;
        }

        public void RegisterTargetEntity(ILockOnTarget lockOnTarget, CharacterEntity entity)
        {
            _registry.Register(lockOnTarget, entity);
        }

        public void UnregisterTargetEntity(ILockOnTarget lockOnTarget)
        {
            _registry.Unregister(lockOnTarget);
        }

        public bool GetTargetEntity(ILockOnTarget lockOnTarget, out CharacterEntity entity)
        {
            return _registry.TryGetEntity(lockOnTarget, out entity);
        }

        private TargetEntityRegistry _registry;
    }
}
