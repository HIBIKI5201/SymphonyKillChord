using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     カメラ移動クラス。
    /// </summary>
    public class CameraMover
    {
        public CameraMover(Transform camera, Transform target)
        {
            _camera = camera;
            _target = target;
        }

        private readonly Transform _camera;
        private readonly Transform _target;
    }
}