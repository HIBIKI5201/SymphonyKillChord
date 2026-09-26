using KillChord.Runtime.Application.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルID 06 のスキル効果を実装するクラス。
    /// </summary>
    public class Skill_06 : SkillBase
    {
        /// <summary>
        ///     スキル効果を実行します。
        /// </summary>
        /// <param name="context"> 実行コンテキストです。 </param>
        public override void Execute(in SkillEffectContext context)
        {
            float healRate =
                (float)context.EffectSpec.GetRequiredValue(SkillEffectParameterId.LifeStealRate);
            float maxHealPerHit =
                (float)context.EffectSpec.GetRequiredValue(SkillEffectParameterId.HealPerHitCap);
            float durationSeconds =
                (float)context.EffectSpec.GetRequiredValue(SkillEffectParameterId.DurationSeconds);

            context.PlayerEntity.StatusEffectSystem.Add(
                new LifeStealBuff(
                    context.PlayerEntity,
                    healRate,
                    maxHealPerHit,
                    durationSeconds,
                    context.EffectSpec.ReapplyPolicy));
        }
    }
}
