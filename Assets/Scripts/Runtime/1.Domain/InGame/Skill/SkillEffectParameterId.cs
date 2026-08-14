namespace KillChord.Runtime.Domain.InGame.Skill
{
    /// <summary>
    ///     スキル効果で使用する数値パラメータの識別子です。
    /// </summary>
    public enum SkillEffectParameterId
    {
        DamageMultiplier = 0,
        CurrentHealthRatio = 1,
        HealthCostRatio = 2,
        HitCount = 3,
        CriticalMultiplier = 4,
        DamageTakenIncreaseRate = 5,
        DurationSeconds = 6,
        LifeStealRate = 7,
        HealPerHitCap = 8,
        DamageReductionRate = 9,
        DamageReductionHitCount = 10,
        BarrierGainRate = 11,
        AttackPowerReductionRate = 12,
        AttackPowerReductionStackLimit = 13,
    }
}
