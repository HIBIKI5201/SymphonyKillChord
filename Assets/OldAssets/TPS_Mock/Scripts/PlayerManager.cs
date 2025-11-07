using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     プレイヤーの管理クラス。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerManager : MonoBehaviour, ICharacter
    {
        public void Init(InputBuffer inputBuffer, CameraManager cameraManager, HealthbarManager healthbarManager)
        {
            _inputBuffer = inputBuffer;

            Rigidbody rb = GetComponent<Rigidbody>();

            _playerMover = new PlayerMover(_playerStatus, rb,
                transform, Camera.main.transform);
            _playerAttacker = new PlayerAttacker(_playerStatus, _config, cameraManager.transform);
            _healthEntity = new HealthEntity(_playerStatus.MaxHealth);
            _healthEntity.OnHealthChanged += healthbarManager.SetHealthBar;

            InputEventRegister(inputBuffer);
        }

        public void TakeDamage(float damage) => _healthEntity.TakeDamage(damage);

        [SerializeField, Tooltip("プレイヤーのステータス")]
        private PlayerStatus _playerStatus;

        [SerializeField, Tooltip("コンフィグ")]
        private PlayerConfig _config;

        private InputBuffer _inputBuffer;
        private PlayerMover _playerMover;
        private PlayerAttacker _playerAttacker;
        private HealthEntity _healthEntity;

        private Vector3 _moveInput;

        private void OnDisable()
        {
            InputEventUnregister(_inputBuffer);
        }

        private void Update()
        {
            Vector3 velocity = _playerMover.CalcPlayerVelocityByInputDirection(in _moveInput);
            _playerMover.SetPlayerVelocity(velocity);

            _playerMover.Update();
        }

        private void FixedUpdate()
        {
            _playerMover.FixedUpdate();
        }

        private void OnDrawGizmos()
        {
            _playerAttacker?.OnDrawGizmos();
        }

        private void InputEventRegister(InputBuffer buffer)
        {
            if (buffer == null)
            {
                Debug.LogError($"{nameof(InputBuffer)} is null");
                return;
            }

            buffer.MoveAction.Performed += HandleInputMove;
            buffer.MoveAction.Canceled += HandleInputMove;
            buffer.AttackAction.Performed += HandleInputAttack;
        }

        private void InputEventUnregister(InputBuffer buffer)
        {
            if (buffer == null) { return; }

            buffer.MoveAction.Performed -= HandleInputMove;
            buffer.MoveAction.Canceled -= HandleInputMove;
            buffer.AttackAction.Performed -= HandleInputAttack;
        }

        private void HandleInputMove(Vector2 input)
        {
            _moveInput = new Vector3(input.x, 0, input.y);
        }

        private void HandleInputAttack(float input)
        {
            _playerAttacker.Attack();
        }
    }
}