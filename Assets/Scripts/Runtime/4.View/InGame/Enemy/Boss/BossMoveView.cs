using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.View.InGame.Sequence;
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

        [SerializeField, Tooltip("攻撃ヒット時に再生するエフェクトPrefab。")]
        private ParticleSystem _attackHitEffectPrefab;

        [SerializeField, Tooltip("攻撃予約時に再生するエフェクトPrefab。")]
        private ParticleSystem _attackReserveEffectPrefab;

        private NavMeshAgent _navMeshAgent;
        private Transform _target;
        private BossAIController _bossAIController;
        private ParticleSystem _attackHitEffectInstance;
        private ParticleSystem _attackReserveEffectInstance;
        private bool _isPlaying;

        /// <summary>
        ///     初期化時に必要な参照を取得します。
        /// </summary>
        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            InitializeAttackEffects();
        }

        /// <summary>
        ///     無効化時に再利用用のエフェクト状態を初期化します。
        /// </summary>
        private void OnDisable()
        {
            ResetAttackEffects();
        }

        /// <summary>
        ///     破棄時に購読解除とController破棄を行います。
        /// </summary>
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
            PlayAttackEffect(_attackReserveEffectInstance);
        }

        /// <summary>
        ///     攻撃時の演出を再生します。
        /// </summary>
        private void PlayEffectHit()
        {
            if (!_isPlaying) return;
            PlayAttackEffect(_attackHitEffectInstance);
            MoveToAttack();
        }

        /// <summary>
        ///     攻撃1拍前の停止処理を行います。
        /// </summary>
        private void On1BeatBefore()
        {
            StopMoving();
            StopRotating();
        }

        /// <summary>
        ///     攻撃2拍前の処理を行います。
        /// </summary>
        private void On2BeatBefore()
        {
        }

        /// <summary>
        ///     ボス専用の攻撃エフェクトを生成します。
        /// </summary>
        private void InitializeAttackEffects()
        {
            _attackHitEffectInstance = CreateAttackEffectInstance(_attackHitEffectPrefab);
            _attackReserveEffectInstance = CreateAttackEffectInstance(_attackReserveEffectPrefab);
        }

        /// <summary>
        ///     攻撃エフェクトのインスタンスを生成します。
        /// </summary>
        /// <param name="effectPrefab"> 生成元のエフェクトPrefab。 </param>
        /// <returns> 生成したエフェクトインスタンス。 </returns>
        private ParticleSystem CreateAttackEffectInstance(ParticleSystem effectPrefab)
        {
            if (effectPrefab == null)
            {
                return null;
            }

            ParticleSystem instance = Instantiate(effectPrefab, transform);
            instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            instance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return instance;
        }

        /// <summary>
        ///     指定した攻撃エフェクトを現在位置で再生します。
        /// </summary>
        /// <param name="effectInstance"> 再生するエフェクト。 </param>
        private void PlayAttackEffect(ParticleSystem effectInstance)
        {
            if (effectInstance == null)
            {
                return;
            }

            effectInstance.transform.position = transform.position;
            effectInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            effectInstance.Play();
        }

        /// <summary>
        ///     攻撃エフェクトの再生状態を初期化します。
        /// </summary>
        private void ResetAttackEffects()
        {
            if (_attackHitEffectInstance != null)
            {
                _attackHitEffectInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (_attackReserveEffectInstance != null)
            {
                _attackReserveEffectInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
