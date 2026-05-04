using UnityEngine;

namespace KillChord.Runtime.Domain.InGame
{
    public interface ILockOnTarget
    {
        public Vector3 Position { get; }
        public bool IsAlive { get; }
    }
}
