using KillChord.Runtime.Application.InGame.StatusEffect;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.StatusEffect;

namespace KillChord.Runtime.Application.InGame.Buff
{
    /// <summary>
    ///     攻撃力を一定割合増加させるバフです。
    /// </summary>
    public class AttackPowerIncreaseBuff
        : StatusEffectBase, IOutgoingDamageModifier
    {
        public AttackPowerIncreaseBuff(
            float increaseRate,
            float durationSeconds,
            StatusEffectReapplyPolicy reapplyPolicy)
            : base(
                EFFECT_ID,
                StatusEffectCategory.Buff,
                StatusEffectDuration.FromSeconds(durationSeconds),
                reapplyPolicy)
        {
            if (!float.IsFinite(increaseRate) || increaseRate < 0f)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(increaseRate),
                    "攻撃力増加率は0以上の有限の数でなければなりません。");
            }

            _increaseRate = increaseRate;
        }

        ///</inheritdoc/>
        public AttackResult ModifyOutgoingDamage(IAttacker attacker, IDefender defender, AttackResult attackResult)
        {
            Damage damage = attackResult.FinalDamage * (1f + _increaseRate);
            return attackResult.WithFinalDamage(damage);
        }

        private static readonly StatusEffectId EFFECT_ID =
            new("Skill07.AttackPowerIncreaseBuff");

        private readonly float _increaseRate;

    }
}
