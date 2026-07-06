using KillChord.Runtime.Adaptor.InGame.Enemy.EnemyAIFacadeInterface;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ボスAI用ファサード：移動系。EnemyMovementAIFacade のボス専用複製。
    /// </summary>
    public class BossMovementAIFacade : MonoBehaviour, IEnemyMovementAIFacade
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        public void Initialize(BossMoveView moveView)
        {
            _moveView = moveView;
        }

        /// <summary> 指示：攻撃可能な位置に移動する。 </summary>
        public void MoveToAttack()
        {
            if (_moveView == null) return;
            _moveView.MoveToAttack();
        }

        /// <summary> 指示：移動を停止する。 </summary>
        public void StopMoving()
        {
            if (_moveView == null) return;
            _moveView.StopMoving();
        }

        private BossMoveView _moveView;
    }
}
