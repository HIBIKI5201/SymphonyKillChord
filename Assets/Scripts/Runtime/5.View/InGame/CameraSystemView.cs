using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Utility;
using UnityEngine;

namespace KillChord.Runtime.View
{
    [DefaultExecutionOrder(ExecutionOrderConst.CAMERA_FOLLOW)]
    public sealed class CameraSystemView : MonoBehaviour
    {
        [Range(-90, 90)]
        [SerializeField] private float _cameraRotateX;

        [SerializeField] private Transform _cameraT;

        [SerializeField] private Transform _playerT;

        [SerializeField]
        private LockOnState _lockOnState;

        [SerializeField]
        private Transform _target;

        private CameraSystemController _controller;

        public void Init(CameraSystemController controller)
        {
            _controller = controller;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                if (_lockOnState == LockOnState.Free)
                    _lockOnState = LockOnState.LockOnManual;
                else
                    _lockOnState = LockOnState.Free;

            _controller.Update(
                _playerT.position,
                _target.position,
                _lockOnState != LockOnState.Free,
                Time.deltaTime,
                out Quaternion rotation,
                out Vector3 position
            );
            _cameraT.SetPositionAndRotation(position, rotation);

            //screenVelocityX = Mathf.Clamp01((screenVelocityX + 1f) / 2);

            //_cameraRotationZ = Mathf.Lerp(_cameraRotationZ, Mathf.Lerp(1f, -1f, screenVelocityX), Time.deltaTime);



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
