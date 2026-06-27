using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.InGame.Result
{
    public class StageResultController
    {
        public StageResultController(
            SceneTransitionUsecase usecase,
            SelectedBattleStageState selectedBattleStageState,
            SelectedMissionState selectedMissionState)
        {
            _usecase = usecase
                ?? throw new ArgumentNullException(
                    nameof(usecase));

            _selectedBattleStageState = selectedBattleStageState
                ?? throw new ArgumentNullException(
                    nameof(selectedBattleStageState));

            _selectedMissionState = selectedMissionState
                ?? throw new ArgumentNullException(
                    nameof(selectedMissionState));
        }

        /// <summary>
        ///     戦闘を完了してOutGameへ戻る。
        /// </summary>
        /// <returns>遷移に成功した場合はtrue。</returns>
        public async Task<bool> CompleteAsync()
        {
            bool success =
                await _usecase
                    .UnloadThenChangeSceneAsync(
                        _selectedBattleStageState.BattleSceneName,
                        _selectedBattleStageState.InGameSceneName,
                        _selectedBattleStageState.ReturnSceneName,
                        CancellationToken.None);

            if (!success)
            {
                return false;
            }

            _selectedBattleStageState.Clear();
            _selectedMissionState.Clear();

            return true;
        }

        /// <summary>
        ///     同じステージへ再出撃する。
        /// </summary>
        /// <returns>再出撃に成功した場合はtrue。</returns>
        public Task<bool> RetryAsync()
        {
            return _usecase
                .UnloadThenReloadSceneAsync(
                    _selectedBattleStageState.BattleSceneName,
                    _selectedBattleStageState.InGameSceneName,
                    CancellationToken.None);
        }


        private readonly SceneTransitionUsecase _usecase;
        private readonly SelectedBattleStageState _selectedBattleStageState;
        private readonly SelectedMissionState _selectedMissionState;
    }
}
