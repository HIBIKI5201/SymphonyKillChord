using UnityEngine;

namespace Mock.MusicBattle.Camera
{
    /// <summary>
    ///     カメラを移動させるモジュール。
    /// </summary>
    public class CameraMover
    {
        public CameraMover(CameraConfigs config, Transform camera, Transform target)
        {
            _config = config;
            _camera = camera;
            _target = target;
        }

        /// <summary>
        ///     カメラの回転入力で更新する。
        /// </summary>
        /// <param name="input"></param>
        public void RotateCamera(Vector2 input)
        {
            // ロックオンモード中は実行しない。
            if (IsLockOnMode()) { return; }

            // 入力値から回転量を取得。
            float yaw = input.x * _config.CameraRotationSpeed * (_config.IsCameraFlipX ? -1 : 1);
            float pitch = -input.y * _config.CameraRotationSpeed;

            // 新たな角度を割り当てる。
            _currentPitch = Mathf.Clamp(_currentPitch + pitch, _config.PitchRangeMin, _config.PitchRangeMax);
            _currentYaw += yaw;
            _currentCameraRotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        }

        /// <summary>
        ///     ヨー方向の更新。
        ///     カメラの横回転を制御する。
        /// </summary>
        public void UpdateYaw(float deltaTime)
        {
            // ロック対象がいる場合はその方向、そうでなければ現在のカメラ回転を使う。
            Quaternion rotation = IsLockOnMode() ? GetLockYaw() : _currentCameraRotation;

            // 回転を考慮して位置を計算する。
            Vector3 targetPosition = _target.position + rotation * _config.CameraOffset;

            // 障害物があったら補正する。
            targetPosition = AdjustCameraForObstacles(targetPosition);

            // Damping補完。
            float damping = Mathf.Max(_config.CameraFollowDamping, 0.0001f);
            float t = 1f - Mathf.Exp(-deltaTime / damping);
            _camera.position = Vector3.Lerp(_camera.position, targetPosition, t);
        }

        /// <summary>
        ///     ピッチ方向の更新。
        ///     カメラの縦回転を制御する。
        /// </summary>
        public void UpdatePitch(float deltaTime)
        {
            // ロック対象がいる場合はその方向、そうでなければプレイヤー方向を使う。
            Quaternion targetRotation = IsLockOnMode() ? LockTargetPitch() : PlayerPitch();

            // Damping補完。
            float damping = Mathf.Max(_config.CameraLookAtDamping, 0.0001f);
            float t = 1f - Mathf.Exp(-deltaTime / damping);
            _camera.rotation = Quaternion.Slerp(_camera.rotation, targetRotation, t);
        }

        /// <summary>
        ///    ギズモの描画。
        /// </summary>
        public void OnDrawGizmos()
        {
            if (_lockTarget == null) { return; }

            // 見ている方向に線を出す。
            Vector3 lookDir = _lockTarget.position - _camera.position;
            if (lookDir.sqrMagnitude <= 0.0001f) lookDir = _camera.forward;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_camera.position, _camera.position + lookDir.normalized * 5f);
        }

        /// <summary>
        ///     ロック対象を設定する。
        /// </summary>
        /// <param name="lockTarget"></param>
        public void SetLockTarget(Transform lockTarget) => _lockTarget = lockTarget;

        private readonly CameraConfigs _config;
        private readonly Transform _camera;
        private readonly Transform _target;

        private Transform _lockTarget;

        private float _currentYaw;
        private float _currentPitch;
        private Quaternion _currentCameraRotation = Quaternion.identity;


        /// <summary>
        ///    ロック対象の方向を向くためのヨー回転を取得する。
        /// </summary>
        /// <returns></returns>
        private Quaternion GetLockYaw()
        {
            Vector3 vec = _lockTarget.position - _target.position;
            vec.y = 0f;

            // 回転が必要なほど敵が離れているか。
            if (vec.sqrMagnitude > 0.001f)
            {
                float targetYaw = Mathf.Atan2(vec.x, vec.z) * Mathf.Rad2Deg;
                return Quaternion.Euler(0f, targetYaw, 0f);
            }

            return Quaternion.identity;
        }

        /// <summary>
        ///    カメラとプレイヤーの間に障害物があるかを確認。
        ///    障害物があればカメラ位置を調整する。
        /// </summary>
        private Vector3 AdjustCameraForObstacles(Vector3 cameraPosition)
        {
            // プレイヤーからカメラへの方向ベクトル。
            Vector3 rayDirection = cameraPosition - _target.position;
            float distance = rayDirection.magnitude;

            // プレイヤーから少し上の位置から開始（足元の床を避ける）。
            Vector3 startPosition = _target.position + Vector3.up * 1f;

            // レイキャストで障害物を検出。
            if (!Physics.SphereCast(
                startPosition, 
                _config.CameraCollisionRadius, 
                rayDirection.normalized,
                out RaycastHit hitInfo, 
                distance)
                || hitInfo.rigidbody.transform == _target)  // プレイヤー自身に当たった場合は無視。
            {
                return cameraPosition;
            }

            // 障害物がある場合、カメラ位置を調整。
            return hitInfo.point + hitInfo.normal * _config.CameraCollisionRadius;

        }

        /// <summary>
        ///    ロック対象のピッチ方向を向く回転を取得する。
        /// </summary>
        /// <returns></returns>
        private Quaternion LockTargetPitch()
        {
            // ロックターゲットの方向を取得。
            Vector3 vec = _lockTarget.position - _camera.position;

            // 方向ベクトルが0の場合はforwardにする。
            if (vec.sqrMagnitude <= 0.0001f) { vec = _camera.forward; }
            
            return Quaternion.LookRotation(vec.normalized, Vector3.up);
        }

        /// <summary>
        ///   プレイヤーのピッチ方向を向く回転を取得する。
        /// </summary>
        /// <returns></returns>
        private Quaternion PlayerPitch()
        {
            // カメラの回転位置から注視位置を取得。
            Vector3 rotatedLookAtOffset = 
                _currentCameraRotation * _config.CameraLookAtOffset;
            Vector3 lookAtPos = _target.position + rotatedLookAtOffset;
            
            return Quaternion.LookRotation(lookAtPos - _camera.position);
        }

        /// <summary>
        ///     ロックオンモードかどうか。
        /// </summary>
        /// <returns></returns>
        private bool IsLockOnMode() => _lockTarget != null;
    }
}
