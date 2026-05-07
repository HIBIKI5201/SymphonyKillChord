using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Player;
using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.Utility;
using KillChord.Runtime.View.Persistent.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///     プレイヤー入力を受け取り移動と攻撃を更新するViewクラス。
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConst.MOVEMENT)]
    public sealed class PlayerView : MonoBehaviour, IDamageable
    {
        [SerializeField, Tooltip("移動ブレンドに使用するAnimatorパラメータ名。")] private string _blendName;
        [SerializeField, Tooltip("プレイヤーのAnimator。")] private Animator _animator;
        [SerializeField, Tooltip("プレイヤーのRigidbody。")] private Rigidbody _rb;

        private Transform _cameraTransform;
        private bool _isInitialized;
        private PlayerInputView _playerInputView;
        private Vector2 _moveVector;
        private bool _isDodge;
        private Collider[] _colliders;
        private Transform _cacheTransform;
        private IPlayerController _controller;
        private PlayerAttackController _playerAttackController;

        /// <summary> プレイヤー攻撃コントローラー。 </summary>
        public PlayerAttackController PlayerAttackController => _playerAttackController;

        /// <summary> 毎フレーム移動更新を行う。 </summary>
        private void Update()
        {
            if (!_isInitialized || _controller == null)
            {
                return;
            }

            UpdateMovement();
        }

        /// <summary> 依存コンポーネントを初期化する。 </summary>
        public void Initialize(
            IPlayerController playerMovementController,
            PlayerAttackController playerAttackController,
            Transform cameraTransform,
            PlayerInputView playerInputView)
        {
            _controller = playerMovementController;
            _playerAttackController = playerAttackController;
            _cameraTransform = cameraTransform;
            _playerInputView = playerInputView;
            _colliders = new Collider[8];
            _cacheTransform = transform;

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

        /// <summary> 攻撃入力を処理する。 </summary>
        private void OnAttack(InputContext<float> input)
        {
            if (input.Phase != InputActionPhase.Started)
            {
                return;
            }

            if (_playerAttackController == null)
            {
                Debug.LogError("[PlayerView] AttackController is null");
                return;
            }

            _playerAttackController.ExecuteAttack();
        }

        /// <summary> 入力に基づいて移動と向きを更新する。 </summary>
        private void UpdateMovement()
        {
            if (_controller == null)
            {
                return;
            }

            Vector2 dir = _moveVector;
            _animator.SetFloat(_blendName, Mathf.Min(1f, dir.magnitude));
            dir = Rotate(dir, -_cameraTransform.eulerAngles.y);

            if (_isDodge)
            {
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
