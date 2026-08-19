using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using SymphonyFrameWork.System.ServiceLocate;
using System;

namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     バトル出撃用の選択状態を構築します。
    /// </summary>
    public sealed class BattleSortieSelectionService
    {
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

            SelectedBattleStageState selectedBattleStageState = ResolveSelectedBattleStageState();
            SelectedMissionState selectedMissionState = ResolveSelectedMissionState();
            selectedBattleStageState.SelectBattleStage(stageDefinition, returnSceneName);
            new OutGameMissionSelectController(selectedMissionState).Select(stageDefinition.MissionId);
            return true;
        }

        /// <summary>
        ///     バトルステージ選択状態を解決します。
        /// </summary>
        /// <returns> 解決した状態です。 </returns>
        private static SelectedBattleStageState ResolveSelectedBattleStageState()
        {
            if (ServiceLocator.TryGetInstance(out SelectedBattleStageState selectedBattleStageState))
            {
                return selectedBattleStageState;
            }

            selectedBattleStageState = new SelectedBattleStageState();
            ServiceLocator.RegisterInstance(selectedBattleStageState);
            return selectedBattleStageState;
        }

        /// <summary>
        ///     ミッション選択状態を解決します。
        /// </summary>
        /// <returns> 解決した状態です。 </returns>
        private static SelectedMissionState ResolveSelectedMissionState()
        {
            if (ServiceLocator.TryGetInstance(out SelectedMissionState selectedMissionState))
            {
                return selectedMissionState;
            }

            selectedMissionState = new SelectedMissionState();
            ServiceLocator.RegisterInstance(selectedMissionState);
            return selectedMissionState;
        }
    }
}
