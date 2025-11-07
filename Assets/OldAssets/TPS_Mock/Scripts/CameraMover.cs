using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    /// TPS用カメラ移動クラス。
    /// </summary>
    public class CameraMover
    {
        public CameraMover(CameraConfig config, Transform camera, Transform target)
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
            if (_lockTarget != null) { return; }

            float yaw = input.x * _config.CameraRotationSpeed * (_config.IsCameraFlipX ? -1 : 1);
            float pitch = -input.y * _config.CameraRotationSpeed;

            _currentPitch = Mathf.Clamp(_currentPitch + pitch, _config.PicthRangeMin, _config.PicthRangeMax);
            _currentYaw += yaw;

            _currentCameraRotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        }

        public void LateUpdate()
        {
            UpdateYaw();
            UpdatePitch();
        }

        public void OnDrawGizmos()
        {
            if (_lockTarget == null) { return; }
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

        private readonly CameraConfig _config;
        private readonly Transform _camera;
        private readonly Transform _target;

        private Transform _lockTarget;

        private float _currentYaw;
        private float _currentPitch;
        private Quaternion _currentCameraRotation = Quaternion.identity;

        private void UpdateYaw()
        {
            // ロック対象がいる場合はその方向、そうでなければ現在のカメラ回転を使う。
            Quaternion rotation = (_lockTarget != null && TryGetLockYaw(out Quaternion lockRot))
                ? lockRot
                : _currentCameraRotation;

            Vector3 targetPosition = _target.position + rotation * _config.CameraOffset;

            // Damping補完。
            float damping = Mathf.Max(_config.CameraFollowDamping, 0.0001f);
            float t = 1f - Mathf.Exp(-Time.deltaTime / damping);
            _camera.position = Vector3.Lerp(_camera.position, targetPosition, t);
        }

        private bool TryGetLockYaw(out Quaternion rotation)
        {
            Vector3 toEnemy = _lockTarget.position - _target.position;
            toEnemy.y = 0f;

            // 回転が必要なほど敵が離れているか。
            if (toEnemy.sqrMagnitude > 0.001f)
            {
                float targetYaw = Mathf.Atan2(toEnemy.x, toEnemy.z) * Mathf.Rad2Deg;
                rotation = Quaternion.Euler(0f, targetYaw, 0f);
                return true;
            }

            rotation = Quaternion.identity;
            return false;
        }

        private void UpdatePitch()
        {
            // ロック対象がいる場合はその方向、そうでなければプレイヤー方向を使う。
            Quaternion targetRotation = _lockTarget != null ? LockTargetPitch() : PlayerPitch();

            // Damping補完。
            float damping = Mathf.Max(_config.CameraLookAtDamping, 0.0001f);
            float t = 1f - Mathf.Exp(-Time.deltaTime / damping);
            _camera.rotation = Quaternion.Slerp(_camera.rotation, targetRotation, t);
        }

        private Quaternion LockTargetPitch()
        {
            Vector3 lookDir = _lockTarget.position - _camera.position;
            if (lookDir.sqrMagnitude <= 0.0001f) { lookDir = _camera.forward; }
            return Quaternion.LookRotation(lookDir.normalized, Vector3.up);
        }

        private Quaternion PlayerPitch()
        {
            Vector3 rotatedLookAtOffset = _currentCameraRotation * _config.CameraLookAtOffset;
            Vector3 lookAtPos = _target.position + rotatedLookAtOffset;
            return Quaternion.LookRotation(lookAtPos - _camera.position);
        }
    }
}
