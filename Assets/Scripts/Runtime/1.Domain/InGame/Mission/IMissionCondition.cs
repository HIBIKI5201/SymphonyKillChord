namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッションの条件（クリア条件・失敗条件）に共通するインターフェース。
    /// </summary>
    public interface IMissionCondition
    {
        /// <summary>
        ///     条件が満たされているかどうかを判定します。
        /// </summary>
        /// <param name="progress">ミッションの進行状況。</param>
        /// <returns>条件を満たしている場合は true、そうでない場合は false。</returns>
        public bool IsSatisfied(MissionProgress progress);

        /// <summary>
        ///     条件の説明文を取得します。
        /// </summary>
        /// <returns>説明文。</returns>
        public string GetDescription();
    }
}
