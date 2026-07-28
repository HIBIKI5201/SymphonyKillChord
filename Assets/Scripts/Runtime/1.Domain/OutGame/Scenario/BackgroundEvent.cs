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
        public BackgroundEvent(BackgroundId backgroundId)
        {
            BackgroundId = backgroundId;
        }

        /// <summary> BackgroundId を取得する。 </summary>
        public BackgroundId BackgroundId { get; }
        /// <summary> RequirePlayerAdvance を取得する。 </summary>
        public bool RequirePlayerAdvance => false;
    }
}
