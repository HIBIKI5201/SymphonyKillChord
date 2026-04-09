using KillChord.Runtime.Domain.InGame;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame
{
    public sealed class LockOnTargetGateway : ILockOnTarget, IDisposable
    {
        public LockOnTargetGateway(Transform fromTarget)
        {
            _cache = fromTarget;
            _isDisposed = false;
        }

        public Vector3 Position => IsAlive ? _cache.position : Vector3.zero;
        public bool IsAlive => !_isDisposed && _cache != null;

        public void Dispose()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(LockOnTargetGateway));
            }
            _cache = null;
            _isDisposed = true;
        }
        private Transform _cache;
        private bool _isDisposed;
    }
}
