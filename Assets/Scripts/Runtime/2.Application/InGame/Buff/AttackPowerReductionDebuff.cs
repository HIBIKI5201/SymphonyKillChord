using KillChord.Runtime.Application.InGame.StatusEffect;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Buff
{
    public class AttackPowerReductionDebuff
        : StatusEffectBase, IOutgoingDamageModifier, IAccumulatingStatusEffect
    {
        public AttackPowerReductionDebuff(
            float reductionRatePerStack,
            int stackCount,
            int maxStackCount,
            float durationInSeconds)
            : base(
                  EFFECT_ID,
                  StatusEffectCategory.Debuff,
                  StatusEffectDuration.FromSeconds(durationInSeconds),
                  StatusEffectReapplyPolicy.RefreshDuration)
        {
            if (!float.IsFinite(reductionRatePerStack) || reductionRatePerStack < 0f)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(reductionRatePerStack),
                    "攻撃力減少率は0以上の有限の数でなければなりません。");
            }

            if (stackCount <= 0)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(stackCount),
                    "スタック数は0以上でなければなりません。");
            }

            if (maxStackCount <= 0)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(maxStackCount),
                    "最大スタック数は0より大きい必要があります。");
            }

            _reductionRatePerStack = reductionRatePerStack;
            _maxStackCount = maxStackCount;
            _stackCount = Mathf.Min(stackCount, maxStackCount);
        }

        /// <summary> 状態効果の識別子。 </summary>
        public static StatusEffectId EffectId => EFFECT_ID;

        /// <summary> スタック数。 </summary>
        public int StackCount => _stackCount;

        /// <summary> 現在の合計攻撃力減少率。 </summary>
        public float TotalReductionRate => Mathf.Clamp01(_reductionRatePerStack * _stackCount);

        public void Accumulate(IStatusEffect statusEffect)
        {
            if(statusEffect is not AttackPowerReductionDebuff debuff)
            {
                throw new System.ArgumentException(
                    "異なる種類の状態効果を累積することはできません。",
                    nameof(statusEffect));
            }
            _stackCount = Mathf.Min(_stackCount + debuff.StackCount, _maxStackCount);
        }

        public AttackResult ModifyOutgoingDamage(IAttacker attacker, IDefender defender, AttackResult attackResult)
        {
            Damage damage = attackResult.FinalDamage * (1f - TotalReductionRate);
            return attackResult.WithFinalDamage(damage);
        }

        private static readonly StatusEffectId EFFECT_ID =
            new("Skill07.AttackPowerReductionDebuff");

        private readonly float _reductionRatePerStack;
        private readonly int _maxStackCount;

        private int _stackCount;
    }
}
