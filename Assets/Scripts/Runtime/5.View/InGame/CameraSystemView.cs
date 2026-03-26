using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Utility;
using UnityEngine;

namespace KillChord.Runtime.View
{
    [DefaultExecutionOrder(ExecutionOrderConst.CAMERA_FOLLOW)]
    public sealed class CameraSystemView : MonoBehaviour
    {
        [SerializeField] private Transform _cameraT;

        [SerializeField] private Transform _playerT;

        [SerializeField] private Transform _target;

        [SerializeField] private LockOnState _lockOnState;

        private CameraSystemController _controller;

        public void Init(CameraSystemController controller)
        {
            _controller = controller;
        }
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        void Update()
        {
            UpdateInput(out Vector2 input);
            input *= 200;

            if (_lockOnState == LockOnState.LockOnAuto && input.sqrMagnitude > float.Epsilon)
                _lockOnState = LockOnState.Free;

            _controller.Update(
                _playerT.position,
                _target.position,
                input,
                _lockOnState != LockOnState.Free,
                Time.deltaTime,
                out Quaternion rotation,
                out Vector3 position
            );
            _cameraT.SetPositionAndRotation(position, rotation);

        }
        private void UpdateInput(out Vector2 input)
        {
            if (Input.GetKeyDown(KeyCode.Mouse2))
                if (_lockOnState == LockOnState.Free)
                    _lockOnState = LockOnState.LockOnManual;
                else
                    _lockOnState = LockOnState.Free;
            if (Input.GetKeyDown(KeyCode.Mouse0) && _lockOnState == LockOnState.Free)
                _lockOnState = LockOnState.LockOnAuto;

            input.x = Input.GetAxisRaw("Mouse X");
            input.y = Input.GetAxisRaw("Mouse Y");
        }
        private enum LockOnState : byte
        {
            /// <summary>
            /// 操作によって自由にカメラを回せる
            /// </summary>
            Free,

            /// <summary>
            /// システムによって目標へロックオンした状態
            /// </summary>
            LockOnAuto,

            /// <summary>
            /// 操作によって目標へロックオンした状態
            /// </summary>
            LockOnManual,
        }
    }
}
