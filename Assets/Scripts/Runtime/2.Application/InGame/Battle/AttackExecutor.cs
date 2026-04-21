using KillChord.Runtime.Domain.InGame.Battle;
using System;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     攻撃を実行するクラス。
    ///     攻撃の計算とダメージの適用を行う。
    /// </summary>
    public static class AttackExecutor
    {
        /// <summary>
        ///     攻撃を実行する。
        /// </summary>
        /// <param name="attackDefinition"></param>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <returns></returns>
        public static AttackResult Execute(
            AttackDefinition attackDefinition,
            IAttacker attacker,
            IDefender defender,
            bool canAttackHit
            )
        {
            if (attackDefinition == null)
                throw new ArgumentNullException(nameof(attackDefinition));
            if (attacker == null)
                throw new ArgumentNullException(nameof(attacker));
            if (defender == null)
                throw new ArgumentNullException(nameof(defender));
            // 計算を行い、ダメージを適用する。
            AttackResult result = AttackCalculator.Calculate(attackDefinition, attacker, defender, canAttackHit);

            // 攻撃が命中しない場合、ダメージを適用せずに結果を返す
            if (!canAttackHit)
            {
                return result;
            }

            defender.TakeDamage(result.FinalDamage);

            Debug.Log(
                 $"[Attack] " +
                 $"AttackName:{attackDefinition.AttackName} " +
                 $"Damage:{result.FinalDamage.Value} " +
                 $"Critical:{result.IsCritical}");

            return result;
        }
    }
}
