using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    /// プレイヤーの移動計算クラス。
    /// </summary>
    public class PlayerMover
    {
        public PlayerMover(PlayerStatus status, Rigidbody rb,
            Transform player, Transform camera)
        {
            _status = status;
            _player = player;
            _camera = camera;
            _rb = rb;
        }

        /// <summary>
        ///     入力方向からプレイヤーの速度を計算する。
        /// </summary>
        /// <param name="inputDirection"></param>
        /// <returns></returns>
        public Vector3 CalcPlayerVelocityByInputDirection(in Vector3 inputDirection)
        {
            Vector3 cameraForward = (_player.position - _camera.position).normalized;
            cameraForward.y = 0f;

            Vector3 cameraRight = new Vector3(cameraForward.z, 0f, -cameraForward.x);
            Vector3 moveDir = cameraForward * inputDirection.z + cameraRight * inputDirection.x;
            moveDir.y = 0f;

            return moveDir.normalized * _status.MoveSpeed;
        }

        /// <summary>
        ///     速度を設定する。
        /// </summary>
        /// <param name="velocity"></param>
        public void SetPlayerVelocity(Vector3 velocity)
        {
            _velocity = velocity;
        }

        public void Jump(float jumpForce)
        {
            Vector3 velocity = _rb.linearVelocity;
            velocity.y = jumpForce;
            _rb.linearVelocity = velocity;
        }

        public void Update()
        {
            Vector3 forward = _player.forward - new Vector3(0f, _player.forward.y, 0f);
            Vector3 targetDir = new Vector3(_velocity.x, 0f, _velocity.z);

            //  Cinemachine式Damping（値が大きいほどゆっくり回転）
            float damping = Mathf.Max(_status.RotationDamping, 0.0001f);
            float t = 1f - Mathf.Exp(-Time.deltaTime / damping);

            Vector3 dir = Vector3.Lerp(forward, targetDir, t);
            if (dir.sqrMagnitude > 0.0001f)
                _player.LookAt(_player.position + dir.normalized);
        }

        public void FixedUpdate()
        {
            Vector3 velocity = new Vector3(_velocity.x, _rb.linearVelocity.y, _velocity.z);
            _rb.linearVelocity = velocity;
        }

        private readonly PlayerStatus _status;

        private readonly Transform _player;
        private readonly Transform _camera;
        private readonly Rigidbody _rb;

        private Vector3 _velocity;
    }
}
