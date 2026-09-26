using KillChord.Runtime.Application.InGame.StatusEffect;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using KillChord.Runtime.Utility.Persistent;
using System;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Buff
{
    /// <summary>
    ///     攻撃時に与えたダメージの一部を回復するライフスティールバフです。
    /// </summary>
    public class LifeStealBuff : StatusEffectBase, IDamageDealtHandler
    {
        public LifeStealBuff(
            CharacterEntity owner,
            float lifeStealRate,
            float maxHealPerHit,
            float durationSeconds,
            StatusEffectReapplyPolicy reapplyPolicy)
            : base(
                EFFECT_ID,
                StatusEffectCategory.Buff,
                StatusEffectDuration.FromSeconds(durationSeconds),
                reapplyPolicy)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));

            if (!float.IsFinite(lifeStealRate) || lifeStealRate < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(lifeStealRate),
                    "ライフスティール率は0以上の有限の数でなければなりません。");
            }

            if (!float.IsFinite(maxHealPerHit) || maxHealPerHit < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxHealPerHit),
                    "1回の攻撃で回復できる最大値は0以上の有限の数でなければなりません。");
            }

            _lifeStealRate = lifeStealRate;
            _maxHealPerHit = maxHealPerHit;
        }

        /// <inheritdoc />
        public void OnDamageDealt(in DamageDealtContext context)
        {
            if (!ReferenceEquals(context.Attacker, _owner))
            {
                return;
            }

            if (context.AttackType == DamageAttackType.Infection)
            {
                return;
            }

            float appliedDamage = context.AttackResult.AppliedDamage.Value;

            if (appliedDamage <= 0f)
            {
                return;
            }

            float healAmount = Mathf.Min(appliedDamage * _lifeStealRate, _maxHealPerHit);

            if (healAmount <= 0f)
            {
                return;
            }

            _owner.Heal(new Health(healAmount));

#if UNITY_EDITOR
            Debug.Log("[LifeStealBuff] " + _owner.Name + "が" + healAmount + "回復しました。");
#endif
        }

        private static readonly StatusEffectId EFFECT_ID =
            new("Skill06.LifeSteal");

        private readonly CharacterEntity _owner;
        private readonly float _lifeStealRate;
        private readonly float _maxHealPerHit;
    }
}
