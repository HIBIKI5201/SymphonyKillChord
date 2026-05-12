namespace KillChord.Runtime.Domain.InGame.Mission.FailCondition
{
    /// <summary>
    ///     失敗条件を表すインターフェース。
    /// </summary>
    public interface IMissionFailCondition
    {
        bool IsSatisfied(MissionProgress progress);
        string GetDescription();
    }
}
