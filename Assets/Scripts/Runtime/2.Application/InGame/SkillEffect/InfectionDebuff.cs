using KillChord.Runtime.Application.InGame.StatusEffect;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using KillChord.Runtime.Utility.Persistent;
using System;

namespace KillChord.Runtime.Application.InGame.SkillEffect
{
    /// <summary>
    ///     伝染状態を表すステータス効果です。
    /// </summary>
    internal class InfectionDebuff : StatusEffectBase, IDamageTakenHandler, IConsumableStatusEffect
    {
        public InfectionDebuff(
            CharacterEntity owner, InfectionGroup infectionGroup, StatusEffectReapplyPolicy reapplyPolicy)
            : base(
                  EFFECT_ID,
                  StatusEffectCategory.Debuff,
                  StatusEffectDuration.UntilRemoved,
                  reapplyPolicy)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _infectionGroup = infectionGroup ?? throw new ArgumentNullException(nameof(infectionGroup));
        }

        /// <summary > ステータス効果のIDを取得します。 </summary>
        public static StatusEffectId EffectId => EFFECT_ID;

        /// <inheritdoc/>
        public bool IsConsumed => _infectionGroup.IsConsumed;

        /// <inheritdoc/>
        public void OnDamageTaken(in DamageTakenContext context)
        {
            if (IsConsumed || context.AttackType == DamageAttackType.Infection)
            {
                return;
            }

            if (!ReferenceEquals(context.Defender, _owner))
            {
                return;
            }

            _infectionGroup.Trigger(context);
        }

        private static readonly StatusEffectId EFFECT_ID =
            new("Skill08.InfectionDebuff");

        private readonly CharacterEntity _owner;
        private readonly InfectionGroup _infectionGroup;
    }
}
