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

        private readonly CameraConfig _config;
        private readonly Transform _camera;
        private readonly Transform _target;
    }
}