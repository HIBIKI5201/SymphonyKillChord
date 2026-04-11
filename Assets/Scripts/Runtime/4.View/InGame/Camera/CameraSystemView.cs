using System;
using KillChord.Runtime.Adaptor.InGame.Camera;
using KillChord.Runtime.Utility;
using SymphonyFrameWork.Debugger.HUD;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     カメラシステムの挙動を管理するViewクラス。
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConst.CAMERA_FOLLOW)]
    public sealed class CameraSystemView : MonoBehaviour
    {
        [SerializeField] private Transform _cameraT;
        [SerializeField] private UpdateModeEnum _updateMode;

        private CameraSystemController _controller;
        private Transform _playerT;
        private Vector2 _input;

        public void Init(CameraSystemController controller, Transform playerT)
        {
            _controller = controller;
            _playerT = playerT;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            SymphonyDebugHUD.AddText(CheckNullC);
            SymphonyDebugHUD.AddText(CheckNullPT);
        }

        private string CheckNullC()
        {
            return (_controller == null).ToString();
        }

        private string CheckNullPT()
        {
            return (_playerT == null).ToString();
        }

        private void OnDestroy()
        {
            SymphonyDebugHUD.RemoveText(CheckNullC);
            SymphonyDebugHUD.RemoveText(CheckNullPT);
        }

        private void FixedUpdate()
        {
            if (_updateMode != UpdateModeEnum.FixedUpdate)
                return;
            Tick(Time.fixedDeltaTime);
        }

        private void Update()
        {
            if (_controller == null || _playerT == null) return;

            UpdateInput(out _input);
            if (_updateMode != UpdateModeEnum.Update)
                return;
            Tick(Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (_controller == null || _playerT == null) return;

            if (_updateMode != UpdateModeEnum.LateUpdate)
                return;
            Tick(Time.deltaTime);
        }


        private void Tick(float deltaTime)
        {
            if (_controller == null || _playerT == null) return;
            Vector2 input = _input * 200;
            _input = Vector2.zero;

            _controller.Update(
                _playerT.position,
                input,
                deltaTime,
                out Quaternion rotation,
                out Vector3 position
            );
            _cameraT.SetPositionAndRotation(position, rotation);
        }

        private void UpdateInput(out Vector2 input)
        {
            input = Vector2.zero;

            if (Input.GetKeyDown(KeyCode.Mouse2))
                _controller.ToggleLockOnState(_playerT.position);

            if (Input.GetKeyDown(KeyCode.Mouse0))
                _controller.TryActiveAutoLockOn(_playerT.position);

            input.x = Input.GetAxisRaw("Mouse X");
            input.y = Input.GetAxisRaw("Mouse Y");
        }
    }
}