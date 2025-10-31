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
        }

        private void Update()
        {
            float cameraX = _inputBuffer.LookDirection.x;
            if (_cameraMoveXFlip) { cameraX = -cameraX; } // X軸反転設定が有効な場合は反転。
            transform.Rotate(0f, cameraX, 0f); // プレイヤーのY軸回転。

            // プレイヤー移動。
            Vector3 inputDirection = _inputBuffer.MoveDirection;
            transform.position += _playerMover.CalcPlayerVelocityByInputDirection(in inputDirection);
        }
    }
}