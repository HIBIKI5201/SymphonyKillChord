using KillChord.Runtime.Domain;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    public sealed class TargetManager : ITargetPositionsProvider
    {
        public IReadOnlyList<Vector3> TargetPositions => _targetPositions;

        public void RegisterTarget(Transform target)
        {
            if (!_targets.Add(target))
            {
                Debug.LogWarning($"Target {target.name} is already registered.", target);
            }
        }
        public void UnregisterTarget(Transform target)
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
