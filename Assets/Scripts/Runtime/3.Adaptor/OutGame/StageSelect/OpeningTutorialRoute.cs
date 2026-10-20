using KillChord.Runtime.Domain.OutGame.StageSelect;
using System;

namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     タイトルから始める初回シナリオの後に、チュートリアル戦闘へ直接進むための予約を扱うクラスです。
    ///     <para>
    ///         初回シナリオはタイトルから直接始まり OutGame を経由しないため、
    ///         次のチュートリアル戦闘をここで予約しておき、シナリオの完了時にシナリオから直接出撃する。
    ///         対象のステージはステージツリーのチュートリアル指定から解決し、ステージ名や MissionId は固定しない。
    ///     </para>
    /// </summary>
    public static class OpeningTutorialRoute
    {
        /// <summary>
        ///     初回シナリオの完了を条件に、チュートリアル戦闘への遷移を予約します。
        ///     <para> 以前の予約は破棄します。初回ルートはタイトルから始まり、他の予約を引き継がないためです。 </para>
        /// </summary>
        /// <param name="stageTree"> チュートリアル戦闘を解決するステージツリー。 </param>
        /// <param name="openingScenario"> 初回シナリオのステージ定義。 </param>
        /// <param name="pendingState"> 予約先の状態。 </param>
        /// <param name="returnSceneName"> チュートリアル戦闘の終了後に戻るシーン名。 </param>
        /// <returns> 予約できた場合はtrue。 </returns>
        public static bool TryReserveTutorialBattle(
            StageTree stageTree,
            ScenarioStageDefinition openingScenario,
            PendingNodeTransitionState pendingState,
            string returnSceneName)
        {
            if (stageTree == null) { throw new ArgumentNullException(nameof(stageTree)); }
            if (openingScenario == null) { throw new ArgumentNullException(nameof(openingScenario)); }
            if (pendingState == null) { throw new ArgumentNullException(nameof(pendingState)); }

            pendingState.Clear();
            if (string.IsNullOrWhiteSpace(returnSceneName)
                || !stageTree.TryGetTutorialNode(out StageNode tutorialNode)
                || tutorialNode.Definition is not BattleStageDefinition tutorialBattle)
            {
                return false;
            }

            pendingState.Reserve(new PendingNodeTransition(openingScenario.StageId, tutorialBattle, returnSceneName));
            return true;
        }
    }
}
