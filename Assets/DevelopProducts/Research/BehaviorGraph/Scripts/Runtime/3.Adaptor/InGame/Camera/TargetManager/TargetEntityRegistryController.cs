using DevelopProducts.BehaviorGraph.Runtime.Application;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Character;

namespace DevelopProducts.BehaviorGraph.Runtime.Adaptor
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
