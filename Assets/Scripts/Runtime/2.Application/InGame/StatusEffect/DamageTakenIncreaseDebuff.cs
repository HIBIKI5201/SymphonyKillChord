using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.StatusEffect;

namespace KillChord.Runtime.Application.InGame.StatusEffect
{
    /// <summary>
    ///     被ダメージ増加のデバフ状態効果。
    /// </summary>
    public class DamageTakenIncreaseDebuff : StatusEffectBase, IIncomingDamageModifier
    {
        public DamageTakenIncreaseDebuff(
            float increaseRate,
            float durationSeconds,
            StatusEffectReapplyPolicy reapplyPolicy)
            : base(
                EFFECT_ID,
                StatusEffectCategory.Debuff,
                StatusEffectDuration.FromSeconds(durationSeconds),
                reapplyPolicy)
        {
            if (!float.IsFinite(increaseRate) || increaseRate < 0f)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(increaseRate),
                    "被ダメージ増加率は0以上の有限の数でなければなりません。");
            }

            _increaseRate = increaseRate;
        }

        public AttackResult ModifyIncomingDamage(IAttacker attacker, IDefender defender, AttackResult attackResult)
        {
            Damage damage = attackResult.FinalDamage * (1f + _increaseRate);

            return attackResult.WithFinalDamage(damage);
        }

        private static readonly StatusEffectId EFFECT_ID =
            new("Skill02.DamageTakenIncrease");

        private readonly float _increaseRate;
    }
}
