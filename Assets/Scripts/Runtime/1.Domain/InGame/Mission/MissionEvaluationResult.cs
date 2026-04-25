namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     評価条件一覧の評価結果を表すクラス。
    /// </summary>
    public class MissionEvaluationResult
    {
        public MissionEvaluationResult(MissionEvaluationProgress[] progresses)
        {
            _progresses = progresses;

            int achievedCount = 0;
            for (int i = 0; i < _progresses.Length; i++)
            {
                if (_progresses[i].IsAchieved)
                {
                    achievedCount++;
                }
            }

            AchievedCount = achievedCount;
            TotalCount = progresses.Length;
        }

        public int AchievedCount { get; }
        public int TotalCount { get; }
        public MissionEvaluationProgress[] Progreaaes => _progresses;

        private readonly MissionEvaluationProgress[] _progresses;

    }
}
