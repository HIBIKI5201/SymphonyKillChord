using Cysharp.Threading.Tasks;
using CriWare;
using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Player;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.Utility.Collections;
using KillChord.Runtime.View.Persistent.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///     プレイヤー入力を受け取り、移動と攻撃を更新するViewクラス。
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConst.MOVEMENT)]
    public sealed class PlayerView : MonoBehaviour, IDamageable
    {
        [SerializeField] private string _blendName;
        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private CriAtomSource _seSource;

        [SerializeField]
        [Header("攻撃インターバルテスト用の値（単位:秒）")]
        private float _attackInterval = 1.0f;

        private int _currentAttackIntervalId;
        private bool _isInitialized;
        private bool _isAttacking;
        private bool _isDodge;
        private Vector2 _moveVector;
        private Transform _cacheTransform;
        private Transform _cameraTransform;
        private IPlayerController _controller;
        private PlayerInputView _playerInputView;
        private PlayerHealthHudPresenter _healthHudPresenter;

        /// <summary> プレイヤー攻撃コントローラー。 </summary>
        public PlayerAttackController PlayerAttackController { get; private set; }

        /// <summary> 毎フレーム移動更新を行う。 </summary>
        private void Update()
        {
            if (!_isInitialized || _controller == null)
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

            _healthHudPresenter?.Dispose();
        }

        /// <summary> 依存コンポーネントを初期化する。 </summary>
        public void Initialize(
            IPlayerController playerMovementController,
            PlayerAttackController playerAttackController,
            Transform cameraTransform,
            PlayerInputView playerInputView,
            PlayerHealthHudPresenter healthHudPresenter)
        {
            _controller = playerMovementController;
            PlayerAttackController = playerAttackController;
            _cameraTransform = cameraTransform;
            _playerInputView = playerInputView;
            _cacheTransform = transform;
            _healthHudPresenter = healthHudPresenter;

            Debug.Assert(_rb != null, $"{nameof(_rb)} is null", this);
            Debug.Assert(_animator != null, $"{nameof(_animator)} is null", this);
            Debug.Assert(_cameraTransform != null, $"{nameof(_cameraTransform)} is null", this);

            _isInitialized = true;
            RegisterActions();
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

            if (PlayerAttackController == null)
            {
                Debug.LogError("[PlayerView] AttackController is null", this);
                return;
            }

            if (PlayerAttackController.ExecuteAttack(out int resultBeatType))
            {
                int attackIntervalId = ++_currentAttackIntervalId;
                AttackCooldown(_attackInterval, attackIntervalId).Forget();

                // 判定ビート種別ごとに再生するSEキュー名を切り替える。
                string cueName = resultBeatType switch
                {
                    1 => "HandgunShoot_3",
                    2 => "RifleShoot_3",
                    3 => "HandgunShoot_2",
                    4 => "RifleShoot_1",
                    6 => "HandgunShoot_1",
                    8 => "RifleShoot_2",
                    _ => string.Empty
                };

                Play(cueName);
            }
        }

        /// <summary>
        /// 攻撃中のクールダウンを管理する。攻撃中は開始から一定時間が経過するまで一部入力を無効化するための_isAttackingフラグを立てる。
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="attackId"></param>
        private async UniTaskVoid AttackCooldown(float duration, int attackId)
        {
            // TODO: 将来的にBPMやアニメーションを考慮した時間設定にすることから、この処理はApplication層に移動予定。
            _isAttacking = true;
            await UniTask.Delay((int)(duration * 1000f));

            // 最新の攻撃IDでなければ無視する
            if (attackId == _currentAttackIntervalId)
            {
                _isAttacking = false;
            }
        }

        /// <summary>
        ///     指定したSEキュー名を再生する。
        /// </summary>
        private void Play(string cueName)
        {
            if (_seSource == null || string.IsNullOrEmpty(cueName))
            {
                Debug.LogWarning("[PlayerView] SE再生をスキップしました（source/cueName不正）", this);
                return;
            }

            _seSource.cueName = cueName;
            _seSource.Play();
        }

        /// <summary> 入力に基づいて移動と向きを更新する。 </summary>
        private void UpdateMovement()
        {
            if (_controller == null)
            {
                return;
            }

            Vector2 dir = _moveVector;

            if (_isAttacking)
            {
                // 攻撃時には移動方向をゼロベクトルにする。
                // 入力方向のキャッシュ_moveVectorは残るため、攻撃終了後はその方向に移動を再開する。
                dir = Vector2.zero;
            }

            _animator.SetFloat(_blendName, Mathf.Min(1f, dir.magnitude));
            dir = Rotate(dir, -_cameraTransform.eulerAngles.y);

            if (_isDodge)
            {
                // 移動入力がない場合は、前方を回避方向とする
                if (dir.sqrMagnitude <= float.Epsilon)
                {
                    var fwd = _cacheTransform.forward;
                    dir.x = fwd.x;
                    dir.y = fwd.z;
                }

                _controller.TryDodge(dir, Time.time);
                _isDodge = false;
            }

            Quaternion rotation = _cacheTransform.rotation;
            _controller.Update(ref rotation, dir, Time.time, out Vector3 velocity);
            _rb.linearVelocity = velocity;
            _cacheTransform.rotation = rotation;
        }

        /// <summary> 2Dベクトルを指定角度だけ回転させる。 </summary>
        private static Vector2 Rotate(Vector2 v, float degrees)
            => Quaternion.Euler(0, 0, degrees) * v;
    }
}

