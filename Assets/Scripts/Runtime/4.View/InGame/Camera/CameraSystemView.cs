using System;
using KillChord.Runtime.Adaptor.InGame.Camera;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.Utility;
using KillChord.Runtime.View.Persistent.Input;
using SymphonyFrameWork.Debugger.HUD;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;
using UnityEngine.InputSystem;

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
        private PlayerInputView _inputView;
        private Transform _playerT;
        private Vector2 _input;

        public void Init(CameraSystemController controller, Transform playerT, PlayerInputView playerInputView)
        {
            _controller = controller;
            _playerT = playerT;
            _inputView = playerInputView;

            _inputView.OnLookInput += OnLook;
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

        private void OnLook(InputContext<Vector2> context)
        {
        }

        private void OnNextLockOn(InputContext<float> context)
        {
            if (context.Phase == InputActionPhase.Started)
            {
                _controller.ToggleLockOnState(_playerT.position);
            }
        }

        private void OnPreviousLockOn(InputContext<float> context)
        {
            if (context.Phase == InputActionPhase.Started)
            {
                _controller.TryActiveAutoLockOn(_playerT.position);
            }
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