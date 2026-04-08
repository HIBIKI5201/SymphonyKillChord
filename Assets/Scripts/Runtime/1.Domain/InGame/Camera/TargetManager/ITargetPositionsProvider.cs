using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame
{
    public interface ITargetPositionsProvider
    {
        public IReadOnlyList<Vector3> TargetPositions { get; }
        public void UpdatePositions();
    }
}
