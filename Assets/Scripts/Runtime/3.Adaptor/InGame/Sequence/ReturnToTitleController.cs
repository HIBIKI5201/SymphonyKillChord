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
        /// <param name="transitionLifetimeToken">
        ///     遷移の寿命。遷移はInGameシーン自身を閉じるため、InGameのオブジェクトの破棄では取り消されない常駐シーンの寿命を渡す。
        /// </param>
        public ReturnToTitleController(
            SceneTransitionUseCase usecase,
            SelectedBattleStageState selectedBattleStageState,
            SelectedMissionState selectedMissionState,
            string titleSceneName,
            CancellationToken transitionLifetimeToken = default)
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
            _transitionLifetimeToken = transitionLifetimeToken;
        }

        /// <summary>
        ///     インゲームを終了してタイトルシーンへ戻る。
        /// </summary>
        /// <param name="fallbackFromSceneName">
        ///     バトルステージが選択されていない場合に遷移元とするシーン名。
        /// </param>
        /// <returns> 遷移に成功した場合はtrue。 </returns>
        public async Task<bool> ReturnToTitleAsync(string fallbackFromSceneName)
        {
            bool success;

            if (_selectedBattleStageState.HasSelectedBattleStage)
            {
                // バトルシーン(Additive)をアンロードしてから、基盤シーンをタイトルへ遷移する。
                success = await _usecase.UnloadThenChangeSceneAsync(
                    _selectedBattleStageState.BattleSceneName,
                    _selectedBattleStageState.InGameSceneName,
                    _titleSceneName,
                    _transitionLifetimeToken);
            }
            else
            {
                // ステージ未選択(チュートリアル直起動など)の場合は現在のシーンから遷移する。
                success = await _usecase.ChangeSceneAsync(
                    fallbackFromSceneName,
                    _titleSceneName,
                    _transitionLifetimeToken);
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

        private readonly SceneTransitionUseCase _usecase;
        private readonly SelectedBattleStageState _selectedBattleStageState;
        private readonly SelectedMissionState _selectedMissionState;
        private readonly string _titleSceneName;
        private readonly CancellationToken _transitionLifetimeToken;
    }
}
