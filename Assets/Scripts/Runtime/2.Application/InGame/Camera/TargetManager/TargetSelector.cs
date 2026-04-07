using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Camera
{
    public sealed class TargetSelector
    {
        public TargetSelector(ITargetPositionsProvider provider)
        {
            _targetPositionsProvider = provider;
        }

        public bool TryGetTargetPosition(out Vector3 result)
        {
            result = Vector3.zero;
            _targetPositionsProvider.UpdatePositions();
            if (_targetPositionsProvider.TargetPositions.Count <= 0)
                return false;
            result = _targetPositionsProvider.TargetPositions[0];
            return true;
        }

        private readonly ITargetPositionsProvider _targetPositionsProvider;
    }
}
