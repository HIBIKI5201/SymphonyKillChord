using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Enemy.EnemyAIFacadeInterface;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.AIFacade
{
    /// <summary>
    ///     敵・ボス共通のAI用ファサード：状態判定系。
    ///     射程・射線・攻撃中の判定を共通で持ち、硬直の扱いは派生クラスに任せる。
    /// </summary>
    public abstract class StateFacadeBase : MonoBehaviour, IEnemyStateFacade
    {
        /// <summary> 目標が自分の攻撃範囲内か </summary>
        public bool IsTargetInAttackRange => IsReady && _aiController.IsPlayerInAttackRange(transform.position, _target.position);

        /// <summary> 目標と自分の間に障害物がないか </summary>
        public bool IsSightClearToAim => IsReady && _raycastDetectView.CanRaycastHitTarget;

        /// <summary> 攻撃中であるか </summary>
        public bool IsAttacking => IsReady && _aiController.IsAttacking;

        /// <summary> 硬直中か。 </summary>
        public abstract bool IsStunned { get; }

        /// <summary>
        ///     硬直発生。
        /// </summary>
        public abstract void Stunned();

        /// <summary>
        ///     硬直回復。
        /// </summary>
        public abstract void StunRecover();

        /// <summary> 攻撃対象。未初期化なら null。 </summary>
        protected Transform Target => _target;

        /// <summary> 射線判定に使う View。未初期化なら null。 </summary>
        protected EnemyRaycastDetectView RaycastDetectView => _raycastDetectView;

        private IEnemyAIController _aiController;
        private Transform _target;
        private EnemyRaycastDetectView _raycastDetectView;

        /// <summary> 初期化済みか。 </summary>
        private bool IsReady => _aiController != null && _target != null && _raycastDetectView != null;

        /// <summary>
        ///     共通の参照を設定する。派生クラスの Initialize から呼ぶ。
        /// </summary>
        /// <param name="aiController"> 攻撃の状態を持つAIコントローラー。 </param>
        /// <param name="target"> 攻撃対象。 </param>
        /// <param name="raycastDetectView"> 射線判定に使う View。 </param>
        protected void InitializeCommon(IEnemyAIController aiController, Transform target, EnemyRaycastDetectView raycastDetectView)
        {
            _aiController = aiController;
            _target = target;
            _raycastDetectView = raycastDetectView;
        }
    }
}
