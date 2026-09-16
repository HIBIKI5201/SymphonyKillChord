using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Enemy.EnemyAIFacadeInterface;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.AIFacade
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

        /// <summary>
        ///     攻撃待機中、プレイヤーを向いたまま指定方向へ横移動する。
        /// </summary>
        /// <param name="direction"> 左右の向きを表す符号。 </param>
        public void StrafeWhileWaiting(int direction)
        {
            _moveView?.StrafeWhileWaiting(direction);
        }

        /// <summary>
        ///     横移動だけを終了し、他の移動指示には干渉しない。
        /// </summary>
        public void StopStrafing()
        {
            _moveView?.StopStrafing();
        }

        private EnemyMoveView _moveView;
    }
}
