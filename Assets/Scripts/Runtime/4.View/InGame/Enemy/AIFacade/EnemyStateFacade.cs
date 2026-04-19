using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Battle;
using UnityEngine;

namespace KillChord.Runtime.View
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
        public bool IsTargetInAttackRange => _aiController.IsPlayerInAttackRange(transform.position, _target.transform.position);

        public bool IsSightClearToAim => _raycastDetectView.CanRaycastHitTarget;

        public bool IsAttacking => _aiController.IsAttacking;
        // 実装待ち
        public bool IsStunned => _isStunned;

        private EnemyAIController _aiController;
        private Transform _target;
        private EnemyRaycastDetectView _raycastDetectView;
        private EnemyBattleState _battleState;

        private bool _isStunned = false;
        /// <summary>
        ///     【デバッグ用】被弾硬直回復。
        /// </summary>
        public void HitStunRecover()
        {
            _isStunned = false;
        }
        /// <summary>
        ///     【デバッグ用】被弾硬直発生。
        /// </summary>
        public void Stunned()
        {
            _isStunned = true;
        }
    }
}
