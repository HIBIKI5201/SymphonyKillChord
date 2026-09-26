using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Skill
{
    /// <summary>
    ///     1つの効果パラメータについて、レベルアップ回数ごとの成長ステップ一覧を保持する値オブジェクトです。
    ///     <para> Stepsの要素位置(Index)が「基準レベルから何回目のレベルアップか」に対応します。 </para>
    /// </summary>
    public readonly struct SkillEffectParameterGrowth : IEquatable<SkillEffectParameterGrowth>
    {
        /// <summary>
        ///     効果パラメータ成長設定を初期化します。
        /// </summary>
        /// <param name="parameterId"> 成長対象の効果パラメータ識別子です。 </param>
        /// <param name="steps"> レベルアップ回数ごとの成長ステップ一覧です。 </param>
        public SkillEffectParameterGrowth(
            SkillEffectParameterId parameterId,
            IReadOnlyList<SkillEffectParameterGrowthStep> steps)
        {
            ParameterId = parameterId;
            _steps = CopySteps(steps);
        }

        /// <summary> 成長対象の効果パラメータ識別子です。 </summary>
        public SkillEffectParameterId ParameterId { get; }

        /// <summary> レベルアップ回数ごとの成長ステップ一覧です。Index=0が1回目のレベルアップに対応します。 </summary>
        public IReadOnlyList<SkillEffectParameterGrowthStep> Steps => _steps ?? Array.Empty<SkillEffectParameterGrowthStep>();

        private readonly SkillEffectParameterGrowthStep[] _steps;

        /// <summary>
        ///     成長ステップ一覧を複製する。
        /// </summary>
        /// <param name="steps"> 複製元です。 </param>
        /// <returns> 複製した配列です。 </returns>
        private static SkillEffectParameterGrowthStep[] CopySteps(IReadOnlyList<SkillEffectParameterGrowthStep> steps)
        {
            if (steps == null || steps.Count == 0)
            {
                return Array.Empty<SkillEffectParameterGrowthStep>();
            }

            SkillEffectParameterGrowthStep[] result = new SkillEffectParameterGrowthStep[steps.Count];
            for (int i = 0; i < steps.Count; i++)
            {
                result[i] = steps[i];
            }

            return result;
        }

        /// <summary>
        ///     値が等しいかを判定します。
        /// </summary>
        /// <param name="other"> 比較対象です。 </param>
        /// <returns> 等しい場合はtrue。 </returns>
        public bool Equals(SkillEffectParameterGrowth other)
        {
            if (ParameterId != other.ParameterId || Steps.Count != other.Steps.Count)
            {
                return false;
            }

            for (int i = 0; i < Steps.Count; i++)
            {
                if (!Steps[i].Equals(other.Steps[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     オブジェクトが等しいかを判定します。
        /// </summary>
        /// <param name="obj"> 比較対象です。 </param>
        /// <returns> 等しい場合はtrue。 </returns>
        public override bool Equals(object obj)
        {
            return obj is SkillEffectParameterGrowth other && Equals(other);
        }

        /// <summary>
        ///     ハッシュコードを取得します。
        /// </summary>
        /// <returns> ハッシュコードです。 </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(ParameterId, Steps.Count);
        }
    }
}
