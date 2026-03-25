using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Utility;
using System;
using Unity.Cinemachine;
using UnityEngine;

namespace KillChord.Runtime.View
{
    [RequireComponent(typeof(CinemachineCamera))]
    [DefaultExecutionOrder(ExecutionOrderConst.CAMERA_FOLLOW)]
    public class CameraManager : MonoBehaviour, IDisposable
    {
        public bool Init(CameraController controller, Transform followTarget)
        {
            if (controller == null)
            {
                Debug.LogError($"{nameof(CameraController)} is null.");
                return false;
            }
            if (followTarget == null)
            {
                Debug.LogError($"{nameof(followTarget)} is null.");
                return false;
            }

            _controller = controller;
            _followTarget = followTarget;
            return true;
        }

        public void ChangeUpdateMode(CameraUpdateModeEnum mode)
        {
            _mode = mode;
        }

        public void SetLookInput(Vector2 lookInput) => _lookInput = lookInput;

        public void SetLockTarget(Transform target) => _controller?.SetLockTarget(target);

        [SerializeField, Tooltip("カメラのアップデート頻度")]
        private CameraUpdateModeEnum _mode = CameraUpdateModeEnum.Update;

        private void Update()
        {
            if (_mode != CameraUpdateModeEnum.Update) { return; }
            Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (_mode != CameraUpdateModeEnum.FixedUpdate) { return; }
            Tick(Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            if (_mode != CameraUpdateModeEnum.LateUpdate) { return; }
            Tick(Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            _controller?.DrawGizmos(transform);
        }

        public void Dispose() { }

        private void Tick(float deltaTime)
        {
            if (_controller == null || _followTarget == null) { return; }

            _controller.Rotate(_lookInput);
            _controller.Tick(transform, _followTarget, deltaTime);
        }

        private CameraController _controller;
        private Transform _followTarget;
        private Vector2 _lookInput;
    }
}
