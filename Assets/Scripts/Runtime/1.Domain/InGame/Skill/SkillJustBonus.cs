using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Skill
{
    /// <summary>
    ///     スキルのコマンドをJustで入力した割合で、スキルの効果を強くする設定です。
    ///     効果の割合 = 基準の割合 +（1 − 基準の割合）×（Justの数 ÷ 入力の数）。
    ///     全部通常なら基準の割合、全部Justなら100%になります。
    /// </summary>
    public sealed class SkillJustBonus
    {
        /// <summary>
        ///     基準の割合と、強くするパラメータを指定して初期化します。
        /// </summary>
        /// <param name="baseRate"> 全部通常で入力したときの効果の割合（0〜1）です。 </param>
        /// <param name="targetParameterIds"> Justの割合を掛けるパラメータです。 </param>
        public SkillJustBonus(float baseRate, IReadOnlyList<SkillEffectParameterId> targetParameterIds)
        {
            if (float.IsNaN(baseRate) || baseRate < MIN_RATE || baseRate > MAX_RATE)
            {
                throw new ArgumentOutOfRangeException(nameof(baseRate), "基準の割合は0〜1で設定してください。");
            }

            BaseRate = baseRate;
            _targetParameterIds = CopyTargets(targetParameterIds);
        }

        /// <summary> 全部通常で入力したときの効果の割合（0〜1）です。 </summary>
        public float BaseRate { get; }

        /// <summary> Justの割合を掛けるパラメータです。 </summary>
        public IReadOnlyList<SkillEffectParameterId> TargetParameterIds => _targetParameterIds;

        /// <summary> 基準の割合の既定値です。 </summary>
        public const float DEFAULT_BASE_RATE = 0.5f;

        /// <summary>
        ///     基準の割合を既定値にし、ダメージ倍率を強くする設定を作ります。
        /// </summary>
        /// <returns> 作った設定です。 </returns>
        public static SkillJustBonus CreateDefault()
        {
            return new SkillJustBonus(DEFAULT_BASE_RATE, new[] { SkillEffectParameterId.DamageMultiplier });
        }

        /// <summary>
        ///     Justの数から効果の割合を求めます。
        /// </summary>
        /// <param name="justCount"> Justで入力した数です。 </param>
        /// <param name="inputCount"> コマンドの入力の数です。 </param>
        /// <returns> 効果の割合（基準の割合〜1）です。入力の数が0以下の場合は1です。 </returns>
        public float CalculateEffectRate(int justCount, int inputCount)
        {
            if (inputCount <= 0)
            {
                return MAX_RATE;
            }

            // Justの数が入力の数を超えたり負になったりしないよう、範囲に収める。
            int clampedJustCount = Math.Clamp(justCount, 0, inputCount);
            float justRatio = (float)clampedJustCount / inputCount;
            return BaseRate + (MAX_RATE - BaseRate) * justRatio;
        }

        /// <summary>
        ///     効果定義のうち強くするパラメータに、効果の割合を掛けた効果定義を作ります。
        /// </summary>
        /// <param name="effectSpec"> 元の効果定義です。 </param>
        /// <param name="effectRate"> 掛ける効果の割合です。 </param>
        /// <returns> 割合を掛けた効果定義です。 </returns>
        public SkillEffectSpec ApplyTo(in SkillEffectSpec effectSpec, float effectRate)
        {
            IReadOnlyList<SkillEffectParameter> parameters = effectSpec.Parameters;
            SkillEffectParameter[] scaled = new SkillEffectParameter[parameters.Count];
            for (int i = 0; i < parameters.Count; i++)
            {
                SkillEffectParameter parameter = parameters[i];
                scaled[i] = IsTarget(parameter.Id)
                    ? new SkillEffectParameter(parameter.Id, parameter.Value * effectRate, parameter.DisplayFormat)
                    : parameter;
            }

            return new SkillEffectSpec(
                effectSpec.EffectType,
                effectSpec.TargetingType,
                scaled,
                effectSpec.ReapplyPolicy,
                effectSpec.SkillNormalAttackDamagePolicy);
        }

        private const float MIN_RATE = 0f;
        private const float MAX_RATE = 1f;

        private readonly SkillEffectParameterId[] _targetParameterIds;

        /// <summary>
        ///     強くするパラメータか判定します。
        /// </summary>
        /// <param name="id"> 判定するパラメータです。 </param>
        /// <returns> 強くする場合はtrue。 </returns>
        private bool IsTarget(SkillEffectParameterId id)
        {
            return Array.IndexOf(_targetParameterIds, id) >= 0;
        }

        /// <summary>
        ///     強くするパラメータの一覧を複製し、重複を取り除きます。
        /// </summary>
        /// <param name="targetParameterIds"> 複製元です。 </param>
        /// <returns> 複製した配列です。 </returns>
        private static SkillEffectParameterId[] CopyTargets(IReadOnlyList<SkillEffectParameterId> targetParameterIds)
        {
            if (targetParameterIds == null || targetParameterIds.Count == 0)
            {
                return Array.Empty<SkillEffectParameterId>();
            }

            List<SkillEffectParameterId> result = new(targetParameterIds.Count);
            for (int i = 0; i < targetParameterIds.Count; i++)
            {
                if (!result.Contains(targetParameterIds[i]))
                {
                    result.Add(targetParameterIds[i]);
                }
            }

            return result.ToArray();
        }
    }
}
