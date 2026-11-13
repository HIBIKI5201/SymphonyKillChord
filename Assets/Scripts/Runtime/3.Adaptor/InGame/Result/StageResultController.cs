using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.InGame.Result
{
    /// <summary>
    ///    ステージ結果画面の制御を行うクラス。
    /// </summary>
    public class StageResultController
    {
        /// <summary>
        ///     シーン遷移・選択中のステージとミッション・リザルト終了時の遷移方針・遷移の寿命を指定して生成する。
        ///     遷移はInGameシーン自身を閉じるため、InGameのオブジェクトの破棄では取り消されない
        ///     常駐シーンの寿命（<paramref name="transitionLifetimeToken"/>）で動かす。
        /// </summary>
        public StageResultController(
            SceneTransitionUseCase usecase,
            SelectedBattleStageState selectedBattleStageState,
            SelectedMissionState selectedMissionState,
            IStageResultExitPolicy exitPolicy = null,
            CancellationToken transitionLifetimeToken = default)
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

            _exitPolicy = exitPolicy;
            _transitionLifetimeToken = transitionLifetimeToken;
        }

        /// <summary>
        ///     戦闘を完了してOutGameへ戻る。
        /// </summary>
        /// <returns>遷移に成功した場合はtrue。</returns>
        public async Task<bool> CompleteAsync()
        {
            string destinationSceneName = ResolveDestinationScene(
                StageResultExitAction.Complete,
                _selectedBattleStageState.ReturnSceneName);
            bool success =
                await _usecase
                    .UnloadThenChangeSceneAsync(
                        _selectedBattleStageState.BattleSceneName,
                        _selectedBattleStageState.InGameSceneName,
                        destinationSceneName,
                        _transitionLifetimeToken);

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
        public async Task<bool> RetryAsync()
        {
            if (!TryResolvePolicyDestination(
                    StageResultExitAction.Retry,
                    out string destinationSceneName))
            {
                return await _usecase
                    .UnloadThenReloadSceneAsync(
                        _selectedBattleStageState.BattleSceneName,
                        _selectedBattleStageState.InGameSceneName,
                        _transitionLifetimeToken);
            }

            bool success = await _usecase.UnloadThenChangeSceneAsync(
                _selectedBattleStageState.BattleSceneName,
                _selectedBattleStageState.InGameSceneName,
                destinationSceneName,
                _transitionLifetimeToken);
            if (success)
            {
                _selectedBattleStageState.Clear();
                _selectedMissionState.Clear();
            }

            return success;
        }

        /// <summary>
        ///     リザルト終了ポリシーを適用した遷移先を取得します。
        /// </summary>
        private string ResolveDestinationScene(
            StageResultExitAction action,
            string defaultSceneName)
        {
            return TryResolvePolicyDestination(
                action,
                out string destinationSceneName)
                ? destinationSceneName
                : defaultSceneName;
        }

        /// <summary>
        ///     未登録または空のポリシー応答を通常遷移として扱います。
        /// </summary>
        private bool TryResolvePolicyDestination(
            StageResultExitAction action,
            out string destinationSceneName)
        {
            destinationSceneName = string.Empty;
            return _exitPolicy != null
                && _exitPolicy.TryGetDestinationScene(
                    action,
                    _selectedBattleStageState,
                    out destinationSceneName)
                && !string.IsNullOrWhiteSpace(destinationSceneName);
        }


        private readonly SceneTransitionUseCase _usecase;
        private readonly SelectedBattleStageState _selectedBattleStageState;
        private readonly SelectedMissionState _selectedMissionState;
        private readonly IStageResultExitPolicy _exitPolicy;
        private readonly CancellationToken _transitionLifetimeToken;
    }
}
