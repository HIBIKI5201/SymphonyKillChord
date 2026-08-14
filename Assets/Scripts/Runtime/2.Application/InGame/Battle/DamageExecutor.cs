using KillChord.Runtime.Domain.InGame.Battle;
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
        ///     ダメージ計算を実行します。
        /// </summary>
        /// <param name="attacker"> 攻撃者です。 </param>
        /// <param name="defender"> 防御者です。 </param>
        /// <param name="attackResult"> 攻撃結果です。 </param>
        /// <param name="attackType"> 攻撃タイプです。 </param>
        /// <returns> 計算結果の攻撃結果です。 </returns>
        public static AttackResult Execute(
            IAttacker attacker,
            IDefender defender,
            AttackResult attackResult,
            DamageAttackType attackType)
        {
            if (attacker == null)
            {
                throw new ArgumentNullException(nameof(attacker));
            }

            if (defender == null)
            {
                throw new ArgumentNullException(nameof(defender));
            }

            // キャラクターのステータス効果によるダメージ修正を適用する
            var result = attacker.StatusEffectSystem.ApplyOutgoingDamageModifiers(
                attacker, defender, attackResult);
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
    }
}
