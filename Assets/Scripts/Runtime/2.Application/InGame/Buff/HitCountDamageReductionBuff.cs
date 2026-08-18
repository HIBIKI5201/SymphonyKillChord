using KillChord.Runtime.Application.InGame.StatusEffect;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Buff
{
    /// <summary>
    ///     指定回数まで被ダメージを一定割合軽減するバフです。
    /// </summary>
    public class HitCountDamageReductionBuff :
        StatusEffectBase, IIncomingDamageModifier, IDamageTakenHandler, IConsumableStatusEffect
    {
        public HitCountDamageReductionBuff(
            float reductionRate,
            int hitCount,
            StatusEffectReapplyPolicy reapplyPolicy)
            : base(EFFECT_ID, StatusEffectCategory.Buff, StatusEffectDuration.UntilRemoved, reapplyPolicy)
        {
            if (!float.IsFinite(reductionRate))
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(reductionRate),
                    "被ダメージ軽減率は有限の数でなければなりません。");
            }

            if (hitCount <= 0)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(hitCount),
                    "ヒットカウントは1以上でなければなりません。");
            }

            _reductionRate = Mathf.Clamp01(reductionRate);
            _remainingHitCount = hitCount;
        }

        /// <inheritdoc />
        public bool IsConsumed => _remainingHitCount <= 0;

        /// <inheritdoc />
        public AttackResult ModifyIncomingDamage(
            IAttacker attacker,
            IDefender defender,
            AttackResult attackResult)
        {
            if (IsConsumed)
            {
                return attackResult;
            }

            Damage damage = attackResult.FinalDamage * (1f - _reductionRate);

            Debug.Log($"[Skill10] ダメージ軽減: {_reductionRate * 100f}%、残りヒットカウント: {_remainingHitCount - 1}");

            return attackResult.WithFinalDamage(damage);
        }

        /// <inheritdoc />
        public void OnDamageTaken(in DamageTakenContext context)
        {
            if (IsConsumed)
            {
                return;
            }

            float landedDamage =
                context.AttackResult.AppliedDamage.Value +
                context.AttackResult.BarrierDamage.Value;

            if (landedDamage <= 0f)
            {
                return;
            }

            _remainingHitCount--;
        }

        private static readonly StatusEffectId EFFECT_ID =
            new("Skill10.DamageReduction");

        private readonly float _reductionRate;
        private int _remainingHitCount;
    }
}
