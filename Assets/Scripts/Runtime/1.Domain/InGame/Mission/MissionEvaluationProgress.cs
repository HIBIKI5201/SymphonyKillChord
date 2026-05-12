namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     評価条件1件分の達成状況を表す構造体。
    /// </summary>
    public readonly struct MissionEvaluationProgress
    {
        public MissionEvaluationProgress(string description, bool isAchieved)
        {
            Description = description;
            IsAchieved = isAchieved;
        }

        public string Description { get; }
        public bool IsAchieved { get; }
    }
}
