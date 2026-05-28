namespace KillChord.Runtime.Domain.InGame.Mission.EvaluationCondition
{
    /// <summary>
    ///     評価条件を表すインターフェース。
    /// </summary>
    public interface IMissionEvaluationCondition
    {
        /// <summary> 成功表示を行うタイミングを取得します。 </summary>
        public MissionEvaluationResultTiming ResultTiming { get; }

        /// <summary>
        ///     条件が満たされているかどうかを判定します。
        /// </summary>
        /// <param name="progress">ミッションの進行状況。</param>
        /// <returns>条件を満たしている場合は true、そうでない場合は false。</returns>
        public bool IsSatisfied(MissionProgress progress);

        /// <summary>
        ///     条件が失敗確定かどうかを判定します。
        /// </summary>
        /// <param name="progress"> ミッションの進行状況。 </param>
        /// <returns> 条件が失敗確定の場合は true、そうでない場合は false。 </returns>
        public bool IsFailed(MissionProgress progress);

        /// <summary>
        ///     条件の説明文を取得します。
        /// </summary>
        /// <returns>説明文。</returns>
        public string GetDescription();
    }
}
