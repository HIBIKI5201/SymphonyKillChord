using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     ロックオン対象方向へ加算する相対ヨー・ピッチ角を計算します。
    /// </summary>
    public sealed class CameraLockOnOffsetCalculator
    {
        /// <summary>
        ///     カメラ設定を受け取り相対視点計算を初期化します。
        /// </summary>
        /// <param name="config"> カメラ設定。 </param>
        public CameraLockOnOffsetCalculator(CameraConfig config)
        {
            _config = config;
        }

        /// <summary>
        ///     入力に応じて相対角度を更新し、無操作時は遅延後に中央へ戻します。
        /// </summary>
        /// <param name="context"> 今フレームの更新コンテキスト。 </param>
        /// <returns> Xがヨー、Yがピッチの相対角度。 </returns>
        public Vector2 Update(in CameraUpdateContext context)
        {
            if (context.Input.sqrMagnitude > float.Epsilon)
            {
                float sensitivity = _config.LockOnOffsetSensitivity * context.DeltaTime;
                _offset.x += context.Input.x * sensitivity;
                _offset.y -= context.Input.y * sensitivity;
                Vector2 clamp = _config.LockOnOffsetClamp;
                _offset.x = Mathf.Clamp(_offset.x, -Mathf.Abs(clamp.x), Mathf.Abs(clamp.x));
                _offset.y = Mathf.Clamp(_offset.y, -Mathf.Abs(clamp.y), Mathf.Abs(clamp.y));
                _idleTime = 0f;
                return _offset;
            }

            _idleTime += context.DeltaTime;
            if (_idleTime >= _config.LockOnOffsetRecenterDelay)
            {
                float ratio = 1f - Mathf.Exp(-_config.LockOnOffsetRecenterSpeed * context.DeltaTime);
                _offset = Vector2.Lerp(_offset, Vector2.zero, ratio);
                if (_offset.sqrMagnitude <= OFFSET_SNAP_THRESHOLD)
                {
                    _offset = Vector2.zero;
                }
            }

            return _offset;
        }

        /// <summary>
        ///     相対角度と無操作時間を初期化します。
        /// </summary>
        public void Reset()
        {
            _offset = Vector2.zero;
            _idleTime = 0f;
        }

        private const float OFFSET_SNAP_THRESHOLD = 0.0001f;

        private readonly CameraConfig _config;
        private Vector2 _offset;
        private float _idleTime;
    }
}
