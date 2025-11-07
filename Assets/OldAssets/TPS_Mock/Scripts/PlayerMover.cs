using Unity.Cinemachine;
using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     プレイヤーの移動計算クラス。
    /// </summary>
    public class PlayerMover
    {
        public PlayerMover(PlayerStatus status, PlayerConfig config,
            Transform player, Transform camera, Rigidbody rb)
        {
            _config = config;
            _status = status;
            _player = player;
            _camera = camera;
            _rb = rb;
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

            // 移動速度を計算。
            Vector3 velocity = moveDir.normalized * _status.MoveSpeed;

            return velocity;
        }

        public void SetPlayerVelocity(Vector3 velocity)
        {
            _velocity = velocity;
        }

        public void Update()
        {
            Vector3 forward = _player.forward - new Vector3(0f, _player.forward.y, 0f);
            Vector3 dir = Vector3.Lerp(forward, new Vector3(_velocity.x, 0f, _velocity.z),
                Mathf.Clamp01(_status.RotationSpeed * Time.deltaTime));
            _player.LookAt(_player.position + dir.normalized);
        }

        public void FixedUpdate()
        {
            Vector3 velocity = new Vector3(_velocity.x, _rb.linearVelocity.y, _velocity.z);
            _rb.linearVelocity = velocity;
        }

        private readonly PlayerStatus _status;
        private readonly PlayerConfig _config;

        private readonly Transform _player;
        private readonly Transform _camera;
        private readonly Rigidbody _rb;

        private Vector3 _velocity;
    }
}