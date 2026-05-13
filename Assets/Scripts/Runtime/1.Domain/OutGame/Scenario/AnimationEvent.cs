using System;

namespace KillChord.Runtime.Domain.OutGame.Scenario
{
    /// <summary>
    /// シナリオ中でアニメーション再生を指示するイベント。
    /// </summary>
    public class AnimationEvent : IScenarioEvent
    {
        /// <summary>
        /// アニメーションイベントを初期化する。
        /// </summary>
        public AnimationEvent(string animationId)
        {
            AnimationId = string.IsNullOrWhiteSpace(animationId)
                ? throw new ArgumentException("animationId is empty.", nameof(animationId))
                : animationId;
        }

        /// <summary> AnimationId を取得する。 </summary>
        public string AnimationId { get; }
        /// <summary> RequirePlayerAdvance を取得する。 </summary>
        public bool RequirePlayerAdvance => false;
    }
}