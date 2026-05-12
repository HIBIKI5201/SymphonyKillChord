using System;

namespace KillChord.Runtime.Domain.OutGame.Scenario
{
    /// <summary>
    /// シナリオ中でフェード演出を指示するイベント。
    /// </summary>
    public class FadeEvent : IScenarioEvent
    {
        /// <summary>
        /// フェードイベントを初期化する。
        /// </summary>
        public FadeEvent(float start, float end, float duration)
        {
            if (float.IsNaN(start) || float.IsNaN(end) || float.IsNaN(duration))
                throw new ArgumentException("FadeEvent values must not be NaN.");

            Start = Math.Clamp(start, 0f, 1f);
            End = Math.Clamp(end, 0f, 1f);
            DurationSec = Math.Max(0f, duration);
        }

        /// <summary> Start を取得する。 </summary>
        public float Start { get; }
        /// <summary> End を取得する。 </summary>
        public float End { get; }
        /// <summary> DurationSec を取得する。 </summary>
        public float DurationSec { get; }
        /// <summary> RequirePlayerAdvance を取得する。 </summary>
        public bool RequirePlayerAdvance => false;
    }
}