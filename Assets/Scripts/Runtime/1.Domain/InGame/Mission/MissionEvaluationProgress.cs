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
        /// <param name="displaySituation">HUD表示状態。</param>
        public MissionEvaluationProgress(
            string description,
            MissionEvaluationDisplaySituation displaySituation)
        {
            Description = description;
            DisplaySituation = displaySituation;
        }

        /// <summary> 条件の説明文を取得します。 </summary>
        public string Description { get; }
        /// <summary> HUD表示状態を取得します。 </summary>
        public MissionEvaluationDisplaySituation DisplaySituation { get; }

        /// <summary> 成功状態かどうかを取得します。 </summary>
        public bool IsSucceeded => DisplaySituation == MissionEvaluationDisplaySituation.Succeeded;
    }
}
