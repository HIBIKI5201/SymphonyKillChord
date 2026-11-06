using System;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     ジャスト成立時のスローモーションの長さと倍率、遅れを取り戻す時間の設定。
    /// </summary>
    [Serializable]
    public sealed class JustSlowMotionSettings
    {
        /// <summary> スローモーションをかける実時間（秒）です。 </summary>
        public float SlowDurationSeconds => Mathf.Max(0f, _slowDurationSeconds);

        /// <summary> スローモーション中のアニメーション再生倍率です。 </summary>
        public float SlowPlaybackScale => Mathf.Clamp01(_slowPlaybackScale);

        /// <summary> スローモーションの後、遅れを取り戻すのにかける実時間（秒）です。 </summary>
        public float CatchUpDurationSeconds => Mathf.Max(MIN_CATCH_UP_DURATION_SECONDS, _catchUpDurationSeconds);

        private const float MIN_CATCH_UP_DURATION_SECONDS = 0.01f;

        [SerializeField, Tooltip("スローモーションをかける実時間（秒）。")]
        private float _slowDurationSeconds = 0.3f;

        [SerializeField, Tooltip("スローモーション中のアニメーション再生倍率。1で等速。")]
        private float _slowPlaybackScale = 0.5f;

        [SerializeField, Tooltip("スローモーションの後、アニメーションを速めて遅れを取り戻す実時間（秒）。短いほど最初が速くなる。")]
        private float _catchUpDurationSeconds = 0.45f;
    }
}
