using KillChord.Runtime.Application.InGame.StatusEffect;
using KillChord.Runtime.Application.InGame.Target;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using System;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Buff
{
    /// <summary>
    ///     クリティカルダメージ倍率を一定値に変更するフィールドバフです。
    /// </summary>
    public class CriticalDamageFieldBuff : StatusEffectBase, ICriticalDamageMultiplierModifier
    {
        public CriticalDamageFieldBuff(
            IPlayerTargetRangeQuery rangeQuery,
            float range,
            float criticalDamageMultiplier,
            float durationSeconds,
            StatusEffectReapplyPolicy reapplyPolicy)
            : base(EFFECT_ID,
                  StatusEffectCategory.Buff,
                  StatusEffectDuration.FromSeconds(durationSeconds),
                  reapplyPolicy)
        {
            _rangeQuery = rangeQuery ?? throw new ArgumentNullException(nameof(rangeQuery));

            if (!float.IsFinite(range) ||
                range <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(range),
                    "フィールド範囲は正の有限の値でなければなりません。");
            }

            if (!float.IsFinite(criticalDamageMultiplier) ||
                criticalDamageMultiplier <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(criticalDamageMultiplier),
                    "クリティカルダメージ倍率は正の有限の値でなければなりません。");
            }

            _range = range;
            _criticalDamageMultiplier = criticalDamageMultiplier;
        }

        /// </inheritdoc/>
        public float ModifyCriticalDamageMultiplier(IAttacker attacker, IDefender defender, float criticalDamageMultiplier)
        {
            // 防御者がプレイヤーキャラクターでない場合、クリティカルダメージ倍率を変更しない
            if (defender is not CharacterEntity target)
            {
                return criticalDamageMultiplier;
            }

            // 防御者がプレイヤーキャラクターで、かつ攻撃者がプレイヤーキャラクターの範囲内にいない場合、クリティカルダメージ倍率を変更しない
            if (!_rangeQuery.IsWithinRange(target, _range))
            {
                return criticalDamageMultiplier;
            }

            Debug.Log("[Skill09] クリティカルダメージ倍率を " + _criticalDamageMultiplier + " 倍に変更します。");

            return _criticalDamageMultiplier;
        }

        private static readonly StatusEffectId EFFECT_ID =
            new StatusEffectId("Skill09.CriticalDamageFieldBuff");

        private readonly IPlayerTargetRangeQuery _rangeQuery;
        private readonly float _range;
        private readonly float _criticalDamageMultiplier;
    }
}
