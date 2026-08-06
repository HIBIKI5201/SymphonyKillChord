using System;

namespace KillChord.Runtime.Domain.OutGame.SkillTree
{
    /// <summary>
    ///     プレイヤーのステータスボーナスを構築するためのビルダークラス。
    /// </summary>
    public sealed class PlayerStatusBonusBuilder
    {
        /// <summary> 最大体力の倍率を取得します。 </summary>
        public float MaxHealthMultiplier { get; private set; } = 1.0f;

        /// <summary> 攻撃力の倍率を取得します。 </summary>
        public float AttackPowerMultiplier { get; private set; } = 1.0f;

        /// <summary> 会心率の加算値を取得します。 </summary>
        public float CriticalChanceAddition { get; private set; }

        /// <summary> 会心ダメージ倍率の加算値を取得します。 </summary>
        public float CriticalMultiplierAddition { get; private set; }

        /// <summary> 範囲攻撃の範囲の追加量を取得します。 </summary>
        public float AreaAttackRangeAddition { get; private set; } = 0.0f;

        /// <summary>
        ///     最大 HP の倍率加算値を加えます。
        /// </summary>
        /// <param name="multiplierAddition"> 最大 HP の倍率加算値。 </param>
        public void AddMaxHealthMultiplier(float multiplierAddition)
        {
            ValidateNonNegativeFinite(multiplierAddition, nameof(multiplierAddition));
            MaxHealthMultiplier += multiplierAddition;
        }

        /// <summary>
        ///     攻撃力の倍率加算値を加えます。
        /// </summary>
        /// <param name="multiplierAddition"> 攻撃力の倍率加算値。 </param>
        public void AddAttackPowerMultiplier(float multiplierAddition)
        {
            ValidateNonNegativeFinite(multiplierAddition, nameof(multiplierAddition));
            AttackPowerMultiplier += multiplierAddition;
        }

        /// <summary>
        ///    会心率を加算します。
        /// </summary>
        /// <param name="value"> 会心率の加算値。 </param>
        public void AddCriticalChance(float value)
        {
            ValidateNonNegativeFinite(value, nameof(value));
            CriticalChanceAddition += value;
        }

        /// <summary>
        ///    会心ダメージ倍率を加算します。
        /// </summary>
        /// <param name="value"> 会心ダメージ倍率の加算値。 </param>
        public void AddCriticalMultiplier(float value)
        {
            ValidateNonNegativeFinite(value, nameof(value));
            CriticalMultiplierAddition += value;
        }

        /// <summary>
        ///     範囲攻撃の射程を加算します。
        /// </summary>
        /// <param name="value"> 射程の加算値。 </param>
        public void AddAreaAttackRange(float value)
        {
            ValidateNonNegativeFinite(value, nameof(value));
            AreaAttackRangeAddition += value;
        }

        /// <summary>
        ///     集計したプレイヤーステータスボーナスを生成します。
        /// </summary>
        /// <returns> 集計済みのボーナス。 </returns>
        public PlayerStatusBonus Build()
        {
            return new PlayerStatusBonus(
                MaxHealthMultiplier,
                AttackPowerMultiplier,
                CriticalChanceAddition,
                CriticalMultiplierAddition,
                AreaAttackRangeAddition);
        }

        /// <summary>
        ///     値が有限の0以上であることを検証します。
        /// </summary>
        private static void ValidateNonNegativeFinite(float value, string paramName)
        {
            if (!float.IsFinite(value) || value < 0f)
            {
                throw new ArgumentOutOfRangeException(paramName, "値は有限の0以上である必要があります。");
            }
        }
    }
}
