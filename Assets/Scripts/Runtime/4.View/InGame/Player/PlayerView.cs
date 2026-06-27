using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Player;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.Utility.Collections;
using KillChord.Runtime.View.InGame.Sequence;
using KillChord.Runtime.View.Persistent.Input;
using KillChord.Runtime.View.Persistent.Music;
using KillChord.Runtime.View.Persistent.Voice;
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

        private const string ATTACK_BEAT_1_KEY = "Attack_Beat1";
        private const string ATTACK_BEAT_2_KEY = "Attack_Beat2";
        private bool _isInitialized;
        private bool _isPlaying;
        private bool _isDodge;
        private string _pendingSkillAnimationKey;
        private Vector2 _moveVector;
        private Vector2 _dogeVector;
        private Transform _cacheTransform;
        private Transform _cameraTransform;
        private IPlayerController _controller;
        private ICharacterAnimationController _characterAnimationController;
        private PlayerInputView _playerInputView;
        private PlayerHealthHudPresenter _healthHudPresenter;
        private CancellationTokenSource _cancellationTokenSource;
        private CharacterAnimationIndices _characterAnimationIndices;
        private Quaternion _rotation;

        /// <summary> プレイヤー攻撃コントローラー。 </summary>
        public PlayerAttackController PlayerAttackController { get; private set; }

        /// <summary> 毎フレーム移動更新を行う。 </summary>
        private void Update()
        {
            if (!_isInitialized || !_isPlaying || _controller == null)
            {
                return;
            }

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
        }

        /// <summary> 依存コンポーネントを初期化する。 </summary>
        public void Initialize(
            IPlayerController playerMovementController,
            PlayerAttackController playerAttackController,
            ICharacterAnimationController characterAnimationController,
             CharacterAnimationIndices animationIndices,
            Transform cameraTransform,
            PlayerInputView playerInputView,
            PlayerHealthHudPresenter healthHudPresenter)
        {
            _controller = playerMovementController;
            PlayerAttackController = playerAttackController;
            _characterAnimationController = characterAnimationController;
            _characterAnimationIndices = animationIndices;
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

            _characterAnimationController?.SetVelocity(Vector2.zero);
            _attackWeaponView?.HideAllWeapons();
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

                PlaySound(_dodgeSoundSource, null);
                _characterAnimationController?.TriggerOneShot(_characterAnimationIndices.Dodge);
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

            if (PlayerAttackController.IsAttacking)
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

                if (string.IsNullOrWhiteSpace(animationKey))
                {
                    animationKey = GetAttackAnimationKey(resultBeatType);
                }

                int attackIndex = _characterAnimationIndices.Attack;

                if (!string.IsNullOrEmpty(animationKey)
                    && _characterAnimationIndices.TryGetOneShotIndex(animationKey, out int oneShotIndex))
                {
                    attackIndex = oneShotIndex;
                }

                float attackAnimationLength =
                    _characterAnimationController?.GetOneShotAnimationLength(attackIndex) ?? 0f;

                _attackWeaponView?.Play(resultBeatType, attackAnimationLength);
                _characterAnimationController?.TriggerOneShot(attackIndex);

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
                _controller.TryDodge(dodgeDir, Time.time);
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
            _characterAnimationController?.SetVelocity(new Vector2(velocity.x, velocity.z));
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

        //TODO:今はint直置きだから後で調整しやすいようにしとく。
        private string GetAttackAnimationKey(int beatType)
        {
            return beatType switch
            {
                1 or 2 or 3 => ATTACK_BEAT_1_KEY,
                4 or 6 or 8 => ATTACK_BEAT_2_KEY,
                _ => string.Empty
            };
        }

        /// <summary> 2Dベクトルを指定角度だけ回転させる。 </summary>
        private static Vector2 Rotate(Vector2 v, float degrees)
            => Quaternion.Euler(0, 0, degrees) * v;
    }
}

