using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     敵AI用ファサード：移動系。
    /// </summary>
    public interface IEnemyMovementAIFacade
    {
        /// <summary>
        ///     指示：攻撃可能な位置に移動する。
        /// </summary>
        public void MoveToAttack();
        /// <summary>
        ///     指示：移動を停止する。
        /// </summary>
        public void StopMoving();
    }
}
