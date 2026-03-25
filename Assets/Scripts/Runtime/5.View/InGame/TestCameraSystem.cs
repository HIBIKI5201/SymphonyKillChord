using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Utility;
using UnityEngine;

namespace KillChord.Runtime.View
{
    [DefaultExecutionOrder(ExecutionOrderConst.CAMERA_FOLLOW)]
    public sealed class TestCameraSystem : MonoBehaviour
    {
        [Range(-90, 90)]
        [SerializeField] private float _cameraRotateX;

        [SerializeField] private Transform _cameraT;

        [SerializeField] private Transform _playerT;


        [SerializeField] private Vector3 _cameraOffset;

        private Vector3 _localCenterOffset;
        private float _cameraRotationZ;

        [SerializeField]
        private LockOnState _lockOnState;

        [SerializeField]
        private Transform _target;

        private CameraCenterOffsetController _cecterOffsetController;

        public void Init(CameraCenterOffsetController cecterOffsetController)
        {
            _cecterOffsetController = cecterOffsetController;
        }

        void Update()
        {
            Quaternion rotation = _cameraT.rotation;
            _cecterOffsetController.Update(ref _localCenterOffset, ref rotation, _playerT.position, _cameraT.right, _target.position, _lockOnState != LockOnState.Free, Time.deltaTime);

            //screenVelocityX = Mathf.Clamp01((screenVelocityX + 1f) / 2);

            //_cameraRotationZ = Mathf.Lerp(_cameraRotationZ, Mathf.Lerp(1f, -1f, screenVelocityX), Time.deltaTime);


            /*if (_lockOnState == LockOnState.Free)
            {
                rotation = Quaternion.Euler(_cameraRotateX, 0, _cameraRotationZ);
            }
            else
            {
                rotation = Quaternion.LookRotation(_target.position - _playerT.position - _localCenterOffset, Vector3.up) * Quaternion.Euler(0, 0, _cameraRotationZ);
            }*/

            Vector3 position = _playerT.position + _localCenterOffset + _cameraT.rotation * _cameraOffset;


            _cameraT.SetPositionAndRotation(position, rotation);
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
