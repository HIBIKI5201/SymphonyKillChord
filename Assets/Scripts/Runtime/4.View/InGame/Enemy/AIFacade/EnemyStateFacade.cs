using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    public class EnemyStateFacade : MonoBehaviour, IEnemyStateFacade
    {
        public void Initialize(EnemyAIController aiController, Transform target, EnemyRaycastDetectView raycastDetectView, EnemyBattleState battleState)
        {
            _aiController = aiController;
            _target = target;
            _raycastDetectView = raycastDetectView;
            _battleState = battleState;
        }
        public bool IsTargetInAttackRange => _aiController.IsPlayerInAttackRange(transform.position, _target.position);

        public bool IsSightClearToAim => _raycastDetectView.CanRaycastHitTarget;

        public bool IsAttacking => _aiController.IsAttacking;
        public bool IsStunned => _battleState.IsStunned;

        private EnemyAIController _aiController;
        private Transform _target;
        private EnemyRaycastDetectView _raycastDetectView;
        private EnemyBattleState _battleState;

        /// <summary>
        ///     【デバッグ用】硬直発生。
        /// </summary>
        public void Stunned()
        {
            CancelInvoke(nameof(StunRecover));
            // TODO 2秒後回復。一時。今後はAnimation Controllerで制御するはず
            Invoke(nameof(StunRecover), 2f);
        }
        /// <summary>
        ///     【デバッグ用】硬直回復。
        /// </summary>
        public void StunRecover()
        {
            _battleState.StunRecover();
        }
    }
}
