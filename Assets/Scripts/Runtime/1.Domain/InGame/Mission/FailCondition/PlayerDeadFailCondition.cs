namespace KillChord.Runtime.Domain.InGame.Mission.FailCondition
{
    /// <summary>
    ///     プレイヤーが死亡すると失敗となる条件を表すクラス。
    /// </summary>
    public class PlayerDeadFailCondition : IMissionFailCondition
    {
        public string GetDescription()
        {
            return "プレイヤーが死亡すると失敗";
        }

        public bool IsSatisfied(MissionProgress progress)
        {
            return progress.IsPlayerDead;
        }
    }
}
