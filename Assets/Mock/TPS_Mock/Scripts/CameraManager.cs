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

            if (TryGetComponent(out _camera))
            {
                _mover = new CameraMover(_config, transform, _camera.Follow);
            }

            InputEventRegister(inputBuffer);
        }

        [SerializeField]
        private CameraConfig _config;

        private InputBuffer _inputBuffer;
        private CameraMover _mover;
        private EnemyContainer _enemyContainer;

        private CinemachineCamera _camera;

        private int _lockTargetIndex = 0;

        private void OnDisable()
        {
            InputEventUnregister(_inputBuffer);
        }

        private void FixedUpdate()
        {
            if (_mover != null)
            {
                _mover.UpdatePitch();
                _mover.UpdateYaw();
            }
        }

        private void OnDrawGizmos()
        {
            _mover?.OnDrawGizmos();
        }

        /// <summary>
        ///   入力イベントの登録。
        /// </summary>
        /// <param name="inputBuffer"></param>
        private void InputEventRegister(InputBuffer inputBuffer)
        {
            if (inputBuffer == null)
            {
                Debug.LogError($"{nameof(InputBuffer)} is null");
                return;
            }
            inputBuffer.LookAction.Performed += _mover.RotateCamera;
            inputBuffer.LookAction.Canceled += _mover.RotateCamera;

            inputBuffer.AttackAction.Started += HandleLockTargetChange;

        }

        /// <summary>
        ///    入力イベントの登録解除。
        /// </summary>
        /// <param name="inputBuffer"></param>
        private void InputEventUnregister(InputBuffer inputBuffer)
        {
            if (inputBuffer == null) { return; }

            inputBuffer.LookAction.Performed -= _mover.RotateCamera;
            inputBuffer.LookAction.Canceled -= _mover.RotateCamera;

            inputBuffer.AttackAction.Started -= HandleLockTargetChange;
        }

        /// <summary>
        ///    ロック対象変更処理。
        /// </summary>
        /// <param name="value"></param>
        private void HandleLockTargetChange(float value)
        {
            _lockTargetIndex++;
            Transform target = _enemyContainer[_lockTargetIndex]?.LockTarget;
            _mover.SetLockTarget(target);
        }
    }
}