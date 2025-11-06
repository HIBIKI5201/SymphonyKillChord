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
            MoveCamera();
        }

        private void MoveCamera()
        {
            // カメラ位置をターゲットのオフセット位置に設定。
            Vector3 targetPosition = _target.position + _config.CameraOffset;
            _camera.position = Vector3.Lerp(_camera.position, targetPosition,
                Mathf.Clamp01(Time.deltaTime));
        }

        private void RotateCamera()
        {
            // カメラの回転をターゲットの回転に合わせる。
            Quaternion targetRotation = Quaternion.LookRotation(_target.position - _camera.position);
            _camera.rotation = Quaternion.Slerp(
                _camera.rotation,
                targetRotation,
                Mathf.Clamp01(Time.deltaTime * _config.CameraRotationSpeed));
        }

        private readonly CameraConfig _config;
        private readonly Transform _camera;
        private readonly Transform _target;
    }
}