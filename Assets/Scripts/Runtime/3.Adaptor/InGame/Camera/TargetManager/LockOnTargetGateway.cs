using KillChord.Runtime.Domain.InGame;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame
{
    public sealed class LockOnTargetGateway : ILockOnTarget
    {
        public LockOnTargetGateway(Transform fromTarget)
        {
            _cache = fromTarget;
        }

        public Vector3 Position => _cache.position;

        private readonly Transform _cache;
    }
}
