using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     プレイヤーの管理クラス。
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        private InputBuffer _inputBuffer;
        private PlayerMover _playerMover;

        private void OnEnable()
        {
            _playerMover = new PlayerMover(transform, Camera.main.transform);
            _inputBuffer = FindAnyObjectByType<InputBuffer>();
        }

        private void Update()
        {
            Vector3 inputDirection = _inputBuffer.MoveDirection;
            transform.position += _playerMover.CalcPlayerVelocityByInputDirection(in inputDirection);
        }
    }
}