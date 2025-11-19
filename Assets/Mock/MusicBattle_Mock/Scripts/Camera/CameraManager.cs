using Mock.MusicBattle.Basis;
using System;
using UnityEngine;

namespace Mock.MusicBattle.Camera
{
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

        private ILockOnTargetContainer _targetContainer;
        private InputBuffer _inputBuffer;

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

        /// <summary>
        ///     1フレームごとの更新を行う。
        /// </summary>
        /// <param name="deltaTime"> デルタタイム </param>
        private void Tick(float deltaTime)
        {

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
