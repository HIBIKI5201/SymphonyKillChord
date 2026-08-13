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

            var result = attacker.StatusEffectSystem.ApplyOutgoingDamageModifiers(
                attacker, defender, attackResult);

            result = defender.StatusEffectSystem.ApplyIncomingDamageModifiers(
                attacker, defender, result);

            Damage appliedDamage = defender.TakeDamage(result.FinalDamage);
            result = result.WithAppliedDamage(appliedDamage);

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
