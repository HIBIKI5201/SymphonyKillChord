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
            // カメラ位置をターゲットのオフセット位置に設定。
            Vector3 targetPosition = _target.position + _config.CameraOffset;
            _camera.position = Vector3.Lerp(_camera.position, targetPosition,
                Mathf.Clamp01(Time.deltaTime * _config.CameraRotationSpeed));
        }

        private readonly CameraConfig _config;
        private readonly Transform _camera;
        private readonly Transform _target;
    }
}