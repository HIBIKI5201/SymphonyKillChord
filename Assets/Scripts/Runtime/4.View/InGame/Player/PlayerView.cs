using KillChord.Runtime.Adaptor.InGame.Animation;
using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.Player;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.Utility.Collections;
using KillChord.Runtime.Utility.Persistent;
using KillChord.Runtime.View.InGame.Character;
using KillChord.Runtime.View.InGame.Sequence;
using KillChord.Runtime.View.Persistent.Input;
using KillChord.Runtime.View.Persistent.Music;
using KillChord.Runtime.View.Persistent.Voice;
using LitMotion;
using LitMotion.Extensions;
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

        [SerializeField, Tooltip("被弾時のエフェクトを再生する位置です。")]
        private Transform _damageEffectPoint;

        [SerializeField, Tooltip("回避成功時の仮エフェクト")]
        private ParticleSystem _dodgeEffect;

        [SerializeField, Tooltip("回避中にMaterialエフェクトを適用するRenderer一覧。")]
        private Renderer[] _dodgeEffectRenderers;

        [SerializeField, Tooltip("回避中に到達させるSmearsPowerの最大値。")]
        private float _dodgeSmearsPower = 1f;

        [SerializeField, Tooltip("被弾時のポストエフェクトのfrom値。")]
        private float _damageEffectFrom = 50f;

        [SerializeField, Tooltip("被弾時のポストエフェクト再生間隔。")]
        private float _damageEffectInterval = 0.1f;
        [SerializeField, Tooltip("被弾時のポストエフェクトMaterial。")]
        private Material _damageEffectMaterial;

        [Space]

        [Header("Voice")]
        [SerializeField, Tooltip("Voice用Source。")]
        private VoiceSource _voiceSource;

        [SerializeField, Tooltip("被弾時VoiceのCueName。空の場合はSource側のCueを再生します。")]
        private string _damageVoiceCueName;

        [SerializeField, Tooltip("ステージ開始時VoiceのCueName。空の場合は再生しない。")]
        private string _stageStartVoiceCueName;

        [SerializeField, Tooltip("ステージクリア時VoiceのCueName。空の場合は再生しない。")]
        private string _stageClearVoiceCueName;

        [SerializeField, Tooltip("ゲームオーバー時VoiceのCueName。空の場合は再生しない。")]
        private string _gameOverVoiceCueName;

        [SerializeField, Tooltip("スキル発動時VoiceのCueName。空の場合は再生しない。")]
        private string _skillVoiceCueName;
        [Space]

        [Header("SE")]
        [SerializeField, Tooltip("被弾SE用Source。")]
        private SoundEffectSource _damageSoundSource;

        [SerializeField, Tooltip("回避SE用Source。")]
        private SoundEffectSource _dodgeSoundSource;

        [SerializeField, Tooltip("Critical SE用Source。")]
        private SoundEffectSource _criticalSoundSource;

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
        private const float ATTACK_CANCEL_INPUT_THRESHOLD_SQR = 0.0225f;
        private const string SMEARS_ON_KEYWORD = "SMEARS_ON";
        private static readonly int SMEARS_ON_PROPERTY_ID = Shader.PropertyToID("_SmearsOn");
        private static readonly int SMEARS_POWER_PROPERTY_ID = Shader.PropertyToID("_SmearsPower");
        private static readonly int SMEARS_DIRECTION_PROPERTY_ID = Shader.PropertyToID("_SmearsDirection");
        private static readonly int DAMAGED_EFFECT_PROPERTY_ID = Shader.PropertyToID("_Pixel");
        private bool _isInitialized;
        private bool _isPlaying;
        private bool _isDodge;
        private string _pendingSkillAnimationKey;
        private Vector2 _moveVector;
        private Vector2 _dodgeVector;
        private Vector3 _cacheVelocity;
        private Quaternion _cacheRotation;
        private Transform _cacheTransform;
        private Transform _cameraTransform;
        private IPlayerController _controller;
        private ICharacterAnimationViewModel _characterAnimationViewModel;
        private IPlayerCharacterAnimationSignal _characterAnimationSignal;
        private PlayerInputView _playerInputView;
        private PlayerHealthHudPresenter _healthHudPresenter;
        private float _lastFootstepTime;
        private int _lastFootstepEighthIndex = int.MinValue;
        private MusicSyncState _musicSyncState;
        private MotionHandle _dodgeMaterialEffectHandle;
        private MotionHandle _damageEffectHandle;
        private MaterialPropertyBlock _dodgeMaterialPropertyBlock;
        private ReusableParticleSystemView _damageEffectView;

        private float _attackFacingRemaining = 0f;
        private Quaternion _attackFacingRotation;

        /// <summary> プレイヤー攻撃コントローラー。 </summary>
        public PlayerAttackController PlayerAttackController { get; private set; }

        private PlayerInputSuppressionState _inputSuppressionState;

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
        private void FixedUpdate()
        {
            if (!_isInitialized || !_isPlaying || _controller == null)
            {
                return;
            }
            UpdateRigidbody();
        }

        private void OnDestroy()
        {
            EventBus<EOnTakeDamage>.Unregister(HandleTakeDamage);

            if (_playerInputView != null)
            {
                UnRegisterActions();
            }

            if (_healthHudPresenter != null)
            {
                _healthHudPresenter.OnDamaged -= PlayDamageFeedback;
                _healthHudPresenter?.Dispose();
            }

            _dodgeMaterialEffectHandle.TryCancel();
            _damageEffectHandle.TryCancel();
        }

        /// <summary> 依存コンポーネントを初期化する。 </summary>
        public void Initialize(
            IPlayerController playerMovementController,
            PlayerAttackController playerAttackController,
            ICharacterAnimationViewContext animationContext,
            MusicSyncState musicSyncState,
            Transform cameraTransform,
            PlayerInputView playerInputView,
            PlayerHealthHudPresenter healthHudPresenter,
            ReusableParticleSystemView damageEffectView,
            PlayerInputSuppressionState inputSuppressionState = null)
        {
            _controller = playerMovementController;
            PlayerAttackController = playerAttackController;
            _inputSuppressionState = inputSuppressionState;
            _damageEffectView = damageEffectView;
            _characterAnimationViewModel = animationContext.ViewModel;
            _characterAnimationSignal = (IPlayerCharacterAnimationSignal)animationContext.Signal;
            _musicSyncState = musicSyncState;
            _cameraTransform = cameraTransform;
            _playerInputView = playerInputView;
            _cacheTransform = transform;
            _healthHudPresenter = healthHudPresenter;
            _healthHudPresenter.OnDamaged += PlayDamageFeedback;
            EventBus<EOnTakeDamage>.Register(HandleTakeDamage);

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
            _cacheRotation = _rb != null ? _rb.rotation : _cacheTransform.rotation;
            _cacheVelocity = Vector3.zero;
        }

        /// <summary> ゲームプレイを停止し、入力イベントの購読を解除する。 </summary>
        public void StopGameplay()
        {
            if (!_isPlaying)
            {
                return;
            }

            UnRegisterActions();

            // 多段ヒットの残りを持ち越さない。プレイ再開時に前回の攻撃が飛ぶのを防ぐ。
            PlayerAttackController?.ClearPendingHits();

            _moveVector = Vector2.zero;
            _dodgeVector = Vector2.zero;
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
        ///     プレイヤーを指定したスタート地点へ戻し、移動・物理・進行中の演出状態をリセットします。
        /// </summary>
        /// <param name="position"> 戻す位置です。 </param>
        /// <param name="rotation"> 戻す回転です。 </param>
        public void ResetToSpawn(Vector3 position, Quaternion rotation)
        {
            // 回避関連の状態と演出をリセットする。
            _dodgeMaterialEffectHandle.TryCancel();
            ResetDodgeMaterialEffect();

            // 被弾ポストエフェクトの再生途中の値を持ち越さない。
            _damageEffectHandle.TryCancel();

            // 位置と回転をスタート地点へ戻す。
            if (_cacheTransform != null)
            {
                _cacheTransform.SetPositionAndRotation(position, rotation);
            }
            else
            {
                transform.SetPositionAndRotation(position, rotation);
            }

            // Rigidbodyの位置・回転・速度を同期してリセットする。
            if (_rb != null)
            {
                _rb.position = position;
                _rb.rotation = rotation;
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }

            _cacheRotation = rotation;
            _cacheVelocity = Vector3.zero;
            _attackFacingRemaining = 0f;
            _attackFacingRotation = rotation;

            // 入力由来の移動・回避要求をクリアする。
            _moveVector = Vector2.zero;
            _dodgeVector = Vector2.zero;
            _isDodge = false;

            _characterAnimationViewModel?.SetVelocity(Vector2.zero);
            SyncFootstepTiming();
        }

        /// <summary>
        ///     被弾時のSEと仮Voiceを再生します。
        /// </summary>
        public void PlayDamageFeedback()
        {
            PlaySound(_damageSoundSource, null);
            PlayVoice(_voiceSource, _damageVoiceCueName);

            // パーティクル演出はViewが設定されている場合のみ再生する。
            if (_damageEffectView != null)
            {
                Vector3 effectPosition = _damageEffectPoint != null
                    ? _damageEffectPoint.position
                    : transform.position;

                _damageEffectView.PlayAt(effectPosition);
            }

            // ポストエフェクトはパーティクル演出の有無に関わらず再生する。
            _damageEffectHandle.TryCancel();
            _damageEffectHandle = LMotion.Create(_damageEffectFrom, 0f, _damageEffectInterval)
                .BindToMaterialFloat(_damageEffectMaterial, DAMAGED_EFFECT_PROPERTY_ID);
        }

        /// <summary>
        ///     ステージ開始時のPlayer Voiceを再生します。
        /// </summary>
        public void PlayStageStartVoice()
        {
            PlayPriorityVoice(_stageStartVoiceCueName);
        }

        /// <summary>
        ///     ステージクリア時のPlayer Voiceを再生します。
        /// </summary>
        public void PlayStageClearVoice()
        {
            PlayPriorityVoice(_stageClearVoiceCueName);
        }

        /// <summary>
        ///     ゲームオーバー時のPlayer Voiceを再生します。
        /// </summary>
        public void PlayGameOverVoice()
        {
            PlayPriorityVoice(_gameOverVoiceCueName);
        }

        /// <summary>
        ///     スキル発動時のPlayer Voiceを再生します。
        /// </summary>
        public void PlaySkillVoice()
        {
            PlayPriorityVoice(_skillVoiceCueName);
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
        public void PlayDodgeMaterialEffect(float duration, in Vector3 direction)
        {
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
                _dodgeMaterialPropertyBlock.SetFloat(SMEARS_ON_PROPERTY_ID, 0f);
                _dodgeMaterialPropertyBlock.SetFloat(SMEARS_POWER_PROPERTY_ID, _dodgeSmearsPower);
                _dodgeMaterialPropertyBlock.SetVector(SMEARS_DIRECTION_PROPERTY_ID, -direction.normalized);
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
                _dodgeMaterialPropertyBlock.SetFloat(SMEARS_POWER_PROPERTY_ID, value);
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
            _playerInputView.OnMobileDodgeFlickInput += OnMobileDodgeFlick;
        }

        /// <summary> 入力イベントの購読を解除する。 </summary>
        private void UnRegisterActions()
        {
            _playerInputView.OnMoveInput -= OnMove;
            _playerInputView.OnAttackInput -= OnAttack;
            _playerInputView.OnDodgeInput -= OnDodge;
            _playerInputView.OnMobileDodgeFlickInput -= OnMobileDodgeFlick;
        }

        /// <summary> 移動入力を保持する。 </summary>
        private void OnMove(InputContext<Vector2> input)
        {
            _moveVector = input.Value;
        }

        /// <summary> 回避入力を受け取ったら回避要求フラグを立てる。 </summary>
        private void OnDodge(InputContext<float> input)
        {
            RequestDodge(input.Phase, _moveVector);
        }

        /// <summary> モバイル仮想スティックの方向付き回避入力を受け取る。 </summary>
        private void OnMobileDodgeFlick(InputContext<Vector2> input)
        {
            RequestDodge(input.Phase, input.Value);
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

            if (_inputSuppressionState != null && _inputSuppressionState.IsSuppressed)
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

                _attackWeaponView?.Play(resultBeatType);

                if (PlayerAttackController.HasCurrentLockOnTarget)
                {
                    StartAttackRotate();
                }
            }
        }

        private void UpdateRigidbody()
        {
            _rb.linearVelocity = _cacheVelocity;
            _rb.MoveRotation(_cacheRotation);
        }

        /// <summary> 入力に基づいて移動と向きを更新する。 </summary>
        private void UpdateMovement()
        {
            if (_controller == null)
            {
                return;
            }

            Vector2 dir = _moveVector;

            if (PlayerAttackController.IsAttacking
                || (_inputSuppressionState != null && _inputSuppressionState.IsSuppressed))
            {
                // 攻撃中・入力抑制中は移動入力をキャンセルする。
                dir = Vector2.zero;
            }
            else if (dir.sqrMagnitude > ATTACK_CANCEL_INPUT_THRESHOLD_SQR
                && !_isDodge
                && !_controller.IsDodging)
            {
                _characterAnimationSignal?.CancelOneShot();
            }

            //_animator.SetFloat(_blendName, Mathf.Min(1f, dir.magnitude));
            dir = Rotate(dir, -_cameraTransform.eulerAngles.y);

            if (_isDodge)
            {
                Vector2 dodgeDir = _dodgeVector;
                // 移動入力がない場合は、前方を回避方向とする
                if (dodgeDir.sqrMagnitude <= float.Epsilon)
                {
                    Vector3 fwd = _cacheRotation * Vector3.forward;
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


            Quaternion rotation = _cacheRotation;

            _controller.Update(ref rotation, dir, Time.time, out Vector3 velocity);
            _cacheVelocity = velocity;
            _cacheRotation = rotation;

            // 攻撃向きロック：0.1秒間は移動入力による回転を上書きして敵方向を維持する
            if (_attackFacingRemaining > 0f)
            {
                _attackFacingRemaining -= Time.deltaTime;
                _cacheRotation = _attackFacingRotation;
            }
            _characterAnimationViewModel?.SetVelocity(new Vector2(velocity.x, velocity.z));
            PlayFootstepSound(velocity);
        }

        /// <summary>
        ///     入力状態を確認し、次の移動更新へ回避を要求する。
        /// </summary>
        /// <param name="phase"> 入力フェーズ。 </param>
        /// <param name="direction"> 要求する回避方向。 </param>
        private void RequestDodge(InputActionPhase phase, in Vector2 direction)
        {
            if (phase != InputActionPhase.Started
                || _isDodge
                || (_inputSuppressionState != null && _inputSuppressionState.IsSuppressed)
                || _controller.IsDodging)
            {
                return;
            }

            _dodgeVector = direction;
            _isDodge = true;
        }

        /// <summary> 攻撃時にターゲット方向への要求回転を更新する。 </summary>
        private void StartAttackRotate()
        {
            Vector3 dir = PlayerAttackController.CurrentLockOnTargetPosition - _cacheTransform.position;
            dir.y = 0;

            if (dir.sqrMagnitude <= float.Epsilon)
            {
                _attackFacingRotation = _cacheRotation;
                _attackFacingRemaining = 0f;
                return;
            }

            _attackFacingRotation = Quaternion.LookRotation(dir, Vector3.up);
            _attackFacingRemaining = 0.1f;
            _cacheRotation = _attackFacingRotation;
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
        ///     優先度の高いVoiceを再生します。再生中のVoiceがある場合は停止してから再生します。
        /// </summary>
        /// <param name="cueName"> 再生するVoiceのCue名。 </param>
        private void PlayPriorityVoice(string cueName)
        {
            if (_voiceSource == null
                || string.IsNullOrWhiteSpace(cueName))
            {
                return;
            }

            // 再生中のVoiceがある場合は停止してから再生する。
            _voiceSource.Stop();
            _voiceSource.Play(cueName);
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
        private void PlayFootstepSound(in Vector3 velocity)
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
        ///     敵への攻撃で被弾した際に、Criticalヒット時のSEを再生します。
        /// </summary>
        /// <param name="e"> イベント情報です。 </param>
        private void HandleTakeDamage(EOnTakeDamage e)
        {
            if (e.Critical && e.Damage > 0)
            {
                PlaySound(_criticalSoundSource, null);
            }
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

