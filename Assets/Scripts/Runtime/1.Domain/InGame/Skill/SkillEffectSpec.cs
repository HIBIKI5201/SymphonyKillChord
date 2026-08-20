namespace KillChord.Runtime.Domain.InGame.Skill
{
    /// <summary>
    ///     スキル効果の定義データです。
    /// </summary>
    public readonly struct SkillEffectSpec
    {
        /// <summary>
        ///     効果定義を初期化します。
        /// </summary>
        /// <param name="effectType"> 効果種別です。 </param>
        /// <param name="targetingType"> 対象解決ルールです。 </param>
        public SkillEffectSpec(SkillEffectType effectType, SkillTargetingType targetingType)
        {
            EffectType = effectType;
            TargetingType = targetingType;
        }

        /// <summary> 効果種別です。 </summary>
        public SkillEffectType EffectType { get; }

        /// <summary> 対象解決ルールです。 </summary>
        public SkillTargetingType TargetingType { get; }
    }
}
