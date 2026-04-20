using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     敵の射線判定ViewModel。
    /// </summary>
    public interface IEnemyRaycastDetectViewModel
    {
        /// <summary>
        ///     敵から対象への射線が直撃できるか判定する。
        /// </summary>
        /// <returns></returns>
        public bool CanRaycastHitTarget { get; }
    }
}
