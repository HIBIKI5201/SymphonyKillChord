using KillChord.Runtime.Adaptor.InGame.Camera;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.Utility;
using KillChord.Runtime.View.Persistent.Input;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

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

        public void Initialize(
            CameraSystemController controller,
            Transform playerT,
            PlayerInputView playerInputView)
        {
            _controller = controller;
            _playerT = playerT;
            _inputView = playerInputView;

#if UNITY_ANDROID
            _inputView.OnMobileLookInput += OnLook;
#else
            _inputView.OnLookInput += OnLook;
#endif
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

            TestChangeLockOn();
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
            _input = context.Value;
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


        private void TestChangeLockOn()
        {
#if UNITY_STANDALONE_WIN
            if (Input.GetKeyDown(KeyCode.Mouse2))
                _controller.ToggleLockOnState(_playerT.position);

            if (Input.GetKeyDown(KeyCode.Mouse0))
                _controller.TryActiveAutoLockOn(_playerT.position);
#endif
        }
    }
}