using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition;
using KillChord.Runtime.Domain.InGame.Mission.FailCondition;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッションの定義を表すクラス。
    /// </summary>
    public class MissionDefinition
    {
        /// <summary>
        ///     MissionDefinition クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="missionId">ミッションID。</param>
        /// <param name="displayName">表示名。</param>
        /// <param name="mainMissionText">メインミッションのテキスト。</param>
        /// <param name="clearCondition">クリア条件。</param>
        /// <param name="failCondition">失敗条件。</param>
        /// <param name="evaluationConditions">評価条件のリスト。</param>
        public MissionDefinition(
            MissionId missionId,
            string displayName,
            string mainMissionText,
            IMissionClearCondition clearCondition,
            IMissionFailCondition failCondition,
            IReadOnlyList<IMissionEvaluationCondition> evaluationConditions)
        {
            MissionId = missionId;
            DisplayName = displayName;
            MainMissionText = mainMissionText;
            ClearCondition = clearCondition;
            FailCondition = failCondition;
            EvaluationConditions = evaluationConditions;
        }

        /// <summary> ミッションIDを取得します。 </summary>
        public MissionId MissionId { get; }
        /// <summary> 表示名を取得します。 </summary>
        public string DisplayName { get; }
        /// <summary> メインミッションのテキストを取得します。 </summary>
        public string MainMissionText { get; }

        /// <summary> クリア条件を取得します。 </summary>
        public IMissionClearCondition ClearCondition { get; }
        /// <summary> 失敗条件を取得します。 </summary>
        public IMissionFailCondition FailCondition { get; }
        /// <summary> 評価条件のリストを取得します。 </summary>
        public IReadOnlyList<IMissionEvaluationCondition> EvaluationConditions { get; }
    }
}
