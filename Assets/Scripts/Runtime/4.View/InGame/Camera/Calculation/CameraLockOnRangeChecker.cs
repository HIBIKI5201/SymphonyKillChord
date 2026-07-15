using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     ロックオン対象がカメラの有効ビューポート内に存在するか判定します。
    /// </summary>
    public sealed class CameraLockOnRangeChecker
    {
        /// <summary>
        ///     カメラ設定を受け取り判定器を初期化します。
        /// </summary>
        /// <param name="config"> カメラ設定。 </param>
        public CameraLockOnRangeChecker(CameraConfig config)
        {
            _config = config;
        }

        /// <summary>
        ///     対象位置がマージンを考慮したビューポート内に存在するか判定します。
        /// </summary>
        /// <param name="camera"> 判定に使用するカメラ。 </param>
        /// <param name="targetPosition"> 対象のワールド座標。 </param>
        /// <returns> カメラ前方かつ有効ビューポート内の場合はtrue。 </returns>
        public bool IsWithinRange(UnityEngine.Camera camera, in Vector3 targetPosition)
        {
            if (camera == null)
            {
                return false;
            }

            Vector3 viewportPosition = camera.WorldToViewportPoint(targetPosition);
            float margin = Mathf.Clamp(_config.LockOnViewportMargin, 0f, 0.5f);
            return viewportPosition.z > 0f
                && viewportPosition.x >= margin
                && viewportPosition.x <= 1f - margin
                && viewportPosition.y >= margin
                && viewportPosition.y <= 1f - margin;
        }

        private readonly CameraConfig _config;
    }
}
