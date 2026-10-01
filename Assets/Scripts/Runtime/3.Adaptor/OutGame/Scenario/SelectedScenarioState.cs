using KillChord.Runtime.Domain.OutGame.StageSelect;
using System;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    ///     現在選択されているシナリオの状態を管理するクラス。
    /// </summary>
    public class SelectedScenarioState
    {
        /// <summary> 選択中のシナリオが変わったとき、または選択が解除されたときに通知します。 </summary>
        public event Action OnSelectionChanged;

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

        /// <summary> 選択状態が更新された回数を取得します。 </summary>
        public int SelectionRevision => _selectionRevision;

        /// <summary> タイトルから開始したオープニングチュートリアルの場合はtrueです。 </summary>
        public bool IsOpeningTutorialScenario { get; private set; }

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

            ApplySelection(stageDefinition, false);
        }

        /// <summary>
        ///     タイトルから開始するオープニングチュートリアルシナリオを選択します。
        /// </summary>
        /// <param name="stageDefinition"> 選択するシナリオステージ定義。 </param>
        public void SelectOpeningTutorialScenario(ScenarioStageDefinition stageDefinition)
        {
            if (stageDefinition == null)
            {
                throw new ArgumentNullException(nameof(stageDefinition));
            }

            ApplySelection(stageDefinition, true);
        }

        /// <summary>
        ///     選択されているシナリオ情報を初期化します。
        /// </summary>
        public void Clear()
        {
            ApplySelection(null, false);
        }

        /// <summary>
        ///     取得時から選択状態が更新されていない場合だけ初期化します。
        /// </summary>
        /// <param name="expectedRevision"> 取得済みの選択状態改訂番号。</param>
        /// <returns> 選択状態を初期化した場合はtrue。</returns>
        public bool TryClear(int expectedRevision)
        {
            if (_selectionRevision != expectedRevision)
            {
                return false;
            }

            Clear();
            return true;
        }

        private ScenarioStageDefinition _currentStageDefinition;
        private int _selectionRevision;

        /// <summary>
        ///     選択状態を差し替えて変更を通知します。
        /// </summary>
        /// <param name="stageDefinition"> 選択するシナリオステージ定義。解除する場合はnull。</param>
        /// <param name="isOpeningTutorialScenario"> オープニングチュートリアルのシナリオの場合はtrue。</param>
        private void ApplySelection(ScenarioStageDefinition stageDefinition, bool isOpeningTutorialScenario)
        {
            _currentStageDefinition = stageDefinition;
            IsOpeningTutorialScenario = isOpeningTutorialScenario;
            _selectionRevision++;
            OnSelectionChanged?.Invoke();
        }
    }
}
