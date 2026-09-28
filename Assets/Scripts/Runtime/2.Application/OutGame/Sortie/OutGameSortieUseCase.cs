using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.Sortie
{
    /// <summary>
    ///     アウトゲームから各ステージへ出撃する流れを制御するユースケース。
    /// </summary>
    public sealed class OutGameSortieUseCase
    {
        /// <summary>
        ///     ユースケースを初期化する。
        /// </summary>
        /// <param name="sceneTransitionUsecase"> シーン遷移ユースケース。 </param>
        /// <param name="outputPort"> 出力ポート。 </param>
        /// <param name="scenarioTransitionCancellationToken"> OutGameシーンの終了時にキャンセルされるトークン。 </param>
        public OutGameSortieUseCase(
            SceneTransitionUseCase sceneTransitionUsecase,
            IOutGameSortieOutputPort outputPort,
            CancellationToken scenarioTransitionCancellationToken)
        {
            _usecase = sceneTransitionUsecase;
            _outputPort = outputPort;
            _scenarioTransitionCancellationToken = scenarioTransitionCancellationToken;
        }

        /// <summary>
        ///    ステージタイプに応じた処理を実行する。
        ///    バトルステージの場合は戦闘準備画面を挟まずバトルを開始する。
        ///    シナリオステージの場合はシーン遷移を行う。
        /// </summary>
        /// <param name="stageType"> ステージの種類。 </param>
        /// <param name="fromSceneName"> 現在のシーン名。 </param>
        /// <param name="targetSceneName"> 遷移先のシーン名。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 出撃が成功したかどうか。 </returns>
        public async Task<bool> RequestSortieAsync(
            StageType stageType,
            string fromSceneName,
            string targetSceneName,
            CancellationToken cancellationToken)
        {
            if (stageType == StageType.Battle)
            {
                _outputPort.StartBattle();
                return true;
            }

            _outputPort.SetOutGameActiveForScenario(false);

            try
            {
                bool success = await _usecase.LoadAdditiveAsync(
                    targetSceneName,
                    cancellationToken);

                if (!success && !cancellationToken.IsCancellationRequested
                    && !_scenarioTransitionCancellationToken.IsCancellationRequested)
                {
                    _outputPort.SetOutGameActiveForScenario(true);
                }

                return success;
            }
            catch
            {
                // シーン終了によるキャンセル後は、常駐入力や旧UIを復元しません。
                if (!cancellationToken.IsCancellationRequested
                    && !_scenarioTransitionCancellationToken.IsCancellationRequested)
                {
                    _outputPort.SetOutGameActiveForScenario(true);
                }
                throw;
            }
        }

        /// <summary>
        ///     戦闘準備画面を介さずにバトル開始を要求します。
        /// </summary>
        /// <returns> 要求を受理した場合はtrueです。 </returns>
        public bool RequestImmediateBattleSortie()
        {
            _outputPort.StartBattle();
            return true;
        }

        /// <summary>
        ///     シナリオシーンを終了してOutGameのホーム画面へ復帰します。
        /// </summary>
        /// <param name="scenarioSceneName"> 終了するシナリオシーン名。 </param>
        /// <param name="returnSceneName"> 復帰先のOutGameシーン名。 </param>
        /// <returns> シーン復帰に成功した場合はtrue。 </returns>
        public async Task<bool> ReturnFromScenarioAsync(
            string scenarioSceneName,
            string returnSceneName)
        {
            bool success = await _usecase.UnloadAndSetActiveAsync(
                scenarioSceneName,
                returnSceneName,
                _scenarioTransitionCancellationToken);

            if (success && !_scenarioTransitionCancellationToken.IsCancellationRequested)
            {
                _outputPort.ShowHomeScreen();
                _outputPort.SetOutGameActiveForScenario(true);
            }

            return success;
        }

        /// <summary>
        ///     シナリオ終了後のバトル出撃を、予約を所有する出力ポートへ要求します。
        /// </summary>
        public Task<ScenarioBattleSortieResult> RequestBattleSortieFromScenarioAsync(
            string scenarioSceneName, string returnSceneName,
            BattleStageDefinition battleStageDefinition, int scenarioSelectionRevision)
        {
            bool isInputValid = battleStageDefinition != null
                && battleStageDefinition.MissionId.Value != 0
                && !string.IsNullOrWhiteSpace(scenarioSceneName)
                && !string.IsNullOrWhiteSpace(returnSceneName)
                && !string.IsNullOrWhiteSpace(battleStageDefinition.TargetSceneName)
                && !string.IsNullOrWhiteSpace(battleStageDefinition.BattleSceneName)
                && !string.Equals(scenarioSceneName, returnSceneName, StringComparison.Ordinal);
            return _outputPort.StartBattleFromScenarioAsync(
                scenarioSceneName, returnSceneName, battleStageDefinition,
                scenarioSelectionRevision, isInputValid);
        }

        private readonly SceneTransitionUseCase _usecase;
        private readonly IOutGameSortieOutputPort _outputPort;
        private readonly CancellationToken _scenarioTransitionCancellationToken;
    }
}
