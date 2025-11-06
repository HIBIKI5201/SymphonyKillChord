using Unity.Cinemachine;
using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     カメラの管理クラス。
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraManager : MonoBehaviour
    {
        private CameraMover _mover;

        private CinemachineCamera _camera;

        private void Awake()
        {
            if (TryGetComponent(out _camera))
            {
                _mover = new CameraMover(transform, _camera.Follow);
            }
        }
    }
}