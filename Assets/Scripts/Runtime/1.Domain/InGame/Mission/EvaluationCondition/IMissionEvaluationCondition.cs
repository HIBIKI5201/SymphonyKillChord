namespace KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition
{
    /// <summary>
    ///     評価条件を表すインターフェース。
    /// </summary>
    public interface IMissionEvaluationCondition
    {
        bool IsSatisfied(MissionProgress progress);
        string GetDescription();
    }
}
