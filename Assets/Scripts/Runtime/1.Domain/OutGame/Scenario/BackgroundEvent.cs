using System;

namespace KillChord.Runtime.Domain.OutGame.Scenario
{
    /// <summary>
    /// シナリオ中で背景切り替えを指示するイベント。
    /// </summary>
    public class BackgroundEvent : IScenarioEvent
    {
        /// <summary>
        /// 背景イベントを初期化する。
        /// </summary>
        public BackgroundEvent(string backgroundId)
        {
            BackgroundId = string.IsNullOrWhiteSpace(backgroundId)
                ? throw new ArgumentException("backgroundId is empty.", nameof(backgroundId))
                : backgroundId;
        }

        /// <summary> BackgroundId を取得する。 </summary>
        public string BackgroundId { get; }
        /// <summary> RequirePlayerAdvance を取得する。 </summary>
        public bool RequirePlayerAdvance => false;
    }
}