using UnityEngine;

namespace Mock.MusicBattle.Camera
{
    public class CameraManager : MonoBehaviour
    {
        /// <summary>
        ///     カメラを初期化する。
        /// </summary>
        /// <returns> 成功したかどうか </returns>
        public bool Init(
            IInputBuffer inputBuffer,
            ILockOnTargetContainer lockOnTargetContainer)
        {
            if (inputBuffer == null) { return false; }
            if (lockOnTargetContainer == null) { return false; }

            inputBuffer.LookAction += HandleLookAction;
            inputBuffer.LockOnSelectAction += HandleLockOnSelectAction;

            _targetContainer = lockOnTargetContainer;

            return true;
        }

        public void ChangeUpdateMode(CameraUpdateModeEnum mode)
        {
            _mode = mode;
        }

        private ILockOnTargetContainer _targetContainer;
        private IInputBuffer _inputBuffer;

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

        private void HandleLookAction(Vector2 value)
        {

        }

        private void HandleLockOnSelectAction(float value)
        {

        }
    }
}
