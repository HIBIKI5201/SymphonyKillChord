using KillChord.Runtime.Adaptor.InGame.Animation;
using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.Player;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.Utility.Collections;
using KillChord.Runtime.View.InGame.Character;
using KillChord.Runtime.View.InGame.Sequence;
using KillChord.Runtime.View.Persistent.Input;
using KillChord.Runtime.View.Persistent.Music;
using KillChord.Runtime.View.Persistent.Voice;
using LitMotion;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///     プレイヤー入力を受け取り、移動と攻撃を更新するViewクラス。
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConst.MOVEMENT)]
    public sealed class PlayerView : MonoBehaviour, IDamageable, IGameplayControllable
    {
        [SerializeField] private string _blendName;
        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody _rb;

        [SerializeField, Tooltip("攻撃時の武器表示と攻撃SEを管理するView。")]
        private PlayerAttackWeaponView _attackWeaponView;

        [SerializeField, Tooltip("回避成功時の仮エフェクト")]
        private ParticleSystem _dodgeEffect;

        [SerializeField, Tooltip("回避中にMaterialエフェクトを適用するRenderer一覧。")]
        private Renderer[] _dodgeEffectRenderers;

        [SerializeField, Tooltip("回避中に到達させるSmearsPowerの最大値。")]
        private float _dodgeSmearsPower = 1f;
        [Space]

        [SerializeField, Tooltip("被弾SE用Source。")]
        private SoundEffectSource _damageSoundSource;
        [Space]

        [SerializeField, Tooltip("仮Voice用Source。")]
        private VoiceSource _voiceSource;
        [SerializeField, Tooltip("被弾時VoiceのCueName。空の場合はSource側のCueを再生します。")]
        private string _damageVoiceCueName;
        [Space]

        [SerializeField, Tooltip("回避SE用Source。")]
        private SoundEffectSource _dodgeSoundSource;

        [SerializeField, Tooltip("足音演出Viewです。")]
        private FootStepView _footStepView;

        [SerializeField, Tooltip("足音SEの共通CueName。床設定側にCueNameがない場合に使用します。")]
        private string _defaultFootstepCueName;

        [SerializeField, Tooltip("床素材ごとの足音SE設定。")]
        private PlayerFootstepSoundConfig[] _footstepSoundConfigs;

        [SerializeField, Tooltip("足元判定の開始位置オフセット。")]
        private Vector3 _footstepRayOffset;

        [SerializeField, Min(0.01f), Tooltip("足元判定の距離。")]
        private float _footstepRayDistance = 1.5f;

        [SerializeField, Min(0.01f), Tooltip("音楽同期が使えない場合の足音SE再生間隔です。")]
        private float _footstepInterval = 0.35f;

        private const float MIN_FOOTSTEP_VELOCITY_SQR = 0.01f;
        private const string SMEARS_ON_KEYWORD = "SMEARS_ON";
        private static readonly int SmearsOnPropertyId = Shader.PropertyToID("_SmearsOn");
        private static readonly int SmearsPowerPropertyId = Shader.PropertyToID("_SmearsPower");
        private static readonly int SmearsDirectionPropertyId = Shader.PropertyToID("_SmearsDirection");
        private bool _isInitialized;
        private bool _isPlaying;
        private bool _isDodge;
        private string _pendingSkillAnimationKey;
        private Vector2 _moveVector;
        private Vector2 _dogeVector;
        private Transform _cacheTransform;
        private Transform _cameraTransform;
        private IPlayerController _controller;
        private ICharacterAnimationViewModel _characterAnimationViewModel;
        private ICharacterAnimationSignal _characterAnimationSignal;
        private PlayerInputView _playerInputView;
        private PlayerHealthHudPresenter _healthHudPresenter;
        private CancellationTokenSource _cancellationTokenSource;
        private Quaternion _rotation;
        private float _lastFootstepTime;
        private int _lastFootstepEighthIndex = int.MinValue;
        private MusicSyncState _musicSyncState;
        private MotionHandle _dodgeMaterialEffectHandle;
        private MaterialPropertyBlock _dodgeMaterialPropertyBlock;

        /// <summary> プレイヤー攻撃コントローラー。 </summary>
        public PlayerAttackController PlayerAttackController { get; private set; }

        /// <summary> 毎フレーム移動更新を行う。 </summary>
        private void Update()
        {
            if (!_isInitialized || !_isPlaying || _controller == null)
            {
                return;
            }
            PlayerAttackController?.UpdateAttackCooldown(Time.deltaTime);
            UpdateMovement();
        }

        private void OnDestroy()
        {
            if (_playerInputView != null)
            {
                UnRegisterActions();
            }

            if (_healthHudPresenter != null)
            {
                _healthHudPresenter.OnDamaged -= PlayDamageFeedbackSound;
                _healthHudPresenter?.Dispose();
            }

            _dodgeMaterialEffectHandle.TryCancel();
        }

        /// <summary> 依存コンポーネントを初期化する。 </summary>
        public void Initialize(
            IPlayerController playerMovementController,
            PlayerAttackController playerAttackController,
            ICharacterAnimationViewContext animationContext,
            MusicSyncState musicSyncState,
            Transform cameraTransform,
            PlayerInputView playerInputView,
            PlayerHealthHudPresenter healthHudPresenter)
        {
            _controller = playerMovementController;
            PlayerAttackController = playerAttackController;
            _characterAnimationViewModel = animationContext.ViewModel;
            _characterAnimationSignal = animationContext.Signal;
            _musicSyncState = musicSyncState;
            _cameraTransform = cameraTransform;
            _playerInputView = playerInputView;
            _cacheTransform = transform;
            _healthHudPresenter = healthHudPresenter;
            _healthHudPresenter.OnDamaged += PlayDamageFeedbackSound;

            Debug.Assert(_rb != null, $"{nameof(_rb)} is null", this);
            Debug.Assert(_animator != null, $"{nameof(_animator)} is null", this);
            Debug.Assert(_cameraTransform != null, $"{nameof(_cameraTransform)} is null", this);

            _isInitialized = true;
        }

        /// <summary> ゲームプレイを開始し、入力イベントを購読する。 </summary>
        public void StartGameplay()
        {
            if (!_isInitialized || _playerInputView == null || _isPlaying)
            {
                return;
            }

            RegisterActions();
            SyncFootstepTiming();
            _isPlaying = true;
        }

        /// <summary> ゲームプレイを停止し、入力イベントの購読を解除する。 </summary>
        public void StopGameplay()
        {
            if (!_isPlaying)
            {
                return;
            }

            UnRegisterActions();

            _moveVector = Vector2.zero;
            _isDodge = false;
            _isPlaying = false;

            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }

            _characterAnimationViewModel?.SetVelocity(Vector2.zero);
            _attackWeaponView?.HideAllWeapons();
            SyncFootstepTiming();
        }

        /// <summary>
        ///     被弾時のSEと仮Voiceを再生します。
        /// </summary>
        public void PlayDamageFeedbackSound()
        {
            PlaySound(_damageSoundSource, null);
            PlayVoice(_voiceSource, _damageVoiceCueName);
        }

        public void PlaySkillAnimation(string animationKey)
        {
            if (string.IsNullOrWhiteSpace(animationKey))
            {
                return;
            }

            _pendingSkillAnimationKey = animationKey;
        }

        /// <summary>
        ///    回避成功時の仮エフェクトを再生します。
        /// </summary>
        public void PlayDodgeSuccessFeedback()
        {
            if (_dodgeEffect == null)
            {
                return;
            }

            _dodgeEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _dodgeEffect.Play();
        }

        /// <summary>
        ///     回避中のMaterialエフェクト(Smears)を再生します。
        /// </summary>
        /// <param name="duration"> 回避の継続時間です。 </param>
        /// <param name="direction"> 回避方向(ワールド空間)です。 </param>
        public void PlayDodgeMaterialEffect(float duration, Vector3 direction)
        {
            direction.Normalize();

            if (_dodgeEffectRenderers == null || _dodgeEffectRenderers.Length == 0)
            {
                return;
            }

            _dodgeMaterialPropertyBlock ??= new MaterialPropertyBlock();

            foreach (Renderer renderer in _dodgeEffectRenderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                // MaterialPropertyBlockはKeywordを切り替えられないため、shader_feature_localの有効化はMaterial側で行う。
                renderer.material.EnableKeyword(SMEARS_ON_KEYWORD);

                renderer.GetPropertyBlock(_dodgeMaterialPropertyBlock);
                _dodgeMaterialPropertyBlock.SetFloat(SmearsOnPropertyId, 0f);
                _dodgeMaterialPropertyBlock.SetVector(SmearsDirectionPropertyId, -direction);
                renderer.SetPropertyBlock(_dodgeMaterialPropertyBlock);
            }

            _dodgeMaterialEffectHandle.TryCancel();

            _dodgeMaterialEffectHandle = LMotion.Create(_dodgeSmearsPower, 0f, duration)
                .WithEase(Ease.InQuad)
                .Bind(this, static (value, state) => state.ApplySmearsPower(value));
        }

        /// <summary>
        ///     キャッシュ済みのRenderer配列とMaterialPropertyBlockを使い回して_SmearsPowerを反映します。
        /// </summary>
        private void ApplySmearsPower(float value)
        {
            foreach (Renderer renderer in _dodgeEffectRenderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(_dodgeMaterialPropertyBlock);
                _dodgeMaterialPropertyBlock.SetFloat(SmearsPowerPropertyId, value);
                renderer.SetPropertyBlock(_dodgeMaterialPropertyBlock);
            }
        }

        /// <summary>
        ///     回避終了時にMaterialエフェクト(Smears)を既定値へ戻します。
        /// </summary>
        public void ResetDodgeMaterialEffect()
        {
            if (_dodgeEffectRenderers == null || _dodgeEffectRenderers.Length == 0)
            {
                return;
            }

            _dodgeMaterialEffectHandle.TryCancel();

            foreach (Renderer renderer in _dodgeEffectRenderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                renderer.material.DisableKeyword(SMEARS_ON_KEYWORD);
                renderer.SetPropertyBlock(null);
            }
        }

        /// <summary> 入力イベントを購読する。 </summary>
        private void RegisterActions()
        {
            _playerInputView.OnMoveInput += OnMove;
            _playerInputView.OnAttackInput += OnAttack;
            _playerInputView.OnDodgeInput += OnDodge;
        }

        /// <summary> 入力イベントの購読を解除する。 </summary>
        private void UnRegisterActions()
        {
            _playerInputView.OnMoveInput -= OnMove;
            _playerInputView.OnAttackInput -= OnAttack;
            _playerInputView.OnDodgeInput -= OnDodge;
        }

        /// <summary> 移動入力を保持する。 </summary>
        private void OnMove(InputContext<Vector2> input)
        {
            _moveVector = input.Value;
        }

        /// <summary> 回避入力を受け取ったら回避要求フラグを立てる。 </summary>
        private void OnDodge(InputContext<float> input)
        {
            if (input.Phase == InputActionPhase.Started)
            {
                if (_controller.IsDodging)
                {
                    return;
                }
                _dogeVector = _moveVector;
                _isDodge = true;
            }
        }

        /// <summary>
        ///     攻撃入力を受け取り、攻撃結果に応じたSEを再生する。
        /// </summary>
        private void OnAttack(InputContext<float> input)
        {
            if (input.Phase != InputActionPhase.Started)
            {
                return;
            }

            if (_controller.IsDodging)
            {
                return;
            }

            if (PlayerAttackController.IsAttacking || PlayerAttackController.IsAttackCooldown)
            {
                return;
            }

            if (PlayerAttackController == null)
            {
                Debug.LogError("[PlayerView] AttackController is null", this);
                return;
            }

            if (PlayerAttackController.ExecuteAttack(out int resultBeatType))
            {
                string animationKey = _pendingSkillAnimationKey;
                _pendingSkillAnimationKey = null;

                float attackAnimationLength = 0f;
                if (_characterAnimationSignal != null)
                {
                    attackAnimationLength = string.IsNullOrWhiteSpace(animationKey)
                        ? _characterAnimationSignal.RequestAttack(resultBeatType)
                        : _characterAnimationSignal.RequestAttack(animationKey);
                }

                _attackWeaponView?.Play(resultBeatType, attackAnimationLength);

                if (PlayerAttackController.HasCurrentLockOnTarget)
                {
                    CancelAttackRotate();
                    _cancellationTokenSource = new CancellationTokenSource();
                    RotateToTargetAsync(
                        PlayerAttackController.CurrentLockOnTargetPosition,
                        PlayerAttackController.AttackRotationSpeed,
                        _cancellationTokenSource.Token);
                }
            }
        }

        /// <summary> 入力に基づいて移動と向きを更新する。 </summary>
        private void UpdateMovement()
        {
            if (_controller == null)
            {
                return;
            }

            Vector2 dir = _moveVector;

            if (PlayerAttackController.IsAttacking)
            {
                // 攻撃時、入力をキャンセルする。
                dir = Vector2.zero;
            }

            //_animator.SetFloat(_blendName, Mathf.Min(1f, dir.magnitude));
            dir = Rotate(dir, -_cameraTransform.eulerAngles.y);

            if (_isDodge)
            {
                Vector2 dodgeDir = _dogeVector;
                // 移動入力がない場合は、前方を回避方向とする
                if (dodgeDir.sqrMagnitude <= float.Epsilon)
                {
                    var fwd = _cacheTransform.forward;
                    dodgeDir.x = fwd.x;
                    dodgeDir.y = fwd.z;
                }
                dodgeDir = Rotate(dodgeDir, -_cameraTransform.eulerAngles.y);
                bool dodgeSucceeded = _controller.TryDodge(dodgeDir, Time.time);

                if (dodgeSucceeded)
                {
                    PlaySound(_dodgeSoundSource, null);
                    _characterAnimationSignal?.RequestDodge();
                }

                _isDodge = false;
            }

            Quaternion rotation = _cacheTransform.rotation;
            if (_cancellationTokenSource != null)
            {
                rotation = _rotation;
            }
            _controller.Update(ref rotation, dir, Time.time, out Vector3 velocity);
            _rb.linearVelocity = velocity;
            _cacheTransform.rotation = rotation;
            _characterAnimationViewModel?.SetVelocity(new Vector2(velocity.x, velocity.z));
            PlayFootstepSound(velocity);
        }

        /// <summary>
        ///     攻撃時にターゲット方向へ滑らかに回転する Task 実装。
        ///     回転は Exp ベースの収束係数で行い、攻撃終了またはターゲット無効で停止する。
        /// </summary>
        private async Task RotateToTargetAsync(Vector3 targetPosition, float speed, CancellationToken ct)
        {
            _rotation = _cacheTransform.rotation;

            try
            {
                while (!ct.IsCancellationRequested
                    && PlayerAttackController != null
                    && PlayerAttackController.IsAttacking
                    && PlayerAttackController.HasCurrentLockOnTarget)
                {
                    Vector3 dirToTarget = targetPosition - _cacheTransform.position;
                    dirToTarget.y = 0f;
                    if (dirToTarget.sqrMagnitude <= float.Epsilon) break;

                    Quaternion targetRot = Quaternion.LookRotation(dirToTarget.normalized, Vector3.up);
                    float t = 1f - Mathf.Exp(-Mathf.Max(0f, speed) * Time.deltaTime);
                    _rotation = Quaternion.Slerp(_rotation, targetRot, t);

                    if (Quaternion.Angle(_rotation, targetRot) < 0.5f)
                    {
                        _rotation = targetRot;
                        break;
                    }

                    await Task.Yield();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                if (_cancellationTokenSource != null && _cancellationTokenSource.Token == ct)
                {
                    try { _cancellationTokenSource.Dispose(); } catch { }
                    _cancellationTokenSource = null;
                }
            }
        }

        private void CancelAttackRotate()
        {
            if (_cancellationTokenSource != null)
            {
                try
                {
                    _cancellationTokenSource.Cancel();
                }
                catch { }
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
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
        ///     Voice Sourceを再生します。
        /// </summary>
        private void PlayVoice(VoiceSource source, string cueName)
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
        ///     現在の足元に対応する足音SE設定を取得します。
        /// </summary>
        /// <returns> 足音SE設定です。 </returns>
        private PlayerFootstepSoundConfig GetCurrentFootstepSoundConfig()
        {
            Vector3 origin = transform.position + _footstepRayOffset;
            if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, _footstepRayDistance))
            {
                return default;
            }

            if (_footstepSoundConfigs == null || _footstepSoundConfigs.Length == 0)
            {
                return default;
            }

            int hitLayerMask = 1 << hit.collider.gameObject.layer;
            for (int i = 0; i < _footstepSoundConfigs.Length; i++)
            {
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
            if (_controller == null || _controller.IsDodging)
            {
                SyncFootstepTiming();
                return;
            }

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

            PlayerFootstepSoundConfig config = GetCurrentFootstepSoundConfig();
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

        /// <summary> 2Dベクトルを指定角度だけ回転させる。 </summary>
        private static Vector2 Rotate(Vector2 v, float degrees)
            => Quaternion.Euler(0, 0, degrees) * v;

#if UNITY_EDITOR
        /// <summary>
        ///     足音判定用RayをSceneビューへ描画します。
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Vector3 origin = transform.position + _footstepRayOffset;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, _footstepRayDistance))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(origin, hit.point);
                Gizmos.DrawWireSphere(hit.point, 0.1f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(origin, origin + (Vector3.down * _footstepRayDistance));
            }
        }
#endif

        /// <summary>
        ///     足元の判定結果と足音SEの対応情報です。
        /// </summary>
        [System.Serializable]
        private struct PlayerFootstepSoundConfig
        {
            [Tooltip("足元判定で使用するレイヤー。")]
            public LayerMask SurfaceLayer;

            [Tooltip("この床で再生するCueName。空の場合は共通Cueを使用します。")]
            public string CueName;
        }
    }
}

