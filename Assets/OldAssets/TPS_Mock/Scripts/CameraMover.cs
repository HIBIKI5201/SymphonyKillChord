using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     カメラ移動クラス。
    /// </summary>
    public class CameraMover
    {
        public CameraMover(Transform camera)
        {
            _camera = camera;
        }

        private readonly Transform _camera;
    }
}