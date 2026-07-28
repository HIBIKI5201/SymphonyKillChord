namespace KillChord.Runtime.Domain.InGame.Buff
{
    /// <summary>
    ///     バフの発動タイミングのタイプ。
    /// </summary>
    public enum BuffExecuteTiming
    {
        /// <summary>
        ///     攻撃計算前に発動。
        /// </summary>
        Attack_Logic_Before,

        /// <summary>
        ///     攻撃計算後に発動。
        /// </summary>
        Attack_Logic_After,

        /// <summary>
        ///     スキル発動時に発動。
        /// </summary>
        Skill,

        /// <summary>
        ///     被ダメージ計算前に発動。
        /// </summary>
        Defense_Logic_Before,
    }
}
