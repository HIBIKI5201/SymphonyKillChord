using KillChord.Runtime.Application.InGame.StatusEffect;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Buff
{
    /// <summary>
    ///     被ダメージを一定割合軽減し続ける永続バフ。
    /// </summary>
    public class DamageReductionBuff : StatusEffectBase, IIncomingDamageModifier
    {
        public DamageReductionBuff(float reductionRate, StatusEffectReapplyPolicy reapplyPolicy) 
            : base(
                EFFECT_ID,
                StatusEffectCategory.Buff,
                StatusEffectDuration.UntilRemoved,
                reapplyPolicy)
        {
            _reductionRate = Mathf.Clamp01(reductionRate);
        }

        /// <inheritdoc />
        public AttackResult ModifyIncomingDamage(
            IAttacker attacker,
            IDefender defender,
            AttackResult result)
        {
            Damage damage =
                result.FinalDamage * (1f - _reductionRate);

            return result.WithFinalDamage(damage);
        }

        private static readonly StatusEffectId EFFECT_ID =
            new(nameof(DamageReductionBuff));

        private readonly float _reductionRate;
    }
}
