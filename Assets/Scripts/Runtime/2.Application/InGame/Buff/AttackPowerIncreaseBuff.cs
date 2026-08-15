using KillChord.Runtime.Application.InGame.StatusEffect;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.StatusEffect;

namespace KillChord.Runtime.Application.InGame.Buff
{
    /// <summary>
    ///     攻撃力を一定量増加させるバフです。
    /// </summary>
    public class AttackPowerIncreaseBuff
        : StatusEffectBase, IAttackPowerModifier
    {
        public AttackPowerIncreaseBuff(
            float increaseAmount,
            float durationSeconds)
            : base(
                EFFECT_ID,
                StatusEffectCategory.Buff,
                StatusEffectDuration.FromSeconds(durationSeconds),
                StatusEffectReapplyPolicy.Replace)
        {
            if (!float.IsFinite(increaseAmount) || increaseAmount < 0f)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(increaseAmount),
                    "攻撃力増加量は0以上の有限の数でなければなりません。");
            }

            _increaseAmount = increaseAmount;
        }

        ///</inheritdoc/>
        public Damage ModifyAttackPower(IAttacker attacker, IDefender defender, Damage attackPower)
        {
            return attackPower + _increaseAmount;
        }

        private static readonly StatusEffectId EFFECT_ID =
            new("Skill07.AttackPowerIncreaseBuff");

        private readonly float _increaseAmount;

    }
}
