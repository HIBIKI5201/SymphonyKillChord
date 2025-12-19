using UnityEngine;

namespace Mock.MusicBattle.Camera
{
    /// <summary>
    ///     カメラを移動させるモジュール。
    /// </summary>
    public class CameraMover
    {
        /// <summary>
        ///     <see cref="CameraMover"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="config">カメラの設定。</param>
        /// <param name="camera">操作するカメラのTransform。</param>
        /// <param name="target">カメラの追跡対象となるターゲットのTransform。</param>
        public CameraMover(CameraConfigs config, Transform camera, Transform target)
        {
            _config = config;
            _camera = camera;
            _target = target;
        }

        // PUBLIC_EVENTS
        // PUBLIC_PROPERTIES
        // INTERFACE_PROPERTIES
        // PUBLIC_CONSTANTS
        #region Publicメソッド
        /// <summary>
        ///     カメラの回転入力で更新します。
        /// </summary>
        /// <param name="input">入力ベクトル。</param>
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
        ///     カメラのヨー（横）方向の回転を更新します。
        /// </summary>
        /// <param name="deltaTime">前回のフレームからの経過時間。</param>
        public void UpdateYaw(float deltaTime)
        {
            // ロック対象がいる場合はその方向、そうでなければ現在のカメラ回転を使う。
            Quaternion rotation = IsLockOnMode() ? GetLockYaw() : _currentCameraRotation;

            // ターゲットからの理想的なカメラオフセットを計算。
            Vector3 idealCameraOffset = rotation * _config.CameraOffset;
            Vector3 currentCameraOffset = _camera.position - _target.position;

            // オフセットを線形補間し、ターゲットからの新しいオフセットを計算。
            float dampingSpeed = IsLockOnMode() ? _config.CameraLockOnFollowDamping : _config.CameraPlayerFollowDamping;
            float damping = Mathf.Max(dampingSpeed, 0.0001f);
            float t = 1f - Mathf.Exp(-deltaTime / damping);
            Vector3 slerpedCameraOffset = Vector3.Slerp(currentCameraOffset, idealCameraOffset, t);

            // 障害物調整前のカメラ位置を計算。
            Vector3 preAdjustedCameraPosition = _target.position + slerpedCameraOffset;

            // 障害物があったら補正する。
            Vector3 finalCameraPosition = AdjustCameraForObstacles(preAdjustedCameraPosition);

            // カメラ位置を更新。
            _camera.position = finalCameraPosition;
        }

        /// <summary>
        ///     カメラのピッチ（縦）方向の回転を更新します。
        /// </summary>
        /// <param name="deltaTime">前回のフレームからの経過時間。</param>
        public void UpdatePitch(float deltaTime)
        {
            // ロック対象がいる場合はその方向、そうでなければプレイヤー方向を使う。
            Quaternion targetRotation = IsLockOnMode() ? LockTargetPitch() : PlayerPitch();

            // Damping補完。
            float dampingSpeed = IsLockOnMode() ? _config.CameraLockOnLookAtDamping : _config.CameraPlayerLookAtDamping;
            float damping = Mathf.Max(dampingSpeed, 0.0001f);
            float t = 1f - Mathf.Exp(-deltaTime / damping);
            _camera.rotation = Quaternion.Slerp(_camera.rotation, targetRotation, t);
        }

        /// <summary>
        ///    ギズモの描画を行います（デバッグ用）。
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
        ///     ロック対象を設定します。
        /// </summary>
        /// <param name="lockTarget">ロック対象のTransform。</param>
        public void SetLockTarget(Transform lockTarget) => _lockTarget = lockTarget;
        #endregion

        // PUBLIC_INTERFACE_METHODS
        // PUBLIC_ENUM_DEFINITIONS
        // PUBLIC_CLASS_DEFINITIONS
        // PUBLIC_STRUCT_DEFINITIONS
        #region 定数
        // 定数なし
        #endregion

