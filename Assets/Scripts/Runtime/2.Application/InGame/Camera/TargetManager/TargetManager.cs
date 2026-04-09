using KillChord.Runtime.Domain.InGame;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame
{
    public sealed class TargetManager
    {

        public int Count => _targets.Count;
        public void Register(ILockOnTarget target)
        {
            if (_targets.Add(target))
                return;
            Debug.LogWarning($"Target {target} is already registered.");
        }
        public void Unregister(ILockOnTarget target)
        {
            if (_targets.Remove(target))
                return;
            Debug.LogWarning($"Target {target} is not registered.");
        }

        public IEnumerable<ILockOnTarget> GetTargets => _targets;

        private readonly HashSet<ILockOnTarget> _targets = new();
    }
}
