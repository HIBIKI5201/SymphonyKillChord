using CriWare;
using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Player;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.Utility;
using KillChord.Runtime.View.Persistent.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///     プレイヤーの見た目や制御を管理するViewクラス。
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConst.MOVEMENT)]
    public sealed class PlayerView : MonoBehaviour, IDamageable
    {
        [SerializeField] private string _blendName;
        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private CriAtomSource _seSource;

        private Transform _cameraTransform;
        private bool _isInitialized = false;

        public PlayerAttackController PlayerAttackController => _playerAttackController;

        private PlayerInputView _playerInputView;
        private Vector2 _moveVector;
        private bool _isDodge;

        void Update()
        {
            if (!_isInitialized || _controller == null) return;
            UpdateMovement();
        }

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
            Debug.Assert(_rb != null, $"{nameof(_rb)}がNull", this);
            Debug.Assert(_animator != null, $"{nameof(_animator)}がNull", this);
            Debug.Assert(_cameraTransform != null, $"{nameof(_cameraTransform)}がNull", this);
            _cacheTransform = transform;
            _isInitialized = true;

            RegisterActions();
        }

        private void RegisterActions()
        {
            _playerInputView.OnMoveInput += OnMove;
            _playerInputView.OnAttackInput += OnAttack;
            _playerInputView.OnDodgeInput += OnDodge;
        }

        private void UnRegisterActions()
        {
            _playerInputView.OnMoveInput -= OnMove;
            _playerInputView.OnAttackInput -= OnAttack;
            _playerInputView.OnDodgeInput -= OnDodge;
        }

        private void OnMove(InputContext<Vector2> input)
        {
            _moveVector = input.Value;
        }

        private void OnDodge(InputContext<float> input)
        {
            if (input.Phase == InputActionPhase.Started)
            {
                _isDodge = true;
            }
        }

        private void OnAttack(InputContext<float> input)
        {
            if (input.Phase != InputActionPhase.Started) return;
            if (_playerAttackController == null)
            {
                Debug.LogError("[PlayerView]AttackControllerがnull");
                return;
            }

            if (_playerAttackController.ExecuteAttack(out var resultBeatType))
            {
                Debug.Log($"{gameObject.name}が攻撃を実行", this);
                string cueName = resultBeatType switch
                {
                    1 => "HandgunShoot_3",
                    2 => "RifleShoot_3",
                    3 => "HandgunShoot_2",
                    4 => "RifleShoot_1",
                    6 => "HandgunShoot_1",
                    8 => "RifleShoot_2",
                    _ => null
                };

                Play(cueName);
            }
        }

        private void Play(string cueName)
        {
            _seSource.cueName = cueName;
            _seSource.Play();
        }

        private void UpdateMovement()
        {
            if (_controller == null) return;
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

        private static Vector2 Rotate(Vector2 v, float degrees)
            => Quaternion.Euler(0, 0, degrees) * v;


        private Collider[] _colliders;
        private Transform _cacheTransform;
        private IPlayerController _controller;
        private PlayerAttackController _playerAttackController;
    }
}