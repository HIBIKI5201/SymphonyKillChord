using System.Collections.Generic;
using UnityEngine;

namespace Mock.MusicBattle.Battle
{
    public interface ILockOnTargetContainer
    {
        public Transform this[int index] => Targets[(index + Targets.Count) % Targets.Count];
        public IReadOnlyList<Transform> Targets { get; }
    }
}
