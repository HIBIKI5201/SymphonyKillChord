using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Enemy;
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

        public Transform GetTargetTransform()
        {
            return _target;
        }

        /// <summary>
        ///     攻撃可能な位置まで移動する。
        /// </summary>
        public void MoveToAttack()
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
        ///     移動を停止する。
        /// </summary>
        public void StopMoving()
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
