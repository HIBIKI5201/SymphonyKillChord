using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.SkillEffect;
using KillChord.Runtime.Application.InGame.Target;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using KillChord.Runtime.Domain.Player;
using System;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルID 08 のスキル効果を実装するクラス。 
    /// </summary>
    public class Skill_08 : SkillBase
    {
        /// <summary>
        ///     次の攻撃への効果予約サービスと範囲判定を指定して生成する。
        /// </summary>
        public Skill_08(
            PendingAttackEffectService pendingAttackEffectService,
            ITargetRadiusQuery targetRadiusQuery)
        {
            _pendingAttackEffectService = pendingAttackEffectService ?? throw new System.ArgumentNullException(nameof(pendingAttackEffectService));
            _targetRadiusQuery = targetRadiusQuery ?? throw new System.ArgumentNullException(nameof(targetRadiusQuery));
        }

        /// <summary>
        ///     次の攻撃が当たったときに感染を広げる効果を予約する。
        /// </summary>
        public override void Execute(in SkillEffectContext context)
        {
            // 感染の範囲・発動回数・ダメージ率と、現在の拍に対応する攻撃定義を取得する。
            float infectionRange =
                (float)context.EffectSpec.GetRequiredValue(SkillEffectParameterId.InfectionRange);

            int infectionTriggerCount =
                (int)context.EffectSpec.GetRequiredValue(SkillEffectParameterId.InfectionTriggerCount);

            float infectionDamageRate =
                (float)context.EffectSpec.GetRequiredValue(SkillEffectParameterId.InfectionDamageRate);

            AttackDefinition attackDefinition =
                context.PlayerEntity.CombatSpec.GetAttackDefinitionByBeatType(context.CurrentBeatType);

            // パラメーターが有効な範囲にあるかを検証する。
            ValidateParameters(
                infectionRange,
                infectionTriggerCount,
                infectionDamageRate,
                context.EffectSpec.ReapplyPolicy);

            // 次の攻撃が当たったときに感染を広げる効果を予約する。
            _pendingAttackEffectService.Register(
                new InfectionOnHitEffect(
                    _targetRadiusQuery,
                    attackDefinition,
                    infectionRange,
                    infectionTriggerCount,
                    infectionDamageRate,
                    context.EffectSpec.ReapplyPolicy));

            Debug.Log($"[Skill_08] 発動" +
                $"Range: {infectionRange}, TriggerCount: {infectionTriggerCount}, DamageRate: {infectionDamageRate}");
        }

        private readonly PendingAttackEffectService _pendingAttackEffectService;
        private readonly ITargetRadiusQuery _targetRadiusQuery;

        /// <summary>
        ///     スキルパラメータを検証します。
        /// </summary>
        private static void ValidateParameters(
            float infectionRange,
            int infectionTriggerCount,
            float infectionDamageRate,
            StatusEffectReapplyPolicy reapplyPolicy)
        {
            if (!float.IsFinite(infectionRange) ||
                infectionRange <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(infectionRange),
                    "伝染範囲は0より大きい有限値で指定してください。");
            }

            if (infectionTriggerCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(infectionTriggerCount),
                    "伝染回数は1以上で指定してください。");
            }

            if (!float.IsFinite(infectionDamageRate) ||
                infectionDamageRate <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(infectionDamageRate),
                    "伝染ダメージ率は0より大きい有限値で指定してください。");
            }

            if (reapplyPolicy !=
                StatusEffectReapplyPolicy.Replace)
            {
                throw new InvalidOperationException(
                    $"{nameof(Skill_08)}では" +
                    $"{StatusEffectReapplyPolicy.Replace}を指定してください。");
            }
        }
    }
}
