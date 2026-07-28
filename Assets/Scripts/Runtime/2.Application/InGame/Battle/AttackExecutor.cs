using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Character;
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
        /// <returns> 攻撃結果。 </returns>
        public static AttackResult Execute(
            AttackDefinition attackDefinition,
            IAttacker attacker,
            IDefender defender,
            bool isJustHit,
            Damage baseDamage
               )
        {
            if (attackDefinition == null)
                throw new ArgumentNullException(nameof(attackDefinition));
            if (attacker == null)
                throw new ArgumentNullException(nameof(attacker));
            if (defender == null)
                throw new ArgumentNullException(nameof(defender));

            CharacterEntity attackerEntity = attacker as CharacterEntity;
            CharacterEntity defenderEntity = defender as CharacterEntity;

            attacker.BuffSystem.Execute(new BuffContext(attackerEntity, defenderEntity), BuffExecuteTiming.Attack_Logic_Before);

            // 計算を行い、ダメージを適用する。
            AttackResult result = AttackCalculator.Calculate(attackDefinition, attacker, defender, isJustHit, baseDamage);

            BuffContext buffContext = new BuffContext(attackerEntity, defenderEntity, result);
            buffContext = attacker.BuffSystem.Execute(buffContext, BuffExecuteTiming.Attack_Logic_After);
            buffContext = defender.BuffSystem.Execute(buffContext, BuffExecuteTiming.Defense_Logic_Before);
            result = buffContext.AttackResult;

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
