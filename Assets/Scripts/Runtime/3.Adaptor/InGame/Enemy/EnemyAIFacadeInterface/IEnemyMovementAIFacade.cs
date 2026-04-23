using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     敵AI用ファサード：移動系。
    /// </summary>
    public interface IEnemyMovementAIFacade
    {
        /// <summary>
        ///     指示：目標を追跡する。
        /// </summary>
        public void ChaseTarget();
        /// <summary>
        ///     指示：追跡を停止する。
        /// </summary>
        public void StopChasing();
    }
}
