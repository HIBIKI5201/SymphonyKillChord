using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Battle;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Battle
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
            IDefender defender)
        {
            AttackStepContext stepContext = new AttackStepContext(attackDefinition, attacker, defender);
            return attackDefinition.AttackPipeline.Execute(stepContext);
        }
    }
}
