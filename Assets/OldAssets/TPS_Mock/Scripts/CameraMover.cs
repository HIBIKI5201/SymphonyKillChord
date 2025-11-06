using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     TPS用カメラ移動クラス。
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
            UpdateMoveCamera();
            UpdateRotateCamera();
        }

        public void RotateCamera(Vector2 input)
        {
            float yaw = input.x * _config.CameraRotationSpeed;
            float pitch = -input.y * _config.CameraRotationSpeed;

            _currentPitch = Mathf.Clamp(_currentPitch + pitch, _config.PicthRange.x, _config.PicthRange.y);
            _currentYaw += yaw;

            _currentCameraRotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        }

        private void UpdateMoveCamera()
        {
            Vector3 rotatedCameraOffset = _currentCameraRotation * _config.CameraOffset;
            Vector3 targetPosition = _target.position + rotatedCameraOffset;

            _camera.position = Vector3.Lerp(
                _camera.position,
                targetPosition,
                Mathf.Clamp01(Time.deltaTime * _config.CameraFollowSpeed)
            );
        }

        private void UpdateRotateCamera()
        {
            Vector3 rotatedLookAtOffset = _currentCameraRotation * _config.CameraLookAtOffset;
            Vector3 currentLookAtPosition = _target.position + rotatedLookAtOffset;

            Quaternion targetRotation = Quaternion.LookRotation(currentLookAtPosition - _camera.position);
            _camera.rotation = Quaternion.Slerp(
                _camera.rotation,
                targetRotation,
                Mathf.Clamp01(Time.deltaTime * _config.CameraLookAtSpeed)
            );
        }

        private readonly CameraConfig _config;
        private readonly Transform _camera;
        private readonly Transform _target;

        private float _currentYaw = 0f;
        private float _currentPitch = 0f;
        private Quaternion _currentCameraRotation = Quaternion.identity;
    }
}
