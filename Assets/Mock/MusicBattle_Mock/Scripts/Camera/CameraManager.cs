using Mock.MusicBattle.Basis;
using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Mock.MusicBattle.Camera
{
    /// <summary>
    ///     カメラのマネージャークラス。
    ///     カメラの各モジュールを実行する。
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraManager : MonoBehaviour, IDisposable
    {
        /// <summary>
        ///     カメラを初期化する。
        /// </summary>
        /// <returns> 成功したかどうか </returns>
        public bool Init(
            InputBuffer inputBuffer,
            ILockOnTargetContainer lockOnTargetContainer)
        {
            if (inputBuffer == null) { return false; }
            if (lockOnTargetContainer == null) { return false; }

            // イベント登録。
            inputBuffer.LookAction.Performed += HandleLookAction;
            inputBuffer.LookAction.Canceled += HandleLookAction;

            inputBuffer.LockOnSelectAction.Started += HandleLockOnSelectAction;

            CinemachineCamera cam = GetComponent<CinemachineCamera>();
            if (_cameraConfigs == null) { return false; }

            _mover = new(_cameraConfigs, transform, cam.Follow);

            _camera = cam;
            _targetContainer = lockOnTargetContainer;

            return true;
        }

        /// <summary>
        ///     アップデートモードを変更する。
        /// </summary>
        /// <param name="mode"></param>
        public void ChangeUpdateMode(CameraUpdateModeEnum mode)
        {
            _mode = mode;
        }

        /// <summary>
        ///     イベント登録解除をする。
        /// </summary>
        public void Dispose()
        {
            if (_inputBuffer != null)
            {
                _inputBuffer.LookAction.Performed -= HandleLookAction;
                _inputBuffer.LookAction.Canceled -= HandleLookAction;

                _inputBuffer.LockOnSelectAction.Started -= HandleLockOnSelectAction;
            }
        }

        [SerializeField]
        private CameraConfigs _cameraConfigs;

        private ILockOnTargetContainer _targetContainer;
        private InputBuffer _inputBuffer;
        private CinemachineCamera _camera;

        private CameraMover _mover;

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
            _mover?.OnDrawGizmos();
        }

        /// <summary>
        ///     1フレームごとの更新を行う。
        /// </summary>
        /// <param name="deltaTime"> デルタタイム </param>
        private void Tick(float deltaTime)
        {
            // 移動モジュールを更新。
            _mover?.UpdatePitch(deltaTime);
            _mover?.UpdateYaw(deltaTime);
        }

        /// <summary>
        ///     Lookアクションの入力を受ける。
        /// </summary>
        /// <param name="value"></param>
        private void HandleLookAction(Vector2 value)
        {

        }

        /// <summary>
        ///     LockOnSelectアクションの入力を受ける。
        /// </summary>
        /// <param name="value"></param>
        private void HandleLockOnSelectAction(float value)
        {

        }
    }
}
