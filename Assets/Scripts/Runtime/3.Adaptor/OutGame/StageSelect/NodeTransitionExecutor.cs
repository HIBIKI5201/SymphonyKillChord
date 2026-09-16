using KillChord.Runtime.Adaptor.OutGame.Sortie;
using KillChord.Runtime.Application.OutGame.Sortie;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using System;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     予約済みノード遷移を実行します。
    /// </summary>
    public sealed class NodeTransitionExecutor
    {
        /// <summary>
        ///     実行器を初期化します。
        /// </summary>
        /// <param name="outGameSortieController"> 出撃コントローラーです。 </param>
        public NodeTransitionExecutor(OutGameSortieController outGameSortieController)
        {
            _outGameSortieController = outGameSortieController
                ?? throw new ArgumentNullException(nameof(outGameSortieController));
        }

        /// <summary>
        ///     シナリオの予約候補を専用出撃へ渡します。受付前には選択・予約を変更しません。
        /// </summary>
        public Task<ScenarioBattleSortieResult> TryExecuteAsync(
            PendingNodeTransition candidate, string scenarioSceneName,
            int scenarioSelectionRevision)
        {
            return _outGameSortieController.RequestBattleSortieFromScenarioAsync(
                scenarioSceneName, candidate.ReturnSceneName,
                candidate.TargetStageDefinition as BattleStageDefinition,
                scenarioSelectionRevision);
        }

        private readonly OutGameSortieController _outGameSortieController;
    }
}
