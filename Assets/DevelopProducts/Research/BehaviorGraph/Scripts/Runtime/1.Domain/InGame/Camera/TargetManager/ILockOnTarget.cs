using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Domain.InGame
{
    public interface ILockOnTarget
    {
        public Vector3 Position { get; }
        public bool IsAlive { get; }
    }
}
