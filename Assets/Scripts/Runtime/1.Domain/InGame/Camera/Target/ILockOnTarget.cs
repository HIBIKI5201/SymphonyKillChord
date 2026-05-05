using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Camera.Target
{
    public interface ILockOnTarget
    {
        public Vector3 Position { get; }
        public bool IsAlive { get; }
    }
}
