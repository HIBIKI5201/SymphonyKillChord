using KillChord.Runtime.Application.InGame.StatusEffect;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.StatusEffect;

namespace KillChord.Runtime.Application.InGame.Buff
{
    public class AttackPowerReductionDebuff
        : StatusEffectBase, IAttackPowerModifier
    {
        public AttackPowerReductionDebuff(
            float reductionAmount,
            float durationInSeconds)
            : base(
                  EFFECT_ID,
                  StatusEffectCategory.Debuff,
                  StatusEffectDuration.FromSeconds(durationInSeconds),
                  StatusEffectReapplyPolicy.Replace)
        {
            if (!float.IsFinite(reductionAmount) || reductionAmount < 0f)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(reductionAmount),
                    "攻撃力減少率は0以上の有限の数でなければなりません。");
            }

            _reductionAmount = reductionAmount;
        }

        /// <summary> 状態効果の識別子。 </summary>
        public static StatusEffectId EffectId => EFFECT_ID;

        /// <summary> 現在の攻撃力減少量。 </summary>
        public float ReductionAmount => _reductionAmount;

        public Damage ModifyAttackPower(IAttacker attacker, IDefender defender, Damage attackPower)
        {
            return attackPower - _reductionAmount;
        }

        private static readonly StatusEffectId EFFECT_ID =
            new("Skill07.AttackPowerReductionDebuff");

        private float _reductionAmount;
    }
}
