using KillChord.Runtime.Application.OutGame.Sortie;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Sortie
{
    /// <summary>
    ///     出撃要求をユースケースへ伝えるコントローラー。
    /// </summary>
    public sealed class OutGameSortieController
    {
        /// <summary>
        ///     コントローラーを初期化する。
        /// </summary>
        /// <param name="useCase">出撃ユースケース。</param>
        public OutGameSortieController(OutGameSortieUseCase useCase)
        {
            _useCase = useCase;
        }

        /// <summary>
        ///     ステージ種別に応じた出撃処理を開始する。
        /// </summary>
        /// <param name="stageType"> ステージ種別。 </param>
        /// <param name="currentSceneName"> 現在のシーン名。 </param>
        /// <param name="targetSceneName"> 遷移先シーン名。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 処理に成功した場合は true。 </returns>
        public async Task<bool> RequestSortieAsync(
            StageType stageType,
            string currentSceneName,
            string targetSceneName,
            CancellationToken cancellationToken)
        {
            return await _useCase.RequestSortieAsync(
                stageType,
                currentSceneName,
                targetSceneName,
                cancellationToken);
        }

        /// <summary>
        ///     戦闘準備画面を介さずにバトル開始を要求します。
        /// </summary>
        /// <returns> 要求を受理した場合はtrueです。 </returns>
        public bool RequestImmediateBattleSortie()
        {
            return _useCase.RequestImmediateBattleSortie();
        }

        /// <summary>
        ///     シナリオシーンを終了してOutGameのホーム画面へ復帰します。
        /// </summary>
        /// <param name="scenarioSceneName"> 終了するシナリオシーン名。 </param>
        /// <param name="returnSceneName"> 復帰先のOutGameシーン名。 </param>
        /// <returns> シーン復帰に成功した場合はtrue。 </returns>
        public Task<bool> ReturnFromScenarioAsync(
            string scenarioSceneName,
            string returnSceneName)
        {
            return _useCase.ReturnFromScenarioAsync(
                scenarioSceneName,
                returnSceneName);
        }

        /// <summary>
        ///     シナリオ終了後の専用バトル出撃を要求し、明示的な終端結果を返します。
        /// </summary>
        public Task<ScenarioBattleSortieResult> RequestBattleSortieFromScenarioAsync(
            string scenarioSceneName,
            string returnSceneName,
            BattleStageDefinition battleStageDefinition,
            int scenarioSelectionRevision)
        {
            return _useCase.RequestBattleSortieFromScenarioAsync(
                scenarioSceneName, returnSceneName, battleStageDefinition,
                scenarioSelectionRevision);
        }

        private readonly OutGameSortieUseCase _useCase;
    }
}
