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
            float cameraRotation = input.x * _config.CameraRotationSpeed;
            _currentCameraAngleY += cameraRotation;
        }

        private void UpdateMoveCamera()
        {
            // カメラオフセットを回転させる。
            Quaternion rotation = Quaternion.Euler(0f, _currentCameraAngleY, 0f);
            Vector3 rotatedCameraOffset = rotation * _config.CameraOffset;

            // ターゲット位置からのカメラ目標位置を計算。
            Vector3 targetPosition = _target.position + rotatedCameraOffset;

            _camera.position = Vector3.Lerp(
                _camera.position,
                targetPosition,
                Mathf.Clamp01(Time.deltaTime * _config.CameraFollowSpeed)
            );
        }

        private void UpdateRotateCamera()
        {
            // 注視点オフセットを回転させる。
            Quaternion rotation = Quaternion.Euler(0f, _currentCameraAngleY, 0f);
            Vector3 rotatedLookAtOffset = rotation * _config.CameraLookAtOffset;

            // 注視点方向を算出。
            Vector3 currentLookAtPosition = _target.position + rotatedLookAtOffset;
            Quaternion targetRotation = Quaternion.LookRotation(
                currentLookAtPosition - _camera.position);

            _camera.rotation = Quaternion.Slerp(
                _camera.rotation,
                targetRotation,
                Mathf.Clamp01(Time.deltaTime * _config.CameraLookAtSpeed)
            );
        }

        private readonly CameraConfig _config;
        private readonly Transform _camera;
        private readonly Transform _target;

        private float _currentCameraAngleY = 0f;
    }
}