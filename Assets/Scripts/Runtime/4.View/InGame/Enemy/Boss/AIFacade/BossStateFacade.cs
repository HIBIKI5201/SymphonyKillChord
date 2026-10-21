using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Enemy.EnemyAIFacadeInterface;
using KillChord.Runtime.View.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ボスAI用ファサード：状態判定系。EnemyStateFacade のボス専用複製。
    ///     ボスはスタンしない仕様のため、硬直系のメンバーは何もしない。
    /// </summary>
    public class BossStateFacade : MonoBehaviour, IEnemyStateFacade
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        public void Initialize(BossAIController aiController, Transform target, EnemyRaycastDetectView raycastDetectView)
        {
            _aiController = aiController;
            _target = target;
            _raycastDetectView = raycastDetectView;
        }

        /// <summary> 初期化済みか。 </summary>
        private bool IsReady => _aiController != null && _target != null && _raycastDetectView != null;

        /// <summary> 目標が自分の攻撃範囲内か </summary>
        public bool IsTargetInAttackRange => IsReady && _aiController.IsPlayerInAttackRange(transform.position, _target.position);

        /// <summary> 目標と自分の間に障害物がないか </summary>
        public bool IsSightClearToAim => IsReady && _raycastDetectView.CanRaycastHitTarget;

        /// <summary> 攻撃中であるか </summary>
        public bool IsAttacking => IsReady && _aiController.IsAttacking;

        /// <summary> 硬直中か。ボスはスタンしないため常に false。 </summary>
        public bool IsStunned => false;

        private BossAIController _aiController;
        private Transform _target;
        private EnemyRaycastDetectView _raycastDetectView;

        /// <summary> 硬直発生。ボスはスタンしないため何もしない。 </summary>
        public void Stunned()
        {
        }

        /// <summary> 硬直回復。ボスはスタンしないため何もしない。 </summary>
        public void StunRecover()
        {
        }
    }
}
