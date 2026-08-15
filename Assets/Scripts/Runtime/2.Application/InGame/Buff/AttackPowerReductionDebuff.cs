using KillChord.Runtime.Application.InGame.StatusEffect;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using System;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Buff
{
    public class AttackPowerReductionDebuff
        : StatusEffectBase, IAttackPowerModifier, IAccumulatingStatusEffect
    {
        public AttackPowerReductionDebuff(
            float reductionAmount,
            float reductionCap,
            float durationInSeconds)
            : base(
                  EFFECT_ID,
                  StatusEffectCategory.Debuff,
                  StatusEffectDuration.FromSeconds(durationInSeconds),
                  StatusEffectReapplyPolicy.RefreshDuration)
        {
            if (!float.IsFinite(reductionAmount) || reductionAmount < 0f)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(reductionAmount),
                    "攻撃力減少率は0以上の有限の数でなければなりません。");
            }

            if (!float.IsFinite(reductionCap) ||
                reductionCap < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(reductionCap),
                    "攻撃力減少上限は0以上の有限値で指定してください。");
            }

            _reductionCap = reductionCap;
            _reductionAmount = Mathf.Min(reductionAmount, reductionCap);
        }

        /// <summary> 状態効果の識別子。 </summary>
        public static StatusEffectId EffectId => EFFECT_ID;

        /// <summary> 現在の攻撃力減少量。 </summary>
        public float ReductionAmount => _reductionAmount;

        public void Accumulate(IStatusEffect statusEffect)
        {
            if(statusEffect is not AttackPowerReductionDebuff debuff)
            {
                throw new System.ArgumentException(
                    "異なる種類の状態効果を累積することはできません。",
                    nameof(statusEffect));
            }
            
            _reductionAmount = Mathf.Min(
                _reductionAmount + debuff.ReductionAmount,
                _reductionCap);
        }

        public Damage ModifyAttackPower(IAttacker attacker, IDefender defender, Damage attackPower)
        {
            return attackPower - _reductionAmount;
        }

        private static readonly StatusEffectId EFFECT_ID =
            new("Skill07.AttackPowerReductionDebuff");

        private readonly float _reductionCap;

        private float _reductionAmount;
    }
}
