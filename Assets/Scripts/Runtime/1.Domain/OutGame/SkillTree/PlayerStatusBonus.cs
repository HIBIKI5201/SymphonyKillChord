namespace KillChord.Runtime.Domain.OutGame.SkillTree
{
    /// <summary>
    ///     解放済みスキルノードから得られるプレイヤーステータスボーナス。
    /// </summary>
    public readonly struct PlayerStatusBonus
    {
        /// <summary>
        ///     プレイヤーステータスボーナスを初期化する。
        /// </summary>
        public PlayerStatusBonus(
            float maxHealthMultiplier,
            float attackPowerMultiplier,
            float criticalChanceAddition,
            float criticalMultiplierAddition,
            float areaAttackRangeAddition)
        {
            MaxHealthMultiplier = maxHealthMultiplier;
            AttackPowerMultiplier = attackPowerMultiplier;
            CriticalChanceAddition = criticalChanceAddition;
            CriticalMultiplierAddition = criticalMultiplierAddition;
            AreaAttackRangeAddition = areaAttackRangeAddition;
        }

        /// <summary> 最大体力の倍率。 </summary>
        public float MaxHealthMultiplier { get; }

        /// <summary> 攻撃力の倍率。 </summary>
        public float AttackPowerMultiplier { get; }

        /// <summary> クリティカル率の加算値。 </summary>
        public float CriticalChanceAddition { get; }

        /// <summary> クリティカルダメージ倍率の加算値。 </summary>
        public float CriticalMultiplierAddition { get; }

        /// <summary> 範囲攻撃の射程加算値。 </summary>
        public float AreaAttackRangeAddition { get; }

        /// <summary> ボーナスを持たない既定値。 </summary>
        public static PlayerStatusBonus None => new PlayerStatusBonus(1f, 1f, 0f, 0f, 0f);
    }
}
