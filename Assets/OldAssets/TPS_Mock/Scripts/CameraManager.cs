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
        public void Init(InputBuffer inputBuffer, EnemyContainer enemyContainer)
        {
            _enemyContainer = enemyContainer;

            _inputBuffer = inputBuffer;
            InputEventRegister(inputBuffer);


            if (TryGetComponent(out _camera))
            {
                _mover = new CameraMover(_config, transform, _camera.Follow);
            }
        }

        [SerializeField]
        private CameraConfig _config;

        private InputBuffer _inputBuffer;
        private CameraMover _mover;
        private EnemyContainer _enemyContainer;

        private CinemachineCamera _camera;

        private void OnDisable()
        {
            InputEventUnregister(_inputBuffer);
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

        private void InputEventRegister(InputBuffer inputBuffer)
        {
            if (inputBuffer != null)
            {
                inputBuffer.LookAction.Performed += _mover.RotateCamera;
                inputBuffer.LookAction.Canceled += _mover.RotateCamera;
            }
            else
            {
                Debug.LogError($"{nameof(InputBuffer)} is null");
            }
        }

        private void InputEventUnregister(InputBuffer inputBuffer)
        {
            if (inputBuffer != null)
            {
                inputBuffer.LookAction.Performed -= _mover.RotateCamera;
                inputBuffer.LookAction.Canceled -= _mover.RotateCamera;
            }
        }
    }
}