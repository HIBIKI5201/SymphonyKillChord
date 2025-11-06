using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     カメラ移動クラス。
    /// </summary>
    public class CameraMover
    {
        public CameraMover(CameraConfig config,
            Transform camera, Transform target)
        {
            _config = config;
            _camera = camera;
            _target = target;
        }

        public void Update()
        {
            UpdateMoveCamera();
            UpdateRotateCamera();
        }

        public void RotateCamera(Vector2 input)
        {
            float cameraRotation = input.x * _config.CameraRotationSpeed;
            _currentCameraAngleY += cameraRotation;
        }

        private void UpdateMoveCamera()
        {
            // カメラ位置をターゲットのオフセット位置に設定。
            Vector3 targetPosition = _target.position
                + Quaternion.Euler(0f, _currentCameraAngleY, 0f) * _config.CameraOffset;
            _camera.position = Vector3.Lerp(_camera.position, targetPosition,
                Mathf.Clamp01(Time.deltaTime * _config.CameraFollowSpeed));
        }

        private void UpdateRotateCamera()
        {
            // カメラの回転をターゲットの回転に合わせる。
            Quaternion targetRotation = Quaternion.LookRotation(
                _target.position + _config.CameraLookAtOffset - _camera.position);

            _camera.rotation = Quaternion.Slerp(
                _camera.rotation,
                targetRotation,
                Mathf.Clamp01(Time.deltaTime * _config.CameraLookAtSpeed));
        }

        private readonly CameraConfig _config;
        private readonly Transform _camera;
        private readonly Transform _target;

        private float _currentCameraAngleY = 0f;
    }
}