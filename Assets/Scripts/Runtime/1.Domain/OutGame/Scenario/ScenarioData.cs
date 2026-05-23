using System.Collections.Generic;
using System;


namespace KillChord.Runtime.Domain.OutGame.Scenario
{
    /// <summary>
    /// シナリオ再生に使用するイベント列を保持する。
    /// </summary>
    public class ScenarioData
    {

        /// <summary>
        /// シナリオ再生に使うイベント列を初期化する。
        /// </summary>
        public ScenarioData(IReadOnlyList<IScenarioEvent> events)
        {
            Events = events ?? throw new ArgumentNullException(nameof(events));
        }
        /// <summary> Events を取得する。 </summary>
        public IReadOnlyList<IScenarioEvent> Events { get; }
    }
}