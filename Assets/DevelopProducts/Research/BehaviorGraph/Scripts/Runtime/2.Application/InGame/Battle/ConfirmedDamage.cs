using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Battle;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     確定ダメージを適用する攻撃処理ステップ。
    ///     攻撃のダメージが確定ダメージより小さい場合、確定ダメージに置き換える。
    /// </summary>
    public class ConfirmedDamage : IAttackStep
    {
        public AttackStepContext Execute(in AttackStepContext context)
        {
            Damage confirmedDamage = context.AttackDefinition.AttackParameterSet.ConfirmedDamage;

            if (context.Damage.Value < confirmedDamage.Value)
            {
                Debug.Log($"[ConfirmedDamage] Damage increased from {context.Damage.Value} to {confirmedDamage.Value}.");
                return new AttackStepContext(confirmedDamage, context.CriticalCount, context);
            }

            return context;
        }
    }
}
