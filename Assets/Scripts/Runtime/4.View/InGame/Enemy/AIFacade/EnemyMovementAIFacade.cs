using KillChord.Runtime.Adaptor.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     敵AI用ファサード：移動系。
    /// </summary>
    public class EnemyMovementAIFacade : MonoBehaviour, IEnemyMovementAIFacade
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="moveView"></param>
        public void Initialize(EnemyMoveView moveView)
        {
            _moveView = moveView;
        }
        /// <summary>
        ///     指示：攻撃可能な位置に移動する。
        /// </summary>
        public void MoveToAttack()
        {
            _moveView.MoveToAttack();
        }
        /// <summary>
        ///     指示：移動を停止する。
        /// </summary>
        public void StopMoving()
        {
            _moveView.StopMoving();
        }

        private EnemyMoveView _moveView;
    }
}
