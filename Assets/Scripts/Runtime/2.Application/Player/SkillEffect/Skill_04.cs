using KillChord.Runtime.Application.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキル04の効果を表すクラス。
    /// </summary>
    public class Skill_04 : SkillBase
    {
        public override void Execute(in SkillEffectContext context)
        {
            float barrierGainRate = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.BarrierGainRate);

            float durationSeconds = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.DurationSeconds);

            context.PlayerEntity.StatusEffectSystem.Add(
                new BarrierGainBuff(
                    context.PlayerEntity,
                    barrierGainRate,
                    durationSeconds,
                    context.EffectSpec.ReapplyPolicy));

            Debug.Log($"[Skill_04] 発動 " +
                $"BarrierGainRate: {barrierGainRate}" +
                $", DurationSeconds: {durationSeconds}");
        }
    }
}
