using KillChord.Runtime.Adaptor.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     敵AI用ファサード：状態判定系。
    /// </summary>
    public class EnemyStateFacade : MonoBehaviour, IEnemyStateFacade
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="aiController"></param>
        /// <param name="target"></param>
        /// <param name="raycastDetectView"></param>
        /// <param name="battleState"></param>
        public void Initialize(EnemyAIController aiController, Transform target, EnemyRaycastDetectView raycastDetectView, EnemyBattleState battleState)
        {
            _aiController = aiController;
            _target = target;
            _raycastDetectView = raycastDetectView;
            _battleState = battleState;
        }
        /// <summary> 目標が自分の攻撃範囲内か </summary>
        public bool IsTargetInAttackRange => _aiController.IsPlayerInAttackRange(transform.position, _target.position);

        /// <summary> 目標と自分の間に障害物がないか </summary>
        public bool IsSightClearToAim => _raycastDetectView.CanRaycastHitTarget;

        /// <summary> 攻撃中であるか </summary>
        public bool IsAttacking => _aiController.IsAttacking;
        /// <summary> 硬直中か。 </summary>
        public bool IsStunned => _battleState.IsStunned;

        private EnemyAIController _aiController;
        private Transform _target;
        private EnemyRaycastDetectView _raycastDetectView;
        private EnemyBattleState _battleState;

        /// <summary>
        ///     硬直発生。
        /// </summary>
        public void Stunned()
        {
            CancelInvoke(nameof(StunRecover));
            // TODO 2秒後回復。一時。今後はAnimation Controllerで制御するはず
            Invoke(nameof(StunRecover), 2f);
        }
        /// <summary>
        ///     硬直回復。
        /// </summary>
        public void StunRecover()
        {
            _battleState.StunRecover();
        }
    }
}
