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
        public MissionDefinition(
            MissionId missionId,
            string desplayName,
            string mainMissionText,
            IMissionClearCondition clearCondition,
            IMissionFailCondition failCondition,
            IReadOnlyList<IMissionEvaluationCondition> evaluationConditions)
        {
            MissionId = missionId;
            DesplayName = desplayName;
            MainMissionText = mainMissionText;
            ClearCondition = clearCondition;
            FailCondition = failCondition;
            EvaluationConditions = evaluationConditions;
        }

        public MissionId MissionId { get; }
        public string DesplayName { get; }
        public string MainMissionText { get; }

        public IMissionClearCondition ClearCondition { get; }
        public IMissionFailCondition FailCondition { get; }
        public IReadOnlyList<IMissionEvaluationCondition> EvaluationConditions { get; }
    }
}
