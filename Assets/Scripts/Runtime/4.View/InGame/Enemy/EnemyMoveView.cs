using KillChord.Runtime.Adaptor.InGame.Animation;
using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.View.InGame.Character;
using KillChord.Runtime.View.InGame.Player;
using KillChord.Runtime.View.InGame.Sequence;
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
        /// <param name="enemyAIController"> 敵AIコントローラーです。 </param>
        /// <param name="target"> 攻撃対象です。 </param>
        /// <param name="animationContext"> アニメーション文脈です。 </param>
        /// <param name="musicSyncState"> 音楽同期状態です。 </param>
        /// <param name="damageEffectView"> ダメージエフェクトViewです。 </param>
        public void Initialize(
            EnemyAIController enemyAIController,
            Transform target,
            ICharacterAnimationViewContext animationContext,
            MusicSyncState musicSyncState,
            ReusableParticleSystemView damageEffectView)
        {
            _enemyAIController = enemyAIController;
            _target = target;
            _characterAnimationViewModel = animationContext.ViewModel;
            _damageEffectView = damageEffectView;
            _characterAnimationSignal = animationContext.Signal;
            _musicSyncState = musicSyncState;
            _isPlaying = false;
            SyncFootstepTiming();
        }

        /// <summary>
        ///     ゲームプレイの開始処理を行います。
        /// </summary>
        public void StartGameplay()
        {
            SyncFootstepTiming();
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
            _characterAnimationViewModel?.SetReserving(false);
            SyncFootstepTiming();
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
                    SyncFootstepTiming();
                    return true;
                }

                PlayFootstepSound(velocity);
                await Awaitable.NextFrameAsync(ct);
            }

            _characterAnimationViewModel?.SetVelocity(Vector2.zero);
            SyncFootstepTiming();
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

                PlayFootstepSound(velocity);
            }
            else
            {
                _characterAnimationViewModel?.SetVelocity(Vector2.zero);
                SyncFootstepTiming();
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
            SyncFootstepTiming();
        }

        public void StopRotating()
        {
            if (_navMeshAgent == null || !_navMeshAgent.enabled) return;

            _navMeshAgent.updateRotation = false;
        }

        public void PlayDamageFeedback()
        {
            if (_damageEffectView != null)
            {
                Vector3 effectPos = _damageEffectTransform != null
                    ? _damageEffectTransform.position
                    : transform.position;

                _damageEffectView.PlayAt(effectPos);
            }
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
            _characterAnimationViewModel?.SetReserving(false);
            _weaponItemView?.HideWeapon();
        }

        [SerializeField, Tooltip("敵攻撃SE用Source。歩兵、砲兵などの違いは敵Prefabごとに設定します。")]
        private SoundEffectSource _attackSoundSource;

        [SerializeField, Tooltip("攻撃ヒット時に再生するエフェクトPrefab。")]
        private ParticleSystem _attackHitEffectPrefab;

        [SerializeField, Tooltip("攻撃予約時に再生するエフェクトPrefab。")]
        private ParticleSystem _attackReserveEffectPrefab;

        [SerializeField, Tooltip("足音演出Viewです。")]
        private FootStepView _footStepView;

        [SerializeField, Tooltip("足音SEの共通CueName。床設定側にCueNameがない場合に使用します。")]
        private string _defaultFootstepCueName;

        [SerializeField, Tooltip("床素材ごとの足音SE設定。")]
        private FootstepSoundConfig[] _footstepSoundConfigs;

        [SerializeField, Tooltip("足元判定の開始位置オフセット。")]
        private Vector3 _footstepRayOffset;

        [SerializeField, Min(0.01f), Tooltip("足元判定の距離。")]
        private float _footstepRayDistance;

        [SerializeField, Min(0.01f), Tooltip("音楽同期が使えない場合の足音SE再生間隔です。")]
        private float _footstepInterval;

        [SerializeField, Tooltip("攻撃構えのanimationString")]
        private string _attackAttackReservedAnimation = "Enemy_AttackReserved";

        [SerializeField, Tooltip("武器アイテムビューです。")]
        private WeaponItemView _weaponItemView;

        [Header("Effects")]
        [SerializeField,Tooltip("攻撃ヒット時に再生するエフェクトのTransformです。")]
        private Transform _damageEffectTransform;

        private const float MIN_FOOTSTEP_VELOCITY_SQR = 0.01f;
        private float _lastFootstepTime;
        private int _lastFootstepEighthIndex = int.MinValue;
        private NavMeshAgent _navMeshAgent;
        private Transform _target;
        private EnemyAIController _enemyAIController;
        private ICharacterAnimationViewModel _characterAnimationViewModel;
        private ICharacterAnimationSignal _characterAnimationSignal;
        private MusicSyncState _musicSyncState;
        private ReusableParticleSystemView _damageEffectView;
        private ParticleSystem _attackHitEffectInstance;
        private ParticleSystem _attackReserveEffectInstance;
        private bool _isPlaying;

        /// <summary>
        ///     初期化時に必要な参照を取得します。
        /// </summary>
        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
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

            _weaponItemView?.Play();
            StopMoving();      
            StopRotating();
            FaceToTarget();
            PlayAttackEffect(_attackReserveEffectInstance);
            _characterAnimationViewModel?.SetReserving(true);
        }
        /// <summary>
        ///     攻撃を実行するエフェクトを再生する。
        /// </summary>
        private void PlayEffectHit()
        {
            if (!_isPlaying) return;
            
            _weaponItemView?.Play();
            _characterAnimationViewModel?.SetReserving(false);
            PlayAttackEffect(_attackHitEffectInstance);
            PlaySound(_attackSoundSource, null);
            MoveToAttack();
            // 攻撃アニメを再生（構えアニメより優先）
            _characterAnimationSignal?.RequestAttack();
        }
        /// <summary>
        ///    ターゲットの方向を向く。
        /// </summary>
        private void FaceToTarget()
        {
            if (_target == null) return;

            Vector3 dir = _target.position - transform.position;
            dir.y = 0f;                              // 水平だけ見る（上下に傾かない）
            if (dir.sqrMagnitude < 0.0001f) return;  // ほぼ真上＝向き不定は無視

            transform.rotation = Quaternion.LookRotation(dir);
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
        ///     足音SEをテンポ同期で再生します。
        /// </summary>
        /// <param name="velocity"> 現在速度です。 </param>
        private void PlayFootstepSound(Vector3 velocity)
        {
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
            if (horizontalVelocity.sqrMagnitude <= MIN_FOOTSTEP_VELOCITY_SQR)
            {
                SyncFootstepTiming();
                return;
            }

            if (!TryConsumeFootstepTiming())
            {
                return;
            }

            FootstepSoundConfig config = GetCurrentFootstepSoundConfig();

            string cueName = string.IsNullOrWhiteSpace(config.CueName)
                ? _defaultFootstepCueName
                : config.CueName;

            _footStepView?.Play(cueName);
        }

        /// <summary>
        ///     足音再生タイミングを消費可能か判定します。
        /// </summary>
        /// <returns> 再生可能な場合はtrue。 </returns>
        private bool TryConsumeFootstepTiming()
        {
            if (_musicSyncState != null && _musicSyncState.BeatLength > 0d)
            {
                int currentEighthIndex = GetCurrentFootstepEighthIndex();
                if (_lastFootstepEighthIndex == currentEighthIndex)
                {
                    return false;
                }

                _lastFootstepEighthIndex = currentEighthIndex;
                return true;
            }

            if (Time.time - _lastFootstepTime < _footstepInterval)
            {
                return false;
            }

            _lastFootstepTime = Time.time;
            return true;
        }

        /// <summary>
        ///     現在時刻の足音タイミングへ同期します。
        /// </summary>
        private void SyncFootstepTiming()
        {
            if (_musicSyncState != null && _musicSyncState.BeatLength > 0d)
            {
                _lastFootstepEighthIndex = GetCurrentFootstepEighthIndex();
                return;
            }

            _lastFootstepTime = Time.time;
        }

        /// <summary>
        ///     現在の八分音符インデックスを取得します。
        /// </summary>
        /// <returns> 八分音符インデックスです。 </returns>
        private int GetCurrentFootstepEighthIndex()
        {
            return Mathf.FloorToInt((float)(_musicSyncState.AccurateBeat * 2d));
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

        /// <summary>
        ///     指定キーのワンショットアニメーションを要求します。
        /// </summary>
        /// <param name="key"> 再生するアニメーションキー。 </param>
        private void PlayOneShot(string key)
        {
            if (_characterAnimationSignal == null)
            {
                return;
            }
            Debug.Log($"[{nameof(EnemyMoveView)}] OneShot要求 key={key} t={Time.time:F2}", this);
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
