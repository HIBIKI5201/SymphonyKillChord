using KillChord.Runtime.Application.InGame.StatusEffect;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using System;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Buff
{
    /// <summary>
    ///     与えたダメージに応じてバリアを獲得するバフです。
    /// </summary>
    public class BarrierGainBuff : StatusEffectBase, IDamageDealtHandler
    {
        public BarrierGainBuff(CharacterEntity owner,
            float barrierGainRate,
            float durationSeconds,
            StatusEffectReapplyPolicy reapplyPolicy)
            : base(
                EFFECT_ID,
                StatusEffectCategory.Buff,
                StatusEffectDuration.FromSeconds(durationSeconds),
                reapplyPolicy)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));

            if (!float.IsFinite(barrierGainRate) || barrierGainRate < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(barrierGainRate),
                    "バリア獲得率は0以上の有限の数でなければなりません。");
            }
            _barrierGainRate = barrierGainRate;
        }

        ///</inheritdoc/>
        public void OnDamageDealt(in DamageDealtContext context)
        {
            if (!ReferenceEquals(context.Attacker, _owner))
            {
                return;
            }

            float appliedDamage = context.AttackResult.AppliedDamage.Value;

            if (appliedDamage <= 0)
            {
                return;
            }

            float barrierAmount = appliedDamage * _barrierGainRate;

            Debug.Log($"[Skill04] バリア獲得: {barrierAmount}");
        }

        private static readonly StatusEffectId EFFECT_ID =
            new("Skill04.BarrierGainBuff");

        private readonly CharacterEntity _owner;
        private readonly float _barrierGainRate;
    }
}
