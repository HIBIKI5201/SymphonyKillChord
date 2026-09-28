using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     チュートリアルの色指定課題を、表示と振動で共通の条件に基づいて参照します。
    /// </summary>
    public static class TutorialAttackTargetQuery
    {
        /// <summary>
        ///     説明表示中や終了済みの課題を除き、現在指定されている攻撃の拍数を取得します。
        /// </summary>
        /// <param name="stageState"> 選択中のバトルステージです。 </param>
        /// <param name="mission"> 現在有効なミッションです。 </param>
        /// <returns> 色指定課題の対象拍数です。対象外の場合はnullです。 </returns>
        public static int? GetTargetBeatCount(SelectedBattleStageState stageState, MissionRuntimeService mission)
        {
            if (stageState == null || !stageState.HasSelectedBattleStage
                || !stageState.CurrentStageDefinition.IsTutorial
                || mission == null || mission.MissionProgress.IsFinished)
            {
                return null;
            }

            ObjectiveSequenceStep step = mission.MissionDefinition.ClearCondition
                .GetStep(mission.MissionProgress.ObjectiveStepIndex);
            if (step == null || ClearConditionChain.Contains<PopupClearCondition>(step.Condition))
            {
                return null;
            }

            ActionRepeatCountClearCondition condition =
                ClearConditionChain.Find<ActionRepeatCountClearCondition>(step.Condition);
            return condition != null && condition.TargetBeatType.HasValue
                ? (int)condition.TargetBeatType.Value
                : null;
        }
    }
}
