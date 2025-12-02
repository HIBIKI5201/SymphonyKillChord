using UnityEngine;

namespace Mock.MusicBattle.Player
{
    public class PlayerMover
    {
        public PlayerMover(PlayerStatus status, Rigidbody rb, Transform player, Transform camera)
        {
            _status = status;
            _player = player;
            _camera = camera;
            _rb = rb;
        }

        // 現在の速度。
        public Vector3 CurrentVelocity {  get; private set; }

        /// <summary>
        ///     入力方向からプレイヤーの速度を計算する。
        /// </summary>
        /// <param name="inputDirection"></param>
        /// <returns></returns>
        public Vector3 CalcPlayerVelocityByInputDirection(Vector2 inputDirection)
        {
            // カメラの向きを基準に移動方向を計算。
            Vector3 cameraForward = (_player.position - _camera.position).normalized;
            cameraForward.y = 0f;
            // カメラの右方向を計算。
            Vector3 cameraRight = new Vector3(cameraForward.z, 0f, -cameraForward.x);
            // 入力方向に基づいて移動ベクトルを計算。
            Vector3 moveDir = cameraForward * inputDirection.y + cameraRight * inputDirection.x;
            moveDir.y = 0f;

            return moveDir.normalized * _status.MoveSpeed;
        }

        /// <summary>
        ///     速度を設定する。
        /// </summary>
        /// <param name="velocity"></param>
        public void SetPlayerVelocity(Vector3 velocity)
        {
            _targetVelocity = velocity;
        }

        public void SetIsGround(bool isGround)
        {
            _isGround = isGround;
        }

        public void Update()
        {
            float t = CalculateAccelerationLerpT();
            UpdateHorizontalVelocity(t);
            UpdateRotation();
        }

        /// <summary>
        ///     加速度補間 t の計算。
        /// </summary>
        /// <returns></returns>
        private float CalculateAccelerationLerpT()
        {
            float acceleration = _targetVelocity.magnitude > 0f
                ? _status.WalkAccelerationDuration
                : _status.StopAccelerationDuration;

            float damping = Mathf.Max(acceleration, 0.0001f);
            return 1f - Mathf.Exp(-Time.deltaTime / damping);
        }

        /// <summary>
        ///   水平方向の速度ベクトルの更新。
        /// </summary>
        /// <param name="t"></param>
        private void UpdateHorizontalVelocity(float t)
        {
            Vector3 horizontalCurrent = new Vector3(CurrentVelocity.x, 0f, CurrentVelocity.z);
            Vector3 horizontalTarget = new Vector3(_targetVelocity.x, 0f, _targetVelocity.z);

            _horizontalVelocity = Vector3.Lerp(horizontalCurrent, horizontalTarget, t);

            CurrentVelocity = new Vector3(
                _horizontalVelocity.x,
                CurrentVelocity.y,
                _horizontalVelocity.z
            );
        }

        /// <summary>
        ///   プレイヤーの回転処理。
        /// </summary>
        private void UpdateRotation()
        {
            // 現在の向きを水平に
            Vector3 forward = _player.forward - new Vector3(0f, _player.forward.y, 0f);
            // 水平方向の速度成分
            Vector3 targetDir = new Vector3(CurrentVelocity.x, 0f, CurrentVelocity.z);
            // Cinemachine式 Damping
            float rotDamping = Mathf.Max(_status.RotationDamping, 0.0001f);
            float rotT = 1f - Mathf.Exp(-Time.deltaTime / rotDamping);
            Vector3 dir = Vector3.Lerp(forward, targetDir, rotT);
            if (dir.sqrMagnitude > 0.0001f)
            {
                _player.LookAt(_player.position + dir.normalized);
            }
        }

        public void FixedUpdate()
        {
            _rb.linearVelocity = new Vector3(CurrentVelocity.x, _rb.linearVelocity.y, CurrentVelocity.z);
            //CurrentVelocity = _rb.linearVelocity;
            Debug.Log(_horizontalVelocity.magnitude);
        }

        private readonly PlayerStatus _status;
        private readonly Transform _player;
        private readonly Transform _camera;
        private readonly Rigidbody _rb;
        // 目標の速度。
        private Vector3 _targetVelocity;
        // 水平方向の速度。
        private Vector3 _horizontalVelocity;
        private bool _isGround;
    }
}
