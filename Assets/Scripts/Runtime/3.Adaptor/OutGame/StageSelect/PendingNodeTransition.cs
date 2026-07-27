using KillChord.Runtime.Domain.OutGame.StageSelect;
using System;

namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     後続実行待ちのノード遷移情報です。
    /// </summary>
    public sealed class PendingNodeTransition
    {
        /// <summary>
        ///     遷移情報を初期化します。
        /// </summary>
        /// <param name="triggerStageId"> 遷移を発生させる接続元ステージIDです。 </param>
        /// <param name="targetStageDefinition"> 遷移先ステージ定義です。 </param>
        /// <param name="returnSceneName"> 戦闘終了後の帰還先シーン名です。 </param>
        public PendingNodeTransition(
            StageId triggerStageId,
            StageDefinition targetStageDefinition,
            string returnSceneName)
        {
            if (triggerStageId.Value == 0)
            {
                throw new ArgumentException("Trigger stage id must not be empty.", nameof(triggerStageId));
            }

            if (targetStageDefinition == null)
            {
                throw new ArgumentNullException(nameof(targetStageDefinition));
            }

            if (string.IsNullOrWhiteSpace(returnSceneName))
            {
                throw new ArgumentException("Return scene name must not be empty.", nameof(returnSceneName));
            }

            TriggerStageId = triggerStageId;
            TargetStageDefinition = targetStageDefinition;
            ReturnSceneName = returnSceneName;
        }

        /// <summary> 遷移を発生させる接続元ステージIDです。 </summary>
        public StageId TriggerStageId { get; }

        /// <summary> 遷移先ステージ定義です。 </summary>
        public StageDefinition TargetStageDefinition { get; }

        /// <summary> 戦闘終了後の帰還先シーン名です。 </summary>
        public string ReturnSceneName { get; }
    }
}
