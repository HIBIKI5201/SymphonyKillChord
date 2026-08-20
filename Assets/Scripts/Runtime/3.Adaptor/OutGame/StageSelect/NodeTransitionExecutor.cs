using KillChord.Runtime.Adaptor.OutGame.Sortie;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using System;

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
        /// <param name="battleSortieSelectionService"> バトル出撃準備サービスです。 </param>
        /// <param name="outGameSortieController"> 出撃コントローラーです。 </param>
        public NodeTransitionExecutor(
            BattleSortieSelectionService battleSortieSelectionService,
            OutGameSortieController outGameSortieController)
        {
            _battleSortieSelectionService = battleSortieSelectionService
                ?? throw new ArgumentNullException(nameof(battleSortieSelectionService));
            _outGameSortieController = outGameSortieController
                ?? throw new ArgumentNullException(nameof(outGameSortieController));
        }

        /// <summary>
        ///     予約済み遷移を実行します。
        /// </summary>
        /// <param name="pendingNodeTransition"> 実行する遷移情報です。 </param>
        /// <returns> 実行に成功した場合はtrueです。 </returns>
        public bool TryExecute(PendingNodeTransition pendingNodeTransition)
        {
            if (pendingNodeTransition == null)
            {
                return false;
            }

            if (pendingNodeTransition.TargetStageDefinition
                is not BattleStageDefinition targetStageDefinition)
            {
                return false;
            }
            if (!_battleSortieSelectionService.TryPrepareBattleSortie(
                    targetStageDefinition,
                    pendingNodeTransition.ReturnSceneName))
            {
                return false;
            }

            return _outGameSortieController.RequestImmediateBattleSortie(targetStageDefinition.TargetSceneName);
        }

        private readonly BattleSortieSelectionService _battleSortieSelectionService;
        private readonly OutGameSortieController _outGameSortieController;
    }
}
