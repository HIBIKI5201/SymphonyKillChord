using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Camera
{
    /// <summary>
    ///     追従対象の速度を計算する構造体。
    /// </summary>
    public struct CameraFollowVelocityApplication
    {
        /// <summary>
        ///     前フレームの位置との差分から追従対象の速度を計算して返す。
        ///     未初期化時や deltaTime が0以下の場合は Vector3.zero を返す。
        /// </summary>
        /// <param name="currentFollowPosition"> 今フレームの追従対象のワールド座標。</param>
        /// <param name="deltaTime"> 前フレームからの経過時間。</param>
        /// <returns>追従対象の速度。</returns>
        public Vector3 UpdateFollowVelocity(in Vector3 currentFollowPosition, float deltaTime)
        {
            if (!_isInitialized || deltaTime <= 0)
            {
                _previousFollowPosition = currentFollowPosition;
                _isInitialized = true;
                return Vector3.zero;
            }

            Vector3 velocity = currentFollowPosition - _previousFollowPosition;
            _previousFollowPosition = currentFollowPosition;
            return velocity / deltaTime;
        }

        private Vector3 _previousFollowPosition;
        private bool _isInitialized;
    }
}
