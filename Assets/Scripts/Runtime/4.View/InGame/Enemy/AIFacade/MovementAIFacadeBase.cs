using KillChord.Runtime.Adaptor.InGame.Enemy.EnemyAIFacadeInterface;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.AIFacade
{
    /// <summary>
    ///     敵・ボス共通のAI用ファサード：移動系。
    /// </summary>
    /// <typeparam name="TMoveView"> 移動を担当する View の型。 </typeparam>
    public abstract class MovementAIFacadeBase<TMoveView> : MonoBehaviour, IEnemyMovementAIFacade
        where TMoveView : class, IEnemyMoveView
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="moveView"> 移動を担当する View。 </param>
        public void Initialize(TMoveView moveView)
        {
            _moveView = moveView;
        }

        /// <summary>
        ///     指示：攻撃可能な位置に移動する。
        /// </summary>
        public void MoveToAttack()
        {
            _moveView?.MoveToAttack();
        }

        /// <summary>
        ///     指示：移動を停止する。
        /// </summary>
        public void StopMoving()
        {
            _moveView?.StopMoving();
        }

        /// <summary> 移動を担当する View。未初期化なら null。 </summary>
        protected TMoveView MoveView => _moveView;

        private TMoveView _moveView;
    }
}
