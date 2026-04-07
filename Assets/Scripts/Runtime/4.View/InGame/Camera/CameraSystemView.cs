using KillChord.Runtime.Adaptor.InGame.Camera;
using KillChord.Runtime.Utility;
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

        [SerializeField] private Transform _playerT;

        [SerializeField] private CameraLockOnState _lockOnState;

        [SerializeField] private UpdateModeEnum _updateMode;

        private CameraSystemController _controller;
        private Vector2 _input;

        public void Init(CameraSystemController controller)
        {
            _controller = controller;
        }
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        private void FixedUpdate()
        {
            if (_updateMode != UpdateModeEnum.FixedUpdate)
                return;
            Tick(Time.fixedDeltaTime);
        }
        private void Update()
        {
            UpdateInput(out _input);
            if (_updateMode != UpdateModeEnum.Update)
                return;
            Tick(Time.deltaTime);
        }
        private void LateUpdate()
        {
            if (_updateMode != UpdateModeEnum.LateUpdate)
                return;
            Tick(Time.deltaTime);
        }


        private void Tick(float deltaTime)
        {
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
            if (Input.GetKeyDown(KeyCode.Mouse2))
                _controller.ToggleLockOnState();

            if (Input.GetKeyDown(KeyCode.Mouse0))
                _controller.TryActiveAutoLockOn();

            input.x = Input.GetAxisRaw("Mouse X");
            input.y = Input.GetAxisRaw("Mouse Y");
        }
    }
}
