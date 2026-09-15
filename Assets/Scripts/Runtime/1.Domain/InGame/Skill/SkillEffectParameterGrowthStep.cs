using System;

namespace KillChord.Runtime.Domain.InGame.Skill
{
    /// <summary>
    ///     1回のレベルアップ分の効果パラメータ成長設定を保持する値オブジェクトです。
    /// </summary>
    public readonly struct SkillEffectParameterGrowthStep : IEquatable<SkillEffectParameterGrowthStep>
    {
        /// <summary>
        ///     成長ステップを初期化します。
        /// </summary>
        /// <param name="growthType"> 成長方式です。 </param>
        /// <param name="growthValue"> 加算量または乗算倍率です。 </param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public SkillEffectParameterGrowthStep(
            SkillEffectParameterGrowthType growthType,
            double growthValue)
        {
            if (double.IsNaN(growthValue) || double.IsInfinity(growthValue))
            {
                throw new ArgumentOutOfRangeException(nameof(growthValue), "成長値には有限値を指定してください。");
            }

            if (growthType == SkillEffectParameterGrowthType.Multiplicative && growthValue <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(growthValue), "乗算方式の成長値は0より大きい値を指定してください。");
            }

            GrowthType = growthType;
            GrowthValue = growthValue;
        }

        /// <summary> 成長方式です。 </summary>
        public SkillEffectParameterGrowthType GrowthType { get; }

        /// <summary> 加算量または乗算倍率です。 </summary>
        public double GrowthValue { get; }

        /// <summary>
        ///     基準値へ、このステップ1回分の成長を適用します。
        /// </summary>
        /// <param name="baseValue"> 適用前の値です。 </param>
        /// <returns> 成長後の値です。 </returns>
        public double ApplyTo(double baseValue)
        {
            return GrowthType switch
            {
                SkillEffectParameterGrowthType.Additive => baseValue + GrowthValue,
                SkillEffectParameterGrowthType.Multiplicative => baseValue * GrowthValue,
                _ => baseValue,
            };
        }

        /// <summary>
        ///     値が等しいかを判定します。
        /// </summary>
        /// <param name="other"> 比較対象です。 </param>
        /// <returns> 等しい場合はtrue。 </returns>
        public bool Equals(SkillEffectParameterGrowthStep other)
        {
            return GrowthType == other.GrowthType && GrowthValue.Equals(other.GrowthValue);
        }

        /// <summary>
        ///     オブジェクトが等しいかを判定します。
        /// </summary>
        /// <param name="obj"> 比較対象です。 </param>
        /// <returns> 等しい場合はtrue。 </returns>
        public override bool Equals(object obj)
        {
            return obj is SkillEffectParameterGrowthStep other && Equals(other);
        }

        /// <summary>
        ///     ハッシュコードを取得します。
        /// </summary>
        /// <returns> ハッシュコードです。 </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(GrowthType, GrowthValue);
        }
    }
}
