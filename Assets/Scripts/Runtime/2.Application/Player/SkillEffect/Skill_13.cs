using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using KillChord.Runtime.Utility.Persistent;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルID 13 のスキル効果を実装するクラス。
    ///     発動後、一定間隔で連撃を適用する。
    /// </summary>
    public class Skill_13 : SkillBase
    {
        /// <summary>
        ///     連撃の予約先を受け取って初期化する。
        /// </summary>
        /// <param name="hitScheduler"> 連撃を時間差で適用するスケジューラです。 </param>
        public Skill_13(SkillHitScheduler hitScheduler)
        {
            _hitScheduler = hitScheduler;
        }

        /// <summary>
        ///     スキル効果を実行するメソッド。連撃を予約し、以降はスケジューラが適用する。
        /// </summary>
        /// <param name="context">スキル効果の発動に必要な情報をまとめた構造体。</param>
        public override void Execute(in SkillEffectContext context)
        {
            float damageMultiplier = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.DamageMultiplier);

            int attackCount = (int)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.HitCount);

            float criticalMultiplier = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.CriticalMultiplier);

            float hitDelaySeconds = GetOptionalValue(context.EffectSpec, SkillEffectParameterId.HitDelaySeconds);
            float hitIntervalSeconds = GetOptionalValue(context.EffectSpec, SkillEffectParameterId.HitIntervalSeconds);

            AttackDefinition attackDefinition = context.PlayerEntity.CombatSpec
                .GetAttackDefinitionByBeatType(context.CurrentBeatType);

            CharacterEntity playerEntity = context.PlayerEntity;
            CharacterEntity targetEntity = context.TargetEntity;
            bool isJustHit = context.IsJustHit;

            // ダメージのタイミングはマスタデータが持ち、エフェクトの生成可否には依存させない。
            _hitScheduler.Schedule(
                attackCount,
                hitDelaySeconds,
                hitIntervalSeconds,
                hitIndex => ApplyHit(
                    attackDefinition,
                    playerEntity,
                    targetEntity,
                    isJustHit,
                    damageMultiplier,
                    criticalMultiplier,
                    hitIndex));
        }

        /// <summary>
        ///     1ヒット分のダメージを適用する。
        /// </summary>
        /// <param name="attackDefinition"> 使用する攻撃定義です。 </param>
        /// <param name="playerEntity"> 攻撃側のエンティティです。 </param>
        /// <param name="targetEntity"> 対象のエンティティです。 </param>
        /// <param name="isJustHit"> ジャスト入力かどうかです。 </param>
        /// <param name="damageMultiplier"> ダメージ倍率です。 </param>
        /// <param name="criticalMultiplier"> クリティカル倍率です。 </param>
        /// <param name="hitIndex"> 何発目かです。 </param>
        /// <returns> 連撃を継続する場合はtrueです。 </returns>
        private static bool ApplyHit(
            AttackDefinition attackDefinition,
            CharacterEntity playerEntity,
            CharacterEntity targetEntity,
            bool isJustHit,
            float damageMultiplier,
            float criticalMultiplier,
            int hitIndex)
        {
            // 連撃の途中で対象が失われた場合は、以降のヒットを打ち切る。
            if (targetEntity == null || targetEntity.IsDead)
            {
                return false;
            }

            AttackResult result = AttackCalculator.Calculate(
                attackDefinition,
                playerEntity,
                targetEntity,
                isJustHit,
                playerEntity.BaseDamage,
                criticalDamageMultiplierOverride: criticalMultiplier);

            result = result.WithFinalDamage(result.FinalDamage * damageMultiplier);
            result = DamageExecutor.Execute(
                playerEntity,
                targetEntity,
                result,
                DamageAttackType.Skill);

            // DamageExecutorは通知を行わないため、ダメージ表示用のイベントをここで発火する。
            EventBus<EOnTakeDamage>.Raise(
                new EOnTakeDamage(
                    result.FinalDamage.Value,
                    result.IsCritical,
                    targetEntity.Id,
                    DamageAttackType.Skill));

            Debug.Log($"[Skill_13] 発動　{hitIndex}ヒット目" +
                $"[FinalDamage: {result.FinalDamage}" +
                $" AppliedDamage: {result.AppliedDamage}," +
                $"IsCritical: {result.IsCritical}]");

            return !targetEntity.IsDead;
        }

        /// <summary>
        ///     任意パラメータを取得する。未設定の場合は0を返す。
        /// </summary>
        /// <param name="effectSpec"> 参照するスキル効果仕様です。 </param>
        /// <param name="parameterId"> 取得するパラメータIDです。 </param>
        /// <returns> 取得した値です。 </returns>
        private static float GetOptionalValue(SkillEffectSpec effectSpec, SkillEffectParameterId parameterId)
        {
            return effectSpec.TryGetParameter(parameterId, out SkillEffectParameter parameter)
                ? (float)parameter.Value
                : 0f;
        }

        private readonly SkillHitScheduler _hitScheduler;
    }
}
