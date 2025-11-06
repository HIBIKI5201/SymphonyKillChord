using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     プレイヤーの管理クラス。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField,Tooltip("プレイヤーのステータス")]
        private PlayerStatus _playerStatus = new PlayerStatus();

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

            _inputBuffer = FindAnyObjectByType<InputBuffer>();
            InputEventRegister(_inputBuffer);
        }

        private void OnDisable()
        {
            InputEventUnregister(_inputBuffer);
        }

        private void OnValidate()
        {
        }

        private void InputEventRegister(InputBuffer buffer)
        {
            if (buffer == null) { return; }

            buffer.LookAction.Performed += HandleInputLook;
            buffer.LookAction.Canceled += HandleInputLook;
            buffer.MoveAction.Performed += HandleInputMove;
            buffer.MoveAction.Canceled += HandleInputMove;
        }

        private void InputEventUnregister(InputBuffer buffer)
        {
            if (buffer == null) { return; }

            buffer.LookAction.Performed -= HandleInputLook;
            buffer.LookAction.Canceled -= HandleInputLook;
            buffer.MoveAction.Performed -= HandleInputMove;
            buffer.MoveAction.Canceled -= HandleInputMove;
        }

        private void HandleInputLook(Vector2 input)
        {
            _playerMover.RotatePlayer(input);
        }

        private void HandleInputMove(Vector2 input)
        {
            _moveInput = new Vector3(input.x, 0, input.y);
            Vector3 velocity = _playerMover.CalcPlayerVelocityByInputDirection(in _moveInput);
            _playerMover.SetPlayerVelocity(velocity);
        }
    }
}