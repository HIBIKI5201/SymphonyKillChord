using KillChord.Runtime.Application.InGame.Camera;
using KillChord.Runtime.Domain.InGame.Character;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public class TargetSelectorController
    {
        public TargetSelectorController(TargetSelector selector, TargetEntityRegistryController registryController)
        {
            _selector = selector;
            _registryController = registryController;
        }

        public void ChangeTarget(in Vector3 playerPosition, in Vector3 direction)
        {
            _selector.ChangeTarget(playerPosition, direction);
        }

        public bool TryGetCurrentTargetEntity(out CharacterEntity entity)
        {
            entity = null;

            if(!_selector.TryGetCurrentTarget(out var target))
                return false;

            return _registryController.GetTargetEntity(target, out entity);
        }

        private readonly TargetSelector _selector;
        private readonly TargetEntityRegistryController _registryController;
    }
}
