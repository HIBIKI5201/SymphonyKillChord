namespace KillChord.Runtime.Domain.InGame.Skill
{
    /// <summary>
    ///     スキルレベルによる効果パラメータの成長方式です。
    /// </summary>
    public enum SkillEffectParameterGrowthType
    {
        /// <summary> レベルごとに固定量を加算します。 </summary>
        Additive = 0,

        /// <summary> レベルごとに倍率を乗算します。 </summary>
        Multiplicative = 1,
    }
}
