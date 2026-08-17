using KillChord.Runtime.Application.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using KillChord.Runtime.Domain.Player;
using System;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルID 10 のスキル効果を実装するクラス。 
    /// </summary>
    public class Skill_10 : SkillBase
    {
        public override void Execute(in SkillEffectContext context)
        {
            float reductionRate = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.DamageReductionRate);

            int hitCount = (int)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.DamageReductionHitCount);

            StatusEffectReapplyPolicy reapplyPolicy = context.EffectSpec.ReapplyPolicy;
            ValidateReapplyPolicy(reapplyPolicy);

            context.PlayerEntity.StatusEffectSystem.Add(
                new HitCountDamageReductionBuff(
                    reductionRate,
                    hitCount,
                    reapplyPolicy));

            Debug.Log($"[Skill_10]発動。減少率: {reductionRate}, ヒット回数: {hitCount}");
        }

        /// <summary>
        ///     回数制状態効果で使用可能な再付与方法か検証します。
        ///     回数指定の限定的なものであるため、ReplaceまたはIgnoreのみを許容。
        /// </summary>
        private static void ValidateReapplyPolicy(
            StatusEffectReapplyPolicy reapplyPolicy)
        {
            if (reapplyPolicy == StatusEffectReapplyPolicy.Replace ||
                reapplyPolicy == StatusEffectReapplyPolicy.Ignore)
            {
                return;
            }

            throw new InvalidOperationException(
                $"{nameof(Skill_10)}では" +
                $"{StatusEffectReapplyPolicy.Replace}または" +
                $"{StatusEffectReapplyPolicy.Ignore}を指定してください。");
        }
    }
}
