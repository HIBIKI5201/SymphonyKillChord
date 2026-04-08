using KillChord.Runtime.Domain.InGame;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    public sealed class TargetManager : ITargetPositionsProvider, ITargetRegisteable
    {
        public IReadOnlyList<Vector3> TargetPositions => _targetPositions;

        public void Register(Transform target)
        {
            if (!_targets.Add(target))
            {
                Debug.LogWarning($"Target {target.name} is already registered.", target);
            }
        }
        public void Unregister(Transform target)
        {
            if (!_targets.Remove(target))
            {
                Debug.LogWarning($"Target {target.name} was not registered.", target);
            }
        }
        public void UpdatePositions()
        {
            _targetPositions.Clear();
            foreach (Transform target in _targets)
            {
                _targetPositions.Add(target.position);
            }
        }

        private readonly List<Vector3> _targetPositions = new();
        private readonly HashSet<Transform> _targets = new();
    }
}
