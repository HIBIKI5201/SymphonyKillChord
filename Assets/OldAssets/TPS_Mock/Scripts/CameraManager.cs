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
        [SerializeField]
        private CameraConfig _config;

        private InputBuffer _inputBuffer;
        private CameraMover _mover;

        private CinemachineCamera _camera;

        private void Awake()
        {
            if (TryGetComponent(out _camera))
            {
                _mover = new CameraMover(_config, transform, _camera.Follow);
            }
        }

        private void OnEnable()
        {
            _inputBuffer = FindAnyObjectByType<InputBuffer>();
            if (_inputBuffer != null)
            {
                _inputBuffer.LookAction.Performed += _mover.RotateCamera;
                _inputBuffer.LookAction.Canceled += _mover.RotateCamera;
            }
        }

        private void OnDisable()
        {
            if (_inputBuffer != null)
            {
                _inputBuffer.LookAction.Performed -= _mover.RotateCamera;
                _inputBuffer.LookAction.Canceled -= _mover.RotateCamera;
            }
        }

        private void Update()
        {
            _mover.Update();
        }

        private void OnDrawGizmosSelected()
        {
            // カメラが無ければ取得を試みる。
            if (_camera == null && !TryGetComponent(out _camera)) { return; } 

            Vector3 position = _camera.Follow.position + _config.CameraOffset;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(position, 0.2f);
        }
    }
}