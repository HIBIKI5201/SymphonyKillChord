using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     ワンショットアニメーションの時間進行とブレンド時間を計算する。
    /// </summary>
    public sealed class CharacterAnimationOneShotTimingCalculator
    {
        /// <summary>
        ///     クリップ長から開始ブレンド時間を取得する。
        /// </summary>
        /// <param name="clipLength"> クリップ長です。 </param>
        /// <param name="frameCount"> ブレンドフレーム数です。 </param>
        /// <returns> 開始ブレンド時間です。 </returns>
        public float GetEnterBlendDurationSeconds(float clipLength, int frameCount)
        {
            return GetBlendDurationSeconds(clipLength, frameCount, DEFAULT_ENTER_BLEND_FRAME_COUNT);
        }

        /// <summary>
        ///     クリップ長から終了ブレンド時間を取得する。
        /// </summary>
        /// <param name="clipLength"> クリップ長です。 </param>
        /// <param name="frameCount"> ブレンドフレーム数です。 </param>
        /// <returns> 終了ブレンド時間です。 </returns>
        public float GetExitBlendDurationSeconds(float clipLength, int frameCount)
        {
            return GetBlendDurationSeconds(clipLength, frameCount, DEFAULT_EXIT_BLEND_FRAME_COUNT);
        }

        /// <summary>
        ///     現在のBPM再生速度を加味した実時間を取得する。
        /// </summary>
        /// <param name="clipLength"> クリップ長です。 </param>
        /// <param name="animationSpeed"> 現在の再生速度です。 </param>
        /// <returns> 実時間です。 </returns>
        public float GetPlaybackDurationSeconds(float clipLength, float animationSpeed)
        {
            return Mathf.Max(0f, clipLength) / Mathf.Max(MIN_SPEED, animationSpeed);
        }

        /// <summary>
        ///     1フレーム分の進行量を基準時間で取得する。
        /// </summary>
        /// <param name="deltaTime"> 経過実時間です。 </param>
        /// <param name="animationSpeed"> 現在の再生速度です。 </param>
        /// <returns> 基準時間上の進行量です。 </returns>
        public float GetBaseProgressDelta(float deltaTime, float animationSpeed)
        {
            return Mathf.Max(0f, deltaTime) * Mathf.Max(MIN_SPEED, animationSpeed);
        }

        /// <summary>
        ///     フレーム数を基準秒へ変換し、クリップ長に収まるよう補正する。
        /// </summary>
        /// <param name="clipLength"> クリップ長です。 </param>
        /// <param name="frameCount"> 指定フレーム数です。 </param>
        /// <param name="defaultFrameCount"> 既定フレーム数です。 </param>
        /// <returns> 基準秒です。 </returns>
        private float GetBlendDurationSeconds(float clipLength, int frameCount, int defaultFrameCount)
        {
            int resolvedFrameCount = frameCount > 0
                ? frameCount
                : defaultFrameCount;

            return Mathf.Min(
                Mathf.Max(0f, clipLength),
                resolvedFrameCount / REFERENCE_FPS);
        }

        private const float REFERENCE_FPS = 30f;
        private const int DEFAULT_ENTER_BLEND_FRAME_COUNT = 4;
        private const int DEFAULT_EXIT_BLEND_FRAME_COUNT = 8;
        private const float MIN_SPEED = 0.0001f;
    }
}
