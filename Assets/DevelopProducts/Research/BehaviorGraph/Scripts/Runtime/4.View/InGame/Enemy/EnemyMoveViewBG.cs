using DevelopProducts.BehaviorGraph.Runtime.Adaptor;
using DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame;
using DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Battle;
using DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Enemy;
using UnityEngine;
using UnityEngine.AI;

namespace DevelopProducts.BehaviorGraph.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     毎フレーム敵移動を更新するビュー。
    ///     敵の移動ロジックはEnemyMoveControllerに委譲される。
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMoveViewBG : MonoBehaviour
    {
        public void Initialize(EnemyAIControllerBG enemyAIController, Transform target, TargetManagerController targetManagerController, EnemyBattleStateBG enemyBattleState)
        {
            _lockOnTargetGateway = new(transform);
            _enemyAIController = enemyAIController;
            _target = target;
            _targetManagerController = targetManagerController;
            _targetManagerController.Register(_lockOnTargetGateway);
            _enemyAIController.OnAttackReserved += PlayEffectReserved;
            _enemyAIController.OnAttack += PlayEffectHit;
            _enemyBattleState = enemyBattleState;
        }

        public EnumEnemyStatus EnemyStatus { get; private set; }

        private NavMeshAgent _navMeshAgent;
        private Transform _target;
        private EnemyAIControllerBG _enemyAIController;
        private TargetManagerController _targetManagerController;
        private LockOnTargetGateway _lockOnTargetGateway;
        private EnemyBattleStateBG _enemyBattleState;

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        //private void Update()
        //{
        //    if (_enemyAIController != null && _target != null)
        //    {
        //        EnemyMoveInstruction intruction = _enemyAIController.Tick(transform.position, _target.position);
        //        ApplyMove(intruction);
        //    }
        //}

        private void OnDestroy()
        {
            if (_enemyAIController == null) return;

            _enemyAIController.OnAttackReserved -= PlayEffectReserved;
            _enemyAIController.OnAttack -= PlayEffectHit;
            _enemyAIController.Dispose();
            _targetManagerController.Unregister(_lockOnTargetGateway);
            _lockOnTargetGateway.Dispose();
        }

        public void ApplyMoveFromBG(EnemyMoveInstruction intruction)
        {
            ApplyMove(intruction);
        }

        private void ApplyMove(EnemyMoveInstruction intruction)
        {
            if (!_navMeshAgent.isOnNavMesh) return;

            if (intruction.ShouldMove)
            {
                _navMeshAgent.speed = intruction.MoveSpeed;
                _navMeshAgent.isStopped = false;
                _navMeshAgent.SetDestination(intruction.Destination);

                _enemyBattleState.RemoveEnemyStatus(EnumEnemyStatus.TargetInSight);
                _enemyBattleState.RemoveEnemyStatus(EnumEnemyStatus.TargetAimable);
                _enemyBattleState.AddEnemyStatus(EnumEnemyStatus.Moving);

                return;
            }

            _navMeshAgent.isStopped = true;

            _enemyBattleState.RemoveEnemyStatus(EnumEnemyStatus.Moving);
            _enemyBattleState.AddEnemyStatus(EnumEnemyStatus.TargetInSight);
            _enemyBattleState.AddEnemyStatus(EnumEnemyStatus.TargetAimable);
        }
        private void PlayEffectReserved()
        {
            ParticleController.Instance.PlayParticleReserve(transform.position);
        }

        private void PlayEffectHit()
        {
            ParticleController.Instance.PlayParticle(transform.position);
        }

    }

}
