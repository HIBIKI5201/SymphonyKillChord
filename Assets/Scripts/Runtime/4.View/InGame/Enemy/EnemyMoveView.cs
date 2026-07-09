using KillChord.Runtime.Adaptor.InGame.Animation;
using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.View.InGame.Sequence;
using KillChord.Runtime.View.InGame.UI;
using KillChord.Runtime.View.Persistent.Music;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     毎フレーム敵移動を更新するビュー。
    ///     敵の移動ロジックはEnemyMoveControllerに委譲される。
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMoveView : MonoBehaviour, IGameplayControllable
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="enemyAIController"></param>
        /// <param name="target"></param>
        public void Initialize(EnemyAIController enemyAIController,
            Transform target,
            ICharacterAnimationViewContext animationContext)

        {
            _enemyAIController = enemyAIController;
            _target = target;
            _characterAnimationViewModel = animationContext.ViewModel;
            _characterAnimationSignal = animationContext.Signal;
            _isPlaying = false;
        }

        /// <summary>
        ///     ゲームプレイの開始処理を行います。
        /// </summary>
        public void StartGameplay()
        {
            _isPlaying = true;
        }

        /// <summary>
        ///    ゲームプレイの停止処理を行います。
        /// </summary>
        public void StopGameplay()
        {
            _isPlaying = false;
            StopMoving();
            StopRotating();
            _characterAnimationViewModel?.SetVelocity(Vector2.zero);
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
        ///     初期地点まで移動する。
        /// </summary>
        /// <param name="target">移動先。</param>
        public async ValueTask<bool> MoveToTargetAysnc(Vector3 target, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (!CanUseNavMeshAgent())
            {
                return false;
            }

            _navMeshAgent.speed = 3f;
            _navMeshAgent.isStopped = false;
            _navMeshAgent.updateRotation = true;
            if (!_navMeshAgent.SetDestination(target))
            {
                _characterAnimationViewModel?.SetVelocity(Vector2.zero);
                return false;
            }

            while (CanUseNavMeshAgent() && _navMeshAgent.pathPending)
            {
                await Awaitable.NextFrameAsync(ct);
            }

            ct.ThrowIfCancellationRequested();

            if (!CanUseNavMeshAgent() || _navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                _characterAnimationViewModel?.SetVelocity(Vector2.zero);
                return false;
            }

            while (CanUseNavMeshAgent())
            {
                ct.ThrowIfCancellationRequested();

                Vector3 velocity = _navMeshAgent.desiredVelocity;
                _characterAnimationViewModel?.SetVelocity(new Vector2(velocity.x, velocity.z));

                if (!_navMeshAgent.pathPending
                    && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance
                    && (!_navMeshAgent.hasPath || _navMeshAgent.velocity.sqrMagnitude <= 0.01f))
                {
                    _characterAnimationViewModel?.SetVelocity(Vector2.zero);
                    return true;
                }

                PlayFootstepSound();
                await Awaitable.NextFrameAsync(ct);
            }

            _characterAnimationViewModel?.SetVelocity(Vector2.zero);
            return false;
        }

        /// <summary>
        ///     攻撃可能な位置まで移動する。
        /// </summary>
        public void MoveToAttack()
        {
            if (!_isPlaying) return;
            if (!_navMeshAgent.isOnNavMesh || _target == null) return;

            EnemyMoveInstruction intruction = _enemyAIController.GetMoveInstruction(transform.position, _target.position);
            if (intruction.ShouldMove)
            {
                _navMeshAgent.speed = intruction.MoveSpeed;
                _navMeshAgent.isStopped = false;
                _navMeshAgent.updateRotation = true;
                _navMeshAgent.SetDestination(intruction.Destination);

                Vector3 velocity = _navMeshAgent.desiredVelocity;
                _characterAnimationViewModel?.SetVelocity(new Vector2(velocity.x, velocity.z));

                PlayFootstepSound();
            }
            else
            {
                _characterAnimationViewModel?.SetVelocity(Vector2.zero);
            }
        }

        /// <summary>
        ///     移動を停止する。
        /// </summary>
        public void StopMoving()
        {
            if (!CanUseNavMeshAgent())
            {
                return;
            }

            _navMeshAgent.isStopped = true;
            _characterAnimationViewModel?.SetVelocity(Vector2.zero);
        }

        public void StopRotating()
        {
            if (_navMeshAgent == null || !_navMeshAgent.enabled) return;

            _navMeshAgent.updateRotation = false;
        }

        /// <summary>
        ///     有効化処理。
        /// </summary>
        public void Activate()
        {
            _enemyAIController.OnAttackReserved += PlayEffectReserved;
            _enemyAIController.OnAttack += PlayEffectHit;
            _enemyAIController.On1BeatBefore += On1BeatBefore;
            _enemyAIController.On2BeatBefore += On2BeatBefore;
        }

        /// <summary>
        ///     無効化処理。
        /// </summary>
        public void Deactivate()
        {
            if (_enemyAIController != null)
            {
                _enemyAIController.OnAttackReserved -= PlayEffectReserved;
                _enemyAIController.OnAttack -= PlayEffectHit;
                _enemyAIController.On1BeatBefore -= On1BeatBefore;
                _enemyAIController.On2BeatBefore -= On2BeatBefore;
            }
        }

        [SerializeField, Tooltip("敵攻撃SE用Source。歩兵、砲兵などの違いは敵Prefabごとに設定します。")]
        private SoundEffectSource _attackSoundSource;

        [SerializeField, Tooltip("足音SEの共通Source。床設定側にSourceがない場合に使用します。")]
        private SoundEffectSource _defaultFootstepSoundSource;

        [SerializeField, Tooltip("足音SEの共通CueName。床設定側にCueNameがない場合に使用します。")]
        private string _defaultFootstepCueName;

        [SerializeField, Tooltip("床素材ごとの足音SE設定。")]
        private FootstepSoundConfig[] _footstepSoundConfigs;

        [SerializeField, Tooltip("足元判定の開始位置オフセット。")]
        private Vector3 _footstepRayOffset;

        [SerializeField, Min(0.01f), Tooltip("足元判定の距離。")]
        private float _footstepRayDistance;

        [SerializeField, Min(0.01f), Tooltip("足音SEの再生間隔。")]
        private float _footstepInterval;

        [SerializeField, Tooltip("攻撃構えのanimationString")]
        private string _attackAttackReservedAnimation = "Enemy_AttackReserved";

        private float _lastFootstepTime;
        private NavMeshAgent _navMeshAgent;
        private Transform _target;
        private EnemyAIController _enemyAIController;
        private ICharacterAnimationViewModel _characterAnimationViewModel;
        private ICharacterAnimationSignal _characterAnimationSignal;
        private bool _isPlaying;
        private bool _isReservedAnimationPlaying;

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void OnDestroy()
        {
            if (_enemyAIController == null) return;

            Deactivate();
            _enemyAIController.Dispose();
        }

        /// <summary>
        ///     NavMeshAgentが使用可能かどうかを確認する。
        /// </summary>
        /// <returns> 使用可能であればtrue、そうでなければfalse。 </returns>
        private bool CanUseNavMeshAgent()
        {
            return _navMeshAgent != null
                && _navMeshAgent.enabled
                && _navMeshAgent.isOnNavMesh;
        }
        /// <summary>
        ///     攻撃を予約するエフェクトを再生する。
        /// </summary>
        private void PlayEffectReserved()
        {
            if (!_isPlaying) return;

            ParticleController.Instance.PlayParticleReserve(transform.position);
            PlayOneShot(_attackAttackReservedAnimation);
            _isReservedAnimationPlaying = true;
        }
        /// <summary>
        ///     攻撃を実行するエフェクトを再生する。
        /// </summary>
        private void PlayEffectHit()
        {
            if (!_isPlaying) return;

            // 構えアニメが再生中の場合、ここで明示的にキャンセル
            if (_isReservedAnimationPlaying)
            {
                // 構えアニメをキャンセルするために、速度をゼロにして状態をリセット
                _characterAnimationViewModel?.SetVelocity(Vector2.zero);
                _isReservedAnimationPlaying = false;
            }

            ParticleController.Instance.PlayParticle(transform.position);
            PlaySound(_attackSoundSource, null);
            MoveToAttack();
            // 攻撃アニメを再生（構えアニメより優先）
            _characterAnimationSignal?.RequestAttack();
        }

        /// <summary>
        ///     現在の足元に対応する足音SE設定を取得します。
        /// </summary>
        private FootstepSoundConfig GetCurrentFootstepSoundConfig()
        {
            // 足元へ下向きの例を飛ばす。
            Vector3 origin = transform.position + _footstepRayOffset;

            if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, _footstepRayDistance))
            {
                return default;
            }

            if (_footstepSoundConfigs == null || _footstepSoundConfigs.Length == 0)
            {
                return default;
            }

            // Layer番号をLayerMask形式に変換する。
            // LayerMaskで複数対応している。
            int hitLayerMask = 1 << hit.collider.gameObject.layer;

            for (int i = 0; i < _footstepSoundConfigs.Length; i++)
            {
                // SurfaceLayerの中に、今当たった床Layerが含まれているかチェック。
                if ((_footstepSoundConfigs[i].SurfaceLayer.value & hitLayerMask) != 0)
                {
                    return _footstepSoundConfigs[i];
                }
            }

            return default;
        }

        /// <summary>
        ///     足音SEを一定間隔で再生します。
        /// </summary>
        private void PlayFootstepSound()
        {
            if (Time.time - _lastFootstepTime < _footstepInterval)
            {
                return;
            }

            FootstepSoundConfig config = GetCurrentFootstepSoundConfig();

            // 床専用Sourceがある場合、それを再生。
            // ない場合はdefaultで。
            SoundEffectSource source = config.Source != null
                ? config.Source
                : _defaultFootstepSoundSource;

            string cueName = string.IsNullOrWhiteSpace(config.CueName)
                ? _defaultFootstepCueName
                : config.CueName;

            PlaySound(source, cueName);
            _lastFootstepTime = Time.time;
        }
        /// <summary>
        ///     SE Sourceを再生します。
        /// </summary>
        private void PlaySound(SoundEffectSource source, string cueName)
        {
            if (source == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(cueName))
            {
                source.Play();
                return;
            }

            source.Play(cueName);
        }

        /// <summary>
        ///     攻撃の1拍前に呼び出される処理。
        /// </summary>
        private void On1BeatBefore()
        {
            StopMoving();
            StopRotating();
            _characterAnimationViewModel?.SetVelocity(Vector2.zero);
        }
        /// <summary>
        ///     攻撃の2拍前に呼び出される処理。
        /// </summary>
        private void On2BeatBefore()
        {

        }

        private void PlayOneShot(string key)
        {
            if (_characterAnimationSignal == null)
            {
                return;
            }

            _characterAnimationSignal?.TryRequestOneShot(key, out _);
        }

#if UNITY_EDITOR
        /// <summary>
        ///     足音判定用RayをSceneビューへ描画します。
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Vector3 origin = transform.position + _footstepRayOffset;

            if (Physics.Raycast(
                    origin,
                    Vector3.down,
                    out RaycastHit hit,
                    _footstepRayDistance))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(origin, hit.point);
                Gizmos.DrawWireSphere(hit.point, 0.1f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(
                    origin,
                    origin + (Vector3.down * _footstepRayDistance));
            }
        }
#endif
    }
}
