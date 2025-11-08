using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     プレイヤーの管理クラス。
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField, Tooltip("カメラのX回転を反転")]
        private bool _cameraMoveXFlip = false;

        private InputBuffer _inputBuffer;
        private PlayerMover _playerMover;

        private void OnEnable()
        {
            _playerMover = new PlayerMover(transform, Camera.main.transform);
            _inputBuffer = FindAnyObjectByType<InputBuffer>();

            InputEventRegister(_inputBuffer);
        }

        private void OnDisable()
        {
            InputEventUnregister(_inputBuffer);
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
            float cameraX = input.x;
            if (_cameraMoveXFlip) { cameraX = -cameraX; } // X軸反転設定が有効な場合は反転。
            transform.Rotate(0f, cameraX, 0f); // プレイヤーのY軸回転。
        }

        private void HandleInputMove(Vector2 input)
        {
            Vector3 inputDirection = new Vector3(input.x, 0, input.y);
            transform.position += _playerMover.CalcPlayerVelocityByInputDirection(in inputDirection);
        }
    }
}