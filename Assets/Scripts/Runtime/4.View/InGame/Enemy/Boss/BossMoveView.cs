using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.View.InGame.Sequence;
using KillChord.Runtime.View.InGame.UI;
using UnityEngine;
using UnityEngine.AI;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     毎フレームボス移動を更新するビュー。
    ///     EnemyMoveView のボス専用複製。移動意思は BossAIController に委譲する。
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class BossMoveView : MonoBehaviour, IGameplayControllable
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        public void Initialize(BossAIController bossAIController, Transform target)
        {
            _bossAIController = bossAIController;
            _target = target;
            _isPlaying = false;
        }

        /// <summary> ゲームプレイ開始処理。 </summary>
        public void StartGameplay()
        {
            _isPlaying = true;
        }

        /// <summary> ゲームプレイ停止処理。 </summary>
        public void StopGameplay()
        {
            _isPlaying = false;
            StopMoving();
            StopRotating();
        }

        /// <summary> 攻撃目標のTransformを取得する。 </summary>
        public Transform GetTargetTransform()
        {
            return _target;
        }

        /// <summary> 攻撃可能な位置まで移動する。 </summary>
        public void MoveToAttack()
        {
            if (!_isPlaying) return;
            if (!_navMeshAgent.isOnNavMesh || _target == null) return;

            EnemyMoveInstruction intruction = _bossAIController.GetMoveInstruction(transform.position, _target.position);
            if (intruction.ShouldMove)
            {
                _navMeshAgent.speed = intruction.MoveSpeed;
                _navMeshAgent.isStopped = false;
                _navMeshAgent.updateRotation = true;
                _navMeshAgent.SetDestination(intruction.Destination);
            }
        }

        /// <summary> 移動を停止する。 </summary>
        public void StopMoving()
        {
            if (!CanUseNavMeshAgent())
            {
                return;
            }
            _navMeshAgent.isStopped = true;
        }

        public void StopRotating()
        {
            if (_navMeshAgent == null || !_navMeshAgent.enabled) return;
            _navMeshAgent.updateRotation = false;
        }

        private NavMeshAgent _navMeshAgent;
        private Transform _target;
        private BossAIController _bossAIController;
        private bool _isPlaying;

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void OnDestroy()
        {
            if (_bossAIController == null) return;

            _bossAIController.OnAttackReserved -= PlayEffectReserved;
            _bossAIController.OnAttack -= PlayEffectHit;
            _bossAIController.On1BeatBefore -= On1BeatBefore;
            _bossAIController.On2BeatBefore -= On2BeatBefore;
            _bossAIController.Dispose();
        }

        /// <summary> 有効化処理。 </summary>
        public void Activate()
        {
            _bossAIController.OnAttackReserved += PlayEffectReserved;
            _bossAIController.OnAttack += PlayEffectHit;
            _bossAIController.On1BeatBefore += On1BeatBefore;
            _bossAIController.On2BeatBefore += On2BeatBefore;
        }

        /// <summary> 無効化処理。 </summary>
        public void Deactivate()
        {
            if (_bossAIController != null)
            {
                _bossAIController.OnAttackReserved -= PlayEffectReserved;
                _bossAIController.OnAttack -= PlayEffectHit;
                _bossAIController.On1BeatBefore -= On1BeatBefore;
                _bossAIController.On2BeatBefore -= On2BeatBefore;
            }
        }

        private bool CanUseNavMeshAgent()
        {
            return _navMeshAgent != null
                && _navMeshAgent.enabled
                && _navMeshAgent.isOnNavMesh;
        }

        private void PlayEffectReserved()
        {
            if (!_isPlaying) return;
            ParticleController.Instance.PlayParticleReserve(transform.position);
        }

        private void PlayEffectHit()
        {
            if (!_isPlaying) return;
            ParticleController.Instance.PlayParticle(transform.position);
            MoveToAttack();
        }

        private void On1BeatBefore()
        {
            StopMoving();
            StopRotating();
        }

        private void On2BeatBefore()
        {
        }
    }
}
