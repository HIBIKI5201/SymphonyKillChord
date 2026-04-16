using UnityEngine;

namespace KillChord.Runtime.Application
{
    /// <summary>
    ///     敵から射線を通し、指定対象に直撃できるか判定するインタフェース。
    /// </summary>
    public interface IEnemyRaycastDetector
    {
        /// <summary>
        ///     敵からの射線が対象に直撃できるか判定する。
        /// </summary>
        /// <returns></returns>
        public bool CanRaycastHitTarget();
    }
}
