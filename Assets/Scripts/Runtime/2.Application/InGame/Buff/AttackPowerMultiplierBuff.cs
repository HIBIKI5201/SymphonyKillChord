using KillChord.Runtime.Application.InGame.StatusEffect;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Buff
{
    /// <summary>
    ///     与えるダメージを一定倍率へ変更し続ける永続バフ。
    /// </summary>
    public class AttackPowerMultiplierBuff : StatusEffectBase, IOutgoingDamageModifier
    {
        public AttackPowerMultiplierBuff(float multiplier, StatusEffectReapplyPolicy reapplyPolicy)
            : base(
                EFFECT_ID,
                StatusEffectCategory.Buff,
                StatusEffectDuration.UntilRemoved,
                reapplyPolicy)
        {
            _multiplier = Mathf.Max(0f, multiplier);
        }

        /// <inheritdoc />
        public AttackResult ModifyOutgoingDamage(
            IAttacker attacker,
            IDefender defender,
            AttackResult result)
        {
            Damage damage = result.FinalDamage * _multiplier;

            return result.WithFinalDamage(damage);
        }

        /// <summary>
        ///     状態効果の識別子。
        /// </summary>
        private static readonly StatusEffectId EFFECT_ID =
            new(nameof(AttackPowerMultiplierBuff));

        private readonly float _multiplier;
    }
}
