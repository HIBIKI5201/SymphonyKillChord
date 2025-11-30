using Mock.MusicBattle.Battle;
using System.Collections.Generic;
using UnityEngine;

namespace Mock.MusicBattle.Develop
{
    /// <summary>
    ///     カメラのデバッグ用のロックオンターゲットのコンテナ。
    /// </summary>
    public class LockOnTargetContainerForCamera : MonoBehaviour, ILockOnTargetContainer
    {
        public IReadOnlyList<Transform> Targets => _targets;

        [SerializeField]
        private List<Transform> _targets = new();
    }
}
