using KillChord.Runtime.Domain.InGame.Camera.Target;
using KillChord.Runtime.Domain.InGame.Character;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Camera.Target
{
    public class TargetEntityRegistry
    {
        public void Register(ILockOnTarget target, CharacterEntity entity)
        {
            if (target == null)
            {
                Debug.LogError("Targetがnull");
                return;
            }

            if (entity == null)
            {
                Debug.LogError("Entityがnull");
                return;
            }

            _targetToEntity[target] = entity;
        }

        public void Unregister(ILockOnTarget target)
        {
            if (target == null)
            {
                Debug.LogError("Targetがnull");
                return;
            }
            _targetToEntity.Remove(target);
        }

        public bool TryGetEntity(ILockOnTarget target, out CharacterEntity entity)
        {
            if (target == null)
            {
                Debug.LogError("Targetがnull");
                entity = null;
                return false;
            }
            return _targetToEntity.TryGetValue(target, out entity);
        }

        private readonly Dictionary<ILockOnTarget, CharacterEntity> _targetToEntity = new();
    }
}
