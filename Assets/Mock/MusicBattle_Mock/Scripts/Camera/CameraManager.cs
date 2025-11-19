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


            return true;
        }
    }
}