        // INSPECTOR_FIELDS
        #region プライベートフィールド
        /// <summary> カメラの設定。 </summary>
        private readonly CameraConfigs _config;
        /// <summary> 操作するカメラのTransform。 </summary>
        private readonly Transform _camera;
        /// <summary> カメラの追跡対象となるターゲットのTransform。 </summary>
        private readonly Transform _target;
        /// <summary> 現在ロックオン中のターゲット。 </summary>
        private Transform _lockTarget;
        /// <summary> 現在のカメラのヨー角。 </summary>
        private float _currentYaw;
        /// <summary> 現在のカメラのピッチ角。 </summary>
        private float _currentPitch;
        /// <summary> 現在のカメラの回転。 </summary>
        private Quaternion _currentCameraRotation = Quaternion.identity;
        #endregion

        // UNITY_LIFECYCLE_METHODS
        // EVENT_HANDLER_METHODS
        // PROTECTED_INTERFACE_VIRTUAL_METHODS
        #region Privateメソッド
        /// <summary>
        ///    ロック対象の方向を向くためのヨー回転を取得します。
        /// </summary>
        /// <returns>ロック対象の方向を向くQuaternion。</returns>
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
        ///    カメラとターゲットの間に障害物があるかを確認し、障害物があればカメラ位置を調整します。
        /// </summary>
        /// <param name="cameraPosition">調整前のカメラの位置。</param>
        /// <returns>調整後のカメラの位置。</returns>
        private Vector3 AdjustCameraForObstacles(Vector3 cameraPosition)
        {
            // プレイヤーからカメラへの方向ベクトル。
            Vector3 origin = _target.position + _config.CameraCollisionOffset;
            Vector3 rayDirection = cameraPosition - origin;
            float distance = rayDirection.magnitude + _config.CameraCollisionRadius;

            // レイキャストで障害物を検出。
            if (!Physics.SphereCast(
                origin,
                _config.CameraCollisionRadius,
                rayDirection.normalized,
                out RaycastHit hitInfo,
                distance)
                || hitInfo.rigidbody?.transform == _target)  // プレイヤー自身に当たった場合は無視。
            {
                return cameraPosition;
            }

            // 障害物がある場合、カメラ位置を調整。
            return hitInfo.point + hitInfo.normal * _config.CameraCollisionRadius;
        }

        /// <summary>
        ///    ロック対象のピッチ方向を向く回転を取得します。
        /// </summary>
        /// <returns>ロック対象のピッチ方向を向くQuaternion。</returns>
        private Quaternion LockTargetPitch()
        {
            // ロックターゲットの方向を取得。
            Vector3 vec = _lockTarget.position - _camera.position;

            // 方向ベクトルが0の場合はforwardにする。
            if (vec.sqrMagnitude <= 0.0001f) { vec = _camera.forward; }

            return Quaternion.LookRotation(vec.normalized, Vector3.up);
        }

        /// <summary>
        ///   ターゲットのピッチ方向を向く回転を取得します。
        /// </summary>
        /// <returns>ターゲットのピッチ方向を向くQuaternion。</returns>
        private Quaternion PlayerPitch()
        {
            // カメラの回転位置から注視位置を取得。
            Vector3 rotatedLookAtOffset =
                _currentCameraRotation * _config.CameraLookAtOffset;
            Vector3 lookAtPos = _target.position + rotatedLookAtOffset;

            return Quaternion.LookRotation(lookAtPos - _camera.position);
        }

        /// <summary>
        ///     現在ロックオンモードかどうかを返します。
        /// </summary>
        /// <returns>ロックオンモードの場合はtrue、それ以外はfalse。</returns>
        private bool IsLockOnMode() => _lockTarget != null;
        #endregion
        // PRIVATE_ENUM_DEFINITIONS
        // PRIVATE_CLASS_DEFINITIONS
        // PRIVATE_STRUCT_DEFINITIONS
    }
}
