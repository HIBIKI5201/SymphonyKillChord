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
        /// <returns></returns>
        public static AttackResult Calculate(
            AttackDefinition attackDefinition,
            IAttacker attacker,
            IDefender defender,
            bool canAttackHit)
        {
            if (!canAttackHit)
            {
                Debug.Log("[Attack]障害物あり／対象が射程外のため、攻撃が無効。");
                return new AttackResult(new Damage(0), false);
            }
            AttackStepContext stepContext = new AttackStepContext(attackDefinition, attacker, defender);
            return attackDefinition.AttackPipeline.Execute(stepContext);
        }
    }
}
