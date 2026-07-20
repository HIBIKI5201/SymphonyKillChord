using KillChord.Runtime.Domain.OutGame.StageSelect;
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

                return _currentStageDefinition.ScenarioId;
            }
        }

        /// <summary> 現在選択されているシナリオステージ定義を取得します。 </summary>
        public ScenarioStageDefinition CurrentStageDefinition
        {
            get
            {
                if (!HasSelectedScenario)
                {
                    throw new InvalidOperationException("Scenario has not been selected.");
                }

                return _currentStageDefinition;
            }
        }

        /// <summary> シナリオが選択されているかどうかを取得します。 </summary>
        public bool HasSelectedScenario => _currentStageDefinition != null;

        /// <summary>
        ///     シナリオを選択します。
        /// </summary>
        /// <param name="stageDefinition"> 選択するシナリオステージ定義。</param>
        public void SelectScenario(ScenarioStageDefinition stageDefinition)
        {
            if (stageDefinition == null)
            {
                throw new ArgumentNullException(nameof(stageDefinition));
            }

            _currentStageDefinition = stageDefinition;
        }

        /// <summary>
        ///     選択されているシナリオ情報を初期化します。
        /// </summary>
        public void Clear()
        {
            _currentStageDefinition = null;
        }

        private ScenarioStageDefinition _currentStageDefinition;
    }
}
