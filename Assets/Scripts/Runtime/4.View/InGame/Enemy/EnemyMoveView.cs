using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame;
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
        public void Initialize(EnemyAIController enemyAIController,
            Transform target)
        {
            _enemyAIController = enemyAIController;
            _target = target;
            _enemyAIController.OnAttackReserved += PlayEffectReserved;
            _enemyAIController.OnAttack += PlayEffectHit;
        }

        /// <summary>
        ///     攻撃対象を追跡する。
        /// </summary>
        public void ChaseTarget()
        {
            if (!_navMeshAgent.isOnNavMesh || _target == null) return;
            EnemyMoveInstruction intruction = _enemyAIController.GetMoveInstruction(transform.position, _target.position);
            if (intruction.ShouldMove)
            {
                _navMeshAgent.speed = intruction.MoveSpeed;
                _navMeshAgent.isStopped = false;
                _navMeshAgent.SetDestination(intruction.Destination);
            }
        }

        /// <summary>
        ///     攻撃対象への追跡を停止する。
        /// </summary>
        public void StopChasing()
        {
            _navMeshAgent.isStopped = true;
        }

        private NavMeshAgent _navMeshAgent;
        private Transform _target;
        private EnemyAIController _enemyAIController;

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void OnDestroy()
        {
            if (_enemyAIController == null) return;

            _enemyAIController.OnAttackReserved -= PlayEffectReserved;
            _enemyAIController.OnAttack -= PlayEffectHit;
            _enemyAIController.Dispose();
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
