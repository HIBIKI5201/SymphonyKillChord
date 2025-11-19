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

        private ILockOnTargetContainer _targetContainer;

        private void HandleLookAction(Vector2 value)
        {

        }

        private void HandleLockOnSelectAction(float value)
        {

        }
    }
}
