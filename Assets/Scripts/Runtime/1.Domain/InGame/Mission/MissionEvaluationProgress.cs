namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     評価条件1件分の達成状況を表す構造体。
    /// </summary>
    public readonly struct MissionEvaluationProgress
    {
        /// <summary>
        ///     MissionEvaluationProgress 構造体の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="description">条件の説明文。</param>
        /// <param name="isAchieved">達成したかどうか。</param>
        public MissionEvaluationProgress(string description, bool isAchieved)
        {
            Description = description;
            IsAchieved = isAchieved;
        }

        /// <summary> 条件の説明文を取得します。 </summary>
        public string Description { get; }
        /// <summary> 達成したかどうかを取得します。 </summary>
        public bool IsAchieved { get; }
    }
}
