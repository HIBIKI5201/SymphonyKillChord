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

        public void LateUpdate()
        {
            UpdateYaw();
            UpdatePitch();
        }

        /// <summary>
        ///     カメラ回転を入力に基づき更新。
        /// </summary>
        /// <param name="input"></param>
        public void RotateCamera(Vector2 input)
        {
            // ロックオン中は入力回転を無効化。
            if (_lockTarget != null) return;

            // 入力に基づき回転量を計算。
            float yaw = input.x * _config.CameraRotationSpeed * (_config.IsCameraFlipX ? -1 : 1);
            float pitch = -input.y * _config.CameraRotationSpeed;

            // 入力に基づきカメラ回転を更新。
            _currentPitch = Mathf.Clamp(_currentPitch + pitch, _config.PicthRangeMin, _config.PicthRangeMax);
            _currentYaw += yaw;

            _currentCameraRotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        }

        /// <summary>
        ///     ギズモ描画。
        /// </summary>
        public void OnDrawGizmos()
        {
            // ロックオン対象がいなければ終了。
            if (_lockTarget == null) return;
            // カメラ位置から敵までの方向を算出。
            Vector3 lookDir = _lockTarget.position - _camera.position;
            if (lookDir.sqrMagnitude <= 0.0001f) { lookDir = _camera.forward; }
            // カメラの注視点を描画。
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_camera.position, _camera.position + lookDir.normalized * 5f);
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
            Quaternion rotation;

            // ロックオン時は敵を向く回転を使用（左右移動の処理を削除）。
            if (_lockTarget != null && TryGetLockYaw(out Quaternion lockRotation))
            {
                rotation = lockRotation;
            }
            else
            {
                rotation = _currentCameraRotation;
            }

            // プレイヤーを中心にカメラを配置。
            Vector3 targetPosition = _target.position + rotation * _config.CameraOffset;

            _camera.position = targetPosition;

            /* 振動バグのため一旦コメントアウト
            _camera.position = Vector3.Lerp(
                _camera.position,
                targetPosition,
                Mathf.Clamp01(Time.deltaTime * _config.CameraFollowSpeed)
            );
            */
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

            _camera.rotation = targetRotation;

            /* 振動バグのため一旦コメントアウト
            _camera.rotation = Quaternion.Slerp(
                _camera.rotation,
                targetRotation,
                Mathf.Clamp01(Time.deltaTime * _config.CameraLookAtSpeed)
            );
            */
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