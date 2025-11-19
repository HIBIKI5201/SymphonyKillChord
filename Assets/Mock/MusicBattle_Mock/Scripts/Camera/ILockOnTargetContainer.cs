using System.Collections.Generic;
using UnityEngine;

namespace Mock.MusicBattle.Camera
{
    public interface ILockOnTargetContainer 
    {
        public Transform this[int index] => Targets[index % Targets.Count];
        public IReadOnlyList<Transform> Targets { get; }
    }
}
