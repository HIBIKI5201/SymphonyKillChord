using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using System;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルID 03 のスキル効果を実装するクラス。
    /// </summary>
    public class Skill_03 : SkillBase
    {
        /// <summary>
        ///     スキル効果を初期化します。
        /// </summary>
        /// <param name="buff"> 付与バフです。 </param>
        public Skill_03(IBuff buff) : base(buff)
        {
        }

        /// <summary>
        ///     スキル効果を実行します。
        /// </summary>
        /// <param name="context"> 実行コンテキストです。 </param>
        public override void Execute(in SkillEffectContext context)
        {
            float damageMultiplier = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.DamageMultiplier);
            // ダメージ倍率が 0 の場合は、基本倍率を使用する。
            if (damageMultiplier == 0f)
            {
                damageMultiplier = BASE_DAMAGE_MULTIPLIER;
            }

            AttackDefinition attackDefinition = context.PlayerEntity.CombatSpec.GetAttackDefinitionByBeatType(context.CurrentBeatType);
            AttackResult result = AttackCalculator.Calculate(
                attackDefinition,
                context.PlayerEntity,
                context.TargetEntity,
                false,
                context.PlayerEntity.BaseDamage * damageMultiplier);

            ReadOnlySpan<CharacterEntity> targets = context.TargetEntities.Span;
            for (int i = 0; i < targets.Length; i++)
            {
                CharacterEntity target = targets[i];
                if (target == null || ReferenceEquals(target, context.TargetEntity))
                {
                    continue;
                }

                target.TakeDamage(result.FinalDamage / damageMultiplier);
            }
        }

        private const float BASE_DAMAGE_MULTIPLIER = 1f;
    }
}
