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

        private Quaternion _cameraBoneRotation;
        private Quaternion _cameraRotation;
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
        private void Start()
        {
            _cameraBoneRotation = _cameraT.rotation;
            _cameraRotation = _cameraT.rotation;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                if (_lockOnState == LockOnState.Free)
                    _lockOnState = LockOnState.LockOnManual;
                else
                    _lockOnState = LockOnState.Free;

            _cecterOffsetController.Update(
                ref _localCenterOffset,
                ref _cameraBoneRotation,
                ref _cameraRotation,
                _playerT.position,
                _target.position,
                _cameraT.position,
                _lockOnState != LockOnState.Free,
                Time.deltaTime);

            //screenVelocityX = Mathf.Clamp01((screenVelocityX + 1f) / 2);

            //_cameraRotationZ = Mathf.Lerp(_cameraRotationZ, Mathf.Lerp(1f, -1f, screenVelocityX), Time.deltaTime);

            Vector3 position = _playerT.position + _localCenterOffset + _cameraBoneRotation * _cameraOffset;


            _cameraT.SetPositionAndRotation(position, _cameraBoneRotation * _cameraRotation);
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
