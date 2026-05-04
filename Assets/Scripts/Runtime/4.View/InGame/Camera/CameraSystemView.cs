using KillChord.Runtime.Adaptor.InGame.Camera;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.Utility;
using KillChord.Runtime.View.Persistent.Input;
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
        public void Initialize(
            CameraSystemController controller,
            CameraSystemPresenter presenter,
            Transform playerT,
            PlayerInputView playerInputView)
        {
            _controller = controller;
            _presenter = presenter;
            _playerT = playerT;
            _inputView = playerInputView;

#if UNITY_ANDROID
            _inputView.OnMobileLookInput += OnLook;
#else
            _inputView.OnLookInput += OnLook;
#endif
            _inputView.OnMoveInput += OnMove;
            _inputView.OnLockOnInput += OnLockOn;
            _inputView.OnAttackInput += OnAttack;
        }

        [SerializeField] private Transform _cameraT;
        [SerializeField] private UpdateModeEnum _updateMode;
        [SerializeField] private int _cameraSensitivity = 5;

        private CameraSystemController _controller;
        private CameraSystemPresenter _presenter;
        private PlayerInputView _inputView;
        private Transform _playerT;
        private Vector2 _input;
        private Vector2 _moveInput;

        private void FixedUpdate()
        {
            if (_updateMode != UpdateModeEnum.FixedUpdate) { return; }
            Tick(Time.fixedDeltaTime);
        }

        private void Update()
        {
            if (_controller == null || _playerT == null) { return; }

            if (_updateMode != UpdateModeEnum.Update)
            { return; }
            Tick(Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (_controller == null || _playerT == null) { return; }

            if (_updateMode != UpdateModeEnum.LateUpdate)
            { return; }
            Tick(Time.deltaTime);
        }

        private void OnLook(InputContext<Vector2> context)
        {
            _input = context.Value;
        }

        /// <summary>
        ///     移動入力を受け取り、入力値を更新する。
        /// </summary>
        /// <param name="context"></param>
        private void OnMove(InputContext<Vector2> context)
        {
            _moveInput = context.Value;
        }

        /// <summary>
        ///     ロックオン入力を受け取り、マニュアルロックオン状態をトグルする。
        /// </summary>
        /// /// <param name="context"></param>
        private void OnLockOn(InputContext<float> context)
        {
            if (context.Phase == InputActionPhase.Started)
            {
                _controller.ToggleLockOnState(_playerT.position);
            }
        }

        /// <summary>
        ///     攻撃入力を受け取り、オートロックオンの発動を試みる。
        /// </summary>
        /// <param name="context"></param>
        private void OnAttack(InputContext<float> context)
        {
            if (context.Phase == InputActionPhase.Started)
            {
                _controller.TryActiveAutoLockOn(_playerT.position);
            }
        }

        private void Tick(float deltaTime)
        {
            if (_controller == null || _playerT == null) { return; }
            Vector2 input = _input * _cameraSensitivity;
            _input = Vector2.zero;

            _presenter.Update(
                _playerT.position,
                input,
                _moveInput,
                deltaTime,
                out Quaternion rotation,
                out Vector3 position
            );
            _cameraT.SetPositionAndRotation(position, rotation);
        }
    }
}