using KillChord.Runtime.Adaptor.InGame.Enemy;
using UnityEngine;
using UnityEngine.AI;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     毎フレーム敵移動を更新するビュー。
    ///     敵の移動ロジックはEnemyMoveControllerに委譲される。
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMoveView : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="enemyAIController"></param>
        /// <param name="target"></param>
        public void Initialize(EnemyAIController enemyAIController,
            Transform target)
        {
            _enemyAIController = enemyAIController;
            _target = target;
            _enemyAIController.OnAttackReserved += PlayEffectReserved;
            _enemyAIController.OnAttack += PlayEffectHit;
        }

        /// <summary>
        ///     攻撃目標のTransformを取得する。
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        ///     攻撃を予約するエフェクトを再生する。
        /// </summary>
        private void PlayEffectReserved()
        {
            ParticleController.Instance.PlayParticleReserve(transform.position);
        }
        /// <summary>
        ///     攻撃を実行するエフェクトを再生する。
        /// </summary>
        private void PlayEffectHit()
        {
            ParticleController.Instance.PlayParticle(transform.position);
        }
    }
}
