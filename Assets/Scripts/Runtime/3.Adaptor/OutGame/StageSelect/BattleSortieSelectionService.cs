using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using System;

namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     バトル出撃用の選択状態を構築します。
    /// </summary>
    public sealed class BattleSortieSelectionService
    {
        /// <summary>
        ///     選択状態の取得方法を指定して生成します。
        ///     選択状態はシーン間で共有されるため、出撃準備のたびに取得します。
        /// </summary>
        /// <param name="selectedBattleStageStateResolver"> バトルステージ選択状態を返す処理です。 </param>
        /// <param name="selectedMissionStateResolver"> ミッション選択状態を返す処理です。 </param>
        public BattleSortieSelectionService(
            Func<SelectedBattleStageState> selectedBattleStageStateResolver,
            Func<SelectedMissionState> selectedMissionStateResolver)
        {
            _selectedBattleStageStateResolver = selectedBattleStageStateResolver
                ?? throw new ArgumentNullException(nameof(selectedBattleStageStateResolver));
            _selectedMissionStateResolver = selectedMissionStateResolver
                ?? throw new ArgumentNullException(nameof(selectedMissionStateResolver));
        }

        /// <summary>
        ///     バトル出撃用の選択状態を構築します。
        /// </summary>
        /// <param name="stageDefinition"> 出撃対象のステージ定義です。 </param>
        /// <param name="returnSceneName"> 戦闘終了後の帰還先シーン名です。 </param>
        /// <returns> 準備に成功した場合はtrueです。 </returns>
        public bool TryPrepareBattleSortie(
            BattleStageDefinition stageDefinition,
            string returnSceneName)
        {
            if (stageDefinition == null
                || stageDefinition.MissionId.Value == 0
                || string.IsNullOrWhiteSpace(stageDefinition.BattleSceneName)
                || string.IsNullOrWhiteSpace(returnSceneName))
            {
                return false;
            }

            SelectedBattleStageState selectedBattleStageState = _selectedBattleStageStateResolver();
            SelectedMissionState selectedMissionState = _selectedMissionStateResolver();
            selectedBattleStageState.SelectBattleStage(stageDefinition, returnSceneName);
            new OutGameMissionSelectController(selectedMissionState).Select(stageDefinition.MissionId);
            return true;
        }

        private readonly Func<SelectedBattleStageState> _selectedBattleStageStateResolver;
        private readonly Func<SelectedMissionState> _selectedMissionStateResolver;
    }
}
