using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     プレイヤーの管理クラス。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerManager : MonoBehaviour
    {
        public void Init(InputBuffer inputBuffer)
        {
            _inputBuffer = inputBuffer;
        }

        [SerializeField, Tooltip("プレイヤーのステータス")]
        private PlayerStatus _playerStatus;

        [SerializeField, Tooltip("コンフィグ")]
        private PlayerConfig _config;

        private InputBuffer _inputBuffer;
        private PlayerMover _playerMover;

        private Vector3 _moveInput;

        private void OnEnable()
        {
            Rigidbody rb = GetComponent<Rigidbody>();

            _playerMover = new PlayerMover(_playerStatus, _config,
                transform, Camera.main.transform, rb);

            InputEventRegister(_inputBuffer);
        }

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

        private void InputEventRegister(InputBuffer buffer)
        {
            if (buffer == null) { return; }

            buffer.MoveAction.Performed += HandleInputMove;
            buffer.MoveAction.Canceled += HandleInputMove;
        }

        private void InputEventUnregister(InputBuffer buffer)
        {
            if (buffer == null) { return; }

            buffer.MoveAction.Performed -= HandleInputMove;
            buffer.MoveAction.Canceled -= HandleInputMove;
        }

        private void HandleInputMove(Vector2 input)
        {
            _moveInput = new Vector3(input.x, 0, input.y);
        }
    }
}