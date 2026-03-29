using KillChord.Runtime.Adaptor;
using UnityEngine;
using UnityEngine.AI;

namespace KillChord.Runtime.View.InGame
{
    /// <summary>
    ///     毎フレーム敵移動を更新するビュー。
    ///     敵の移動ロジックはEnemyMoveControllerに委譲される。
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMoveView : MonoBehaviour
    {
        public void Initialize(EnemyAIController enemyAIController, Transform target)
        {
            _enemyAIController = enemyAIController;
            _target = target;
            _enemyAIController.OnAttackReserved += PlayEffectReserved;
            _enemyAIController.OnAttack += PlayEffectHit;
        }

        private NavMeshAgent _navMeshAgent;
        private Transform _target;
        private EnemyAIController _enemyAIController;

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (_enemyAIController != null && _target != null)
            {
                EnemyMoveInstruction intruction = _enemyAIController.Tick(transform.position, _target.position);
                ApplyMove(intruction);
            }
        }

        private void OnDestroy()
        {
            if (_enemyAIController == null) return;

            _enemyAIController.OnAttackReserved -= PlayEffectReserved;
            _enemyAIController.OnAttack -= PlayEffectHit;
            _enemyAIController.Dispose();
        }

        private void ApplyMove(EnemyMoveInstruction intruction)
        {
            if (!_navMeshAgent.isOnNavMesh) return;

            if (intruction.ShouldMove)
            {
                _navMeshAgent.speed = intruction.MoveSpeed;
                _navMeshAgent.isStopped = false;
                _navMeshAgent.SetDestination(intruction.Destination);
                return;
            }

            _navMeshAgent.isStopped = true;
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
