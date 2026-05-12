namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     評価条件一覧の評価結果を表すクラス。
    /// </summary>
    public class MissionEvaluationResult
    {
        /// <summary>
        ///     MissionEvaluationResult クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="progresses">評価状況のリスト。</param>
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

        /// <summary> 達成した条件の数を取得します。 </summary>
        public int AchievedCount { get; }
        /// <summary> 合計の条件数を取得します。 </summary>
        public int TotalCount { get; }
        /// <summary> 評価状況のリストを取得します。 </summary>
        public MissionEvaluationProgress[] Progresses => _progresses;

        /// <summary> 評価状況のリスト。 </summary>
        private readonly MissionEvaluationProgress[] _progresses;
    }
}
