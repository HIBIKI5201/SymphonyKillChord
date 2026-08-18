using KillChord.Runtime.Application.InGame.Buff;
using KillChord.Runtime.Application.InGame.Target;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using System;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルID 09 のスキル効果を実装するクラス。 
    /// </summary>
    public class Skill_09 : SkillBase
    {
        public Skill_09(IPlayerTargetRangeQuery rangeQuery)
        {
            _rangeQuery = rangeQuery ?? throw new ArgumentNullException(nameof(rangeQuery));
        }

        public override void Execute(in SkillEffectContext context)
        {
            float criticalDamageMultiplier =
                (float)context.EffectSpec.GetRequiredValue(
                    SkillEffectParameterId.CriticalMultiplier);

            float durationSeconds =
                (float)context.EffectSpec.GetRequiredValue(
                    SkillEffectParameterId.DurationSeconds);

            // SMGの攻撃定義を取得して、範囲を取得する
            AttackDefinition smgAtkDef =
                context.PlayerEntity.CombatSpec
                .GetAttackDefinitionByBeatType(BeatType.Eight);
            float fieldRange = smgAtkDef.Range;

            context.PlayerEntity.StatusEffectSystem.Add(
                new CriticalDamageFieldBuff(
                    _rangeQuery,
                    fieldRange,
                    criticalDamageMultiplier,
                    durationSeconds,
                    context.EffectSpec.ReapplyPolicy));
        }

        private readonly IPlayerTargetRangeQuery _rangeQuery;
    }
}
