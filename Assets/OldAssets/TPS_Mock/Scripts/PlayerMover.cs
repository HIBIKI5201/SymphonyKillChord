 using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     プレイヤーの移動計算クラス。
    /// </summary>
    public class PlayerMover
    {
        public PlayerMover(Transform player, Transform camera)
        {
            _player = player;
            _camera = camera;
        }

        /// <summary>
        ///     カメラ正面方向を基準にプレイヤーの移動量を計算する。
        /// </summary>
        /// <param name="inputDirection"></param>
        public Vector3 CalcPlayerVelocityByInputDirection(in Vector3 inputDirection)
        {
            // カメラとプレイヤーの差からカメラの前方向を計算。
            Vector3 cameraForward = (_player.position - _camera.position).normalized;
            cameraForward.y = 0f;

            // カメラの右方向を水平面上の垂直方向ベクトルとして計算。
            Vector3 cameraRight = new Vector3(cameraForward.z, 0f, -cameraForward.x);

            // 入力方向をカメラ位置基準で変換。
            Vector3 moveDir = cameraForward * inputDirection.z + cameraRight * inputDirection.x;
            moveDir.y = 0f;

            // 移動量を計算。
            Vector3 velocity = moveDir.normalized * _moveSpeed * Time.deltaTime;

            return velocity;
        }

        private readonly Transform _player;
        private readonly Transform _camera;

        private float _moveSpeed = 5f;
    }
}