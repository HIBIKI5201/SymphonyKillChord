using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     TPS用カメラ移動クラス（ロックオン時は敵を中央に捉える）
    /// </summary>
    public class CameraMover
    {
        public CameraMover(CameraConfig config, Transform camera, Transform target)
        {
            _config = config;
            _camera = camera;
            _target = target;
        }

        public void Update()
        {
            UpdateYaw();
            UpdatePitch();
        }

        public void RotateCamera(Vector2 input)
        {
            // ロックオン中は入力回転を無効化。
            if (_lockTarget != null) return;

            float yaw = input.x * _config.CameraRotationSpeed;
            float pitch = -input.y * _config.CameraRotationSpeed;

            _currentPitch = Mathf.Clamp(_currentPitch + pitch, _config.PicthRange.x, _config.PicthRange.y);
            _currentYaw += yaw;

            _currentCameraRotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        }

        public void SetLockTarget(Transform lockTarget) => _lockTarget = lockTarget;

        private readonly CameraConfig _config;
        private readonly Transform _camera;
        private readonly Transform _target;

        private Transform _lockTarget;

        private float _currentYaw = 0f;
        private float _currentPitch = 0f;
        private Quaternion _currentCameraRotation = Quaternion.identity;

        private void UpdateYaw()
        {
            Quaternion rotation = _currentCameraRotation;

            // ロックオン時は敵を向く回転を使用（左右移動の処理を削除）。
            if (_lockTarget != null && TryGetLockYaw(out Quaternion lockRotation))
            {
                rotation = lockRotation;
            }

            // プレイヤーを中心にカメラを配置。
            Vector3 targetPosition = _target.position + rotation * _config.CameraOffset;

            _camera.position = Vector3.Lerp(
                _camera.position,
                targetPosition,
                Mathf.Clamp01(Time.deltaTime * _config.CameraFollowSpeed)
            );
        }

        /// <summary>
        /// ロックオン時のカメラ回転を取得（敵の方向を向く）
        /// </summary>
        private bool TryGetLockYaw(out Quaternion rotation)
        {
            Vector3 toEnemy = _lockTarget.position - _target.position;
            toEnemy.y = 0f; // 水平方向のみ

            if (toEnemy.sqrMagnitude > 0.001f)
            {
                // 🎯 敵の方向を向くYaw角度を計算
                float targetYaw = Mathf.Atan2(toEnemy.x, toEnemy.z) * Mathf.Rad2Deg;

                rotation = Quaternion.Euler(0f, targetYaw, 0f);
                return true;
            }

            rotation = Quaternion.identity;
            return false;
        }

        private void UpdatePitch()
        {
            Quaternion targetRotation;

            if (_lockTarget != null)
            {
                targetRotation = LockTargetPitch();
            }
            else
            {
                targetRotation = PlayerPitch();
            }

            _camera.rotation = Quaternion.Slerp(
                _camera.rotation,
                targetRotation,
                Mathf.Clamp01(Time.deltaTime * _config.CameraLookAtSpeed)
            );
        }

        /// <summary>
        ///     ロック中：敵を真ん中に捉える。
        /// </summary>
        /// <returns></returns>
        private Quaternion LockTargetPitch()
        {
            // カメラ位置から敵までの方向を算出。
            Vector3 lookDir = _lockTarget.position - _camera.position;
            if (lookDir.sqrMagnitude <= 0.0001f) { lookDir = _camera.forward; }

            return Quaternion.LookRotation(lookDir.normalized, Vector3.up);
        }

        /// <summary>
        ///     通常モード：プレイヤーの背中を見る。
        /// </summary>
        /// <returns></returns>
        private Quaternion PlayerPitch()
        {
            Vector3 rotatedLookAtOffset = _currentCameraRotation * _config.CameraLookAtOffset;
            Vector3 lookAtPos = _target.position + rotatedLookAtOffset;
            return Quaternion.LookRotation(lookAtPos - _camera.position);
        }
    }
}