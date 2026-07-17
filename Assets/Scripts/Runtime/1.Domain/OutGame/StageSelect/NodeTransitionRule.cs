using System;

namespace KillChord.Runtime.Domain.OutGame.StageSelect
{
    /// <summary>
    ///     ノード再生完了後の後続処理ルールを表します。
    /// </summary>
    public sealed class NodeTransitionRule
    {
        /// <summary>
        ///     ルールを初期化します。
        /// </summary>
        /// <param name="triggerStageId"> 発火元のステージIDです。 </param>
        /// <param name="requireTutorialIncomplete"> チュートリアル未完了が条件の場合はtrueです。 </param>
        /// <param name="actionType"> 実行するアクション種別です。 </param>
        /// <param name="targetStageId"> 遷移先ステージIDです。 </param>
        /// <param name="priority"> 優先度です。 </param>
        public NodeTransitionRule(
            StageId triggerStageId,
            bool requireTutorialIncomplete,
            NodeTransitionActionType actionType,
            StageId targetStageId,
            int priority)
        {
            if (triggerStageId.Value == 0)
            {
                throw new ArgumentException("Trigger stage id must not be empty.", nameof(triggerStageId));
            }

            if (targetStageId.Value == 0)
            {
                throw new ArgumentException("Target stage id must not be empty.", nameof(targetStageId));
            }

            TriggerStageId = triggerStageId;
            RequireTutorialIncomplete = requireTutorialIncomplete;
            ActionType = actionType;
            TargetStageId = targetStageId;
            Priority = priority;
        }

        /// <summary> 発火元のステージIDです。 </summary>
        public StageId TriggerStageId { get; }

        /// <summary> チュートリアル未完了を要求する場合はtrueです。 </summary>
        public bool RequireTutorialIncomplete { get; }

        /// <summary> 実行するアクション種別です。 </summary>
        public NodeTransitionActionType ActionType { get; }

        /// <summary> 遷移先ステージIDです。 </summary>
        public StageId TargetStageId { get; }

        /// <summary> 優先度です。 </summary>
        public int Priority { get; }

        /// <summary>
        ///     指定コンテキストに対してこのルールが一致するかを判定します。
        /// </summary>
        /// <param name="currentStageDefinition"> 現在のステージ定義です。 </param>
        /// <param name="isTutorialCompleted"> チュートリアル完了状態です。 </param>
        /// <returns> 一致する場合はtrueです。 </returns>
        public bool IsMatch(StageDefinition currentStageDefinition, bool isTutorialCompleted)
        {
            if (currentStageDefinition == null)
            {
                return false;
            }

            if (!currentStageDefinition.StageId.Equals(TriggerStageId))
            {
                return false;
            }

            if (RequireTutorialIncomplete && isTutorialCompleted)
            {
                return false;
            }

            return true;
        }
    }
}
