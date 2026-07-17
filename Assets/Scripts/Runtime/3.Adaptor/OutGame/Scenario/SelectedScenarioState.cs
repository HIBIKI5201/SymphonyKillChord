using System;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    ///     現在選択されているシナリオの状態を管理するクラス。
    /// </summary>
    public class SelectedScenarioState
    {
        /// <summary> 現在選択されているシナリオIDを取得します。 </summary>
        public string CurrentScenarioId
        {
            get
            {
                if (!HasSelectedScenario)
                {
                    throw new InvalidOperationException("Scenario has not been selected.");
                }

                return _currentScenarioId;
            }
        }

        /// <summary> シナリオが選択されているかどうかを取得します。 </summary>
        public bool HasSelectedScenario { get; private set; }

        /// <summary>
        ///     シナリオを選択します。
        /// </summary>
        /// <param name="scenarioId">選択するシナリオID。</param>
        public void SelectScenario(string scenarioId)
        {
            if (string.IsNullOrWhiteSpace(scenarioId))
            {
                throw new ArgumentException("Scenario id must not be null or empty.", nameof(scenarioId));
            }

            _currentScenarioId = scenarioId;
            HasSelectedScenario = true;
        }

        /// <summary>
        ///     選択されているシナリオ情報を初期化します。
        /// </summary>
        public void Clear()
        {
            _currentScenarioId = string.Empty;
            HasSelectedScenario = false;
        }

        /// <summary> 現在選択されているシナリオID。 </summary>
        private string _currentScenarioId = string.Empty;
    }
}
