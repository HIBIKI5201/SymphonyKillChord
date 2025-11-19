using Mock.MusicBattle.Camera;
using UnityEngine;

namespace Mock.MusicBattle.Camera
{
    /// <summary>
    ///     カメラを移動させるモジュール。
    /// </summary>
    public class CameraMover
    {
        public CameraMover(CameraConfigs config, Transform camera, Transform target)
        {
            _config = config;
            _camera = camera;
            _target = target;
        }

        private CameraConfigs _config;
        private Transform _camera;
        private Transform _target;
    }
}
