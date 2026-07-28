using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.InGame.Sequence
{
    /// <summary>
    ///     インゲームからタイトルシーンへ復帰する制御を行うクラス。
    /// </summary>
    public class ReturnToTitleController
    {
        /// <summary>
        ///     必要な依存関係を指定して生成する。
        /// </summary>
        /// <param name="usecase"> シーン遷移Usecase。 </param>
        /// <param name="selectedBattleStageState"> 選択中バトルステージの状態。 </param>
        /// <param name="selectedMissionState"> 選択中ミッションの状態。省略可。 </param>
        /// <param name="titleSceneName"> 遷移先となるタイトルシーン名。 </param>
        public ReturnToTitleController(
            SceneTransitionUsecase usecase,
            SelectedBattleStageState selectedBattleStageState,
            SelectedMissionState selectedMissionState,
            string titleSceneName)
        {
            _usecase = usecase
                ?? throw new ArgumentNullException(nameof(usecase));

            _selectedBattleStageState = selectedBattleStageState
                ?? throw new ArgumentNullException(nameof(selectedBattleStageState));

            _selectedMissionState = selectedMissionState;

            if (string.IsNullOrWhiteSpace(titleSceneName))
            {
                throw new ArgumentException("タイトルシーン名が設定されていません。", nameof(titleSceneName));
            }

            _titleSceneName = titleSceneName;
        }

        /// <summary>
        ///     インゲームを終了してタイトルシーンへ戻る。
        /// </summary>
        /// <param name="fallbackFromSceneName">
        ///     バトルステージが選択されていない場合に遷移元とするシーン名。
        /// </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 遷移に成功した場合はtrue。 </returns>
        public async Task<bool> ReturnToTitleAsync(
            string fallbackFromSceneName,
            CancellationToken cancellationToken)
        {
            bool success;

            if (_selectedBattleStageState.HasSelectedBattleStage)
            {
                // バトルシーン(Additive)をアンロードしてから、基盤シーンをタイトルへ遷移する。
                success = await _usecase.UnloadThenChangeSceneAsync(
                    _selectedBattleStageState.BattleSceneName,
                    _selectedBattleStageState.InGameSceneName,
                    _titleSceneName,
                    cancellationToken);
            }
            else
            {
                // ステージ未選択(チュートリアル直起動など)の場合は現在のシーンから遷移する。
                success = await _usecase.ChangeSceneAsync(
                    fallbackFromSceneName,
                    _titleSceneName,
                    cancellationToken);
            }

            if (!success)
            {
                return false;
            }

            if (_selectedBattleStageState.HasSelectedBattleStage)
            {
                _selectedBattleStageState.Clear();
            }

            _selectedMissionState?.Clear();

            return true;
        }

        private readonly SceneTransitionUsecase _usecase;
        private readonly SelectedBattleStageState _selectedBattleStageState;
        private readonly SelectedMissionState _selectedMissionState;
        private readonly string _titleSceneName;
    }
}
