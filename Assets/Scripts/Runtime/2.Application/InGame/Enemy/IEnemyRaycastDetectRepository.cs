using UnityEngine;

namespace KillChord.Runtime.Application
{
    /// <summary>
    ///     敵から射線を通し、目標に直撃できるか判定する。
    /// </summary>
    public interface IEnemyRaycastDetectRepository
    {
        public bool CanRaycastHitTarget { get; }
    }
}
