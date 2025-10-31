using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     入力バッファクラス。
    /// </summary>
    public class InputBuffer : MonoBehaviour
    {
        /// <summary> 視点移動方向プロパティ </summary>
        public Vector2 LookDirection => _lookDirection;

        /// <summary> 移動方向プロパティ </summary>
        public Vector3 MoveDirection => _moveDirection;

        private Vector2 _lookDirection;
        private Vector3 _moveDirection;

        private void Update()
        {
            _moveDirection = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            _moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        }
    }
}