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
        public FadeEvent(
            float start,
            float end,
            float duration,
            FadeTarget target = FadeTarget.Screen,
            FadeMode mode = FadeMode.Alpha)
        {
            if (!float.IsFinite(start) || !float.IsFinite(end) || !float.IsFinite(duration))
            {
                throw new ArgumentException("FadeEvent values must be finite.");
            }
            if (!Enum.IsDefined(typeof(FadeTarget), target))
            {
                throw new ArgumentOutOfRangeException(nameof(target), target, "FadeTarget is not defined.");
            }
            if (!Enum.IsDefined(typeof(FadeMode), mode))
            {
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "FadeMode is not defined.");
            }
            if (mode == FadeMode.Black
                && target is not FadeTarget.PortraitLeft
                && target is not FadeTarget.PortraitCenter
                && target is not FadeTarget.PortraitRight)
            {
                throw new ArgumentException("FadeMode.Black can only target a portrait.", nameof(target));
            }

            Start = Math.Clamp(start, 0f, 1f);
            End = Math.Clamp(end, 0f, 1f);
            DurationSec = Math.Max(0f, duration);
            Target = target;
            Mode = mode;
        }

        /// <summary> Start を取得する。 </summary>
        public float Start { get; }
        /// <summary> End を取得する。 </summary>
        public float End { get; }
        /// <summary> DurationSec を取得する。 </summary>
        public float DurationSec { get; }
        /// <summary> フェード対象を取得する。 </summary>
        public FadeTarget Target { get; }
        /// <summary> フェード方法を取得する。 </summary>
        public FadeMode Mode { get; }
        /// <summary> RequirePlayerAdvance を取得する。 </summary>
        public bool RequirePlayerAdvance => false;
    }
}
