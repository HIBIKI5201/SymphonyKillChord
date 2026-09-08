using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Utility.Persistent;
using System;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     ダメージ計算を実行するユーティリティクラス。
    /// </summary>
    public static class DamageExecutor
    {
        /// <summary>
        ///     通常のダメージ計算を実行します。
        /// </summary>
        /// <param name="attacker"> 攻撃者です。 </param>
        /// <param name="defender"> 防御者です。 </param>
        /// <param name="attackResult"> 攻撃結果です。 </param>
        /// <param name="attackType"> 攻撃タイプです。 </param>
        /// <param name="notifyNormalDamage"> 通常ダメージを通知するかどうかを示す値です。 </param>
        /// <returns> 計算結果の攻撃結果です。 </returns>
        public static AttackResult Execute(
            IAttacker attacker,
            IDefender defender,
            AttackResult attackResult,
            DamageAttackType attackType,
            bool notifyNormalDamage = false)
        {
            return ExecuteInternal(attacker, defender, attackResult, attackType, true, notifyNormalDamage);
        }

        /// <summary>
        ///     派生ダメージ計算を実行します。
        ///     攻撃者のステータス効果によるダメージ修正は適用されません。
        /// </summary>
        /// <param name="attacker"> 攻撃者です。 </param>
        /// <param name="defender"> 防御者です。 </param>
        /// <param name="attackResult"> 攻撃結果です。 </param>
        /// <param name="attackType"> 攻撃タイプです。 </param>
        /// <returns> 計算結果の攻撃結果です。 </returns>
        public static AttackResult ExecuteDerived(
            IAttacker attacker,
            IDefender defender,
            AttackResult attackResult,
            DamageAttackType attackType)
        {
            return ExecuteInternal(attacker, defender, attackResult, attackType, false, false);
        }

        /// <summary>
        ///     ダメージ計算を実行します。
        /// </summary>
        /// <param name="attacker"> 攻撃者です。 </param>
        /// <param name="defender"> 防御者です。 </param>
        /// <param name="attackResult"> 攻撃結果です。 </param>
        /// <param name="attackType"> 攻撃タイプです。 </param>
        /// <param name="applyOutgoingModifiers"> 攻撃者のステータス効果によるダメージ修正を適用するかどうかを示す値です。 </param>
        /// <returns> 計算結果の攻撃結果です。 </returns>
        public static AttackResult ExecuteInternal(
            IAttacker attacker,
            IDefender defender,
            AttackResult attackResult,
            DamageAttackType attackType,
            bool applyOutgoingModifiers,
            bool notifyNormalDamage)
        {
            if (attacker == null)
            {
                throw new ArgumentNullException(nameof(attacker));
            }

            if (defender == null)
            {
                throw new ArgumentNullException(nameof(defender));
            }

            var result = attackResult;

            // キャラクターのステータス効果によるダメージ修正を適用する
            if (applyOutgoingModifiers)
            {
                result = attacker.StatusEffectSystem.ApplyOutgoingDamageModifiers(
                    attacker, defender, result);
            }

            result = defender.StatusEffectSystem.ApplyIncomingDamageModifiers(
                attacker, defender, result);

            // バリアを持つ防御者の場合、バリアでダメージを吸収する
            Damage damageToHealth = result.FinalDamage;
            Damage barrierDamage = default;

            if (defender.CanTakeDamage &&
                defender is IBarrierHolder barrierHolder)
            {
                damageToHealth = barrierHolder.AbsorbBarrier(result.FinalDamage, out barrierDamage);
            }

            NotifySkillDamage(defender, result, attackType, notifyNormalDamage);
            Damage appliedDamage = default;

            // 防御者がダメージを受けることができる場合、またはダメージが0より大きい場合にのみ、ダメージを適用する
            if (damageToHealth.Value > 0f ||
                !defender.CanTakeDamage)
            {
                appliedDamage = defender.TakeDamage(damageToHealth);
            }

            // 攻撃結果にバリアダメージと適用されたダメージを設定する
            result = result
                .WithBarrierDamage(barrierDamage)
                .WithAppliedDamage(appliedDamage);

            // 攻撃者と防御者のステータス効果システムにダメージを通知する
            defender.StatusEffectSystem.NotifyDamageTaken(
                new DamageTakenContext(
                    attacker,
                    defender,
                    result,
                    attackType));
            attacker.StatusEffectSystem.NotifyDamageDealt(
                new DamageDealtContext(
                    attacker,
                    defender,
                    result,
                    attackType));

            return result;
        }

        /// <summary>
        ///     スキルダメージを通知します。
        /// </summary>
        /// <param name="defender"> ダメージを受ける防御者です。 </param>
        /// <param name="attackResult"> 攻撃結果です。 </param>
        /// <param name="attackType"> 攻撃タイプです。 </param>
        /// <param name="notifyNormalDamage"> 通常ダメージを通知するかどうかを示す値です。 </param>
        private static void NotifySkillDamage(
            IDefender defender,
            in AttackResult attackResult,
            DamageAttackType attackType,
            bool notifyNormalDamage)
        {
            // スキルダメージ、感染ダメージ、または通常ダメージで通知が有効な場合にのみ通知する
            bool shouldNotify =
                attackType == DamageAttackType.Skill ||
                attackType == DamageAttackType.Infection ||
                attackType == DamageAttackType.Normal && notifyNormalDamage;

            if (!shouldNotify)
            {
                return;
            }

            if (defender is not CharacterEntity character)
            {
                return;
            }

            EventBus<EOnTakeDamage>.Raise(
                new EOnTakeDamage(
                    attackResult.FinalDamage.Value,
                    attackResult.IsCritical,
                    attackResult.IsJustHit,
                    character.Id,
                    attackType));
        }
    }
}
