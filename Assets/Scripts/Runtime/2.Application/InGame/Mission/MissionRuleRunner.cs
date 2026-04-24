using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Application.InGame.Mission
{
    /// <summary>
    ///     ミッションの進行状況を評価し、ミッションのクリアや失敗を判定するクラス。
    /// </summary>
    public class MissionRuleRunner
    {
        public MissionRuleRunner(MissionDefinition missionDefinition)
        {
            _missionDefinition = missionDefinition;
        }

        public void Evaluate(MissionProgress missionProgress)
        {
            if (missionProgress.IsFinished)
            {
                return;
            }

            if (_missionDefinition.FailCondition != null &&
                _missionDefinition.FailCondition.IsSatisfied(missionProgress))
            {
                missionProgress.Finish(MissionEndReason.Fail);
                return;
            }

            if (_missionDefinition.ClearCondition != null &&
               _missionDefinition.ClearCondition.IsSatisfied(missionProgress))
            {
                missionProgress.Finish(MissionEndReason.Clear);
            }
        }

        private readonly MissionDefinition _missionDefinition;
    }
}
