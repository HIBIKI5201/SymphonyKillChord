using System.Collections.Generic;
using UnityEngine;

namespace Mock.MusicBattle.Camera
{
    public interface ILockOnTargetContainer 
    {
        public IReadOnlyList<Transform> Targets { get; }
    }
}
