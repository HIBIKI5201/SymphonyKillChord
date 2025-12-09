using System.Collections.Generic;
using UnityEngine;

namespace Mock.MusicBattle.Battle
{
    public interface ILockOnTargetContainer
    {
        public Transform this[int index] => Targets.Count != 0 ? Targets[(index + Targets.Count) % Targets.Count] : null;
        public IReadOnlyList<Transform> Targets { get; }
        public IReadOnlyList<Transform> NearerTargets { get; }
    }
}
