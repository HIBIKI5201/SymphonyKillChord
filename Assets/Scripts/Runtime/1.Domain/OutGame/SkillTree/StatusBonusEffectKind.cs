namespace KillChord.Runtime.Domain.OutGame.SkillTree
{
    /// <summary>
    ///     ステータスボーナス効果の種別。
    /// </summary>
    public enum StatusBonusEffectKind
    {
        /// <summary> 最大HP。 </summary>
        MaxHealth,
        /// <summary> 攻撃力。 </summary>
        AttackPower,
        /// <summary> 会心率。 </summary>
        CriticalChance,
        /// <summary> 会心ダメージ。 </summary>
        CriticalDamage,
        /// <summary> 範囲攻撃時の攻撃範囲。 </summary>
        AreaAttackRange,
    }
}
