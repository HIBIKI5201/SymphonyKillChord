using KillChord.Runtime.Domain.InGame.Battle;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     攻撃の計算を行う静的クラス。
    /// </summary>
    public static class AttackCalculator
    {
        /// <summary>
        ///     計算処理を行い、攻撃の結果を返す。
        /// </summary>
        /// <param name="attackDefinition"></param>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <param name="isOutOfRange"> 対象が射程外にいる場合はtrue。ダメージ減衰の判定に使う。 </param>
        /// <returns> 攻撃結果。 </returns>
        public static AttackResult Calculate(
            AttackDefinition attackDefinition,
            IAttacker attacker,
            IDefender defender,
            bool isJustHit,
            Damage baseDamage,
            bool isOutOfRange = false
            )
        {
            AttackStepContext stepContext = new AttackStepContext(attackDefinition, attacker, defender, isJustHit, baseDamage, isOutOfRange);
            return attackDefinition.AttackPipeline.Execute(stepContext);
        }
    }
}
