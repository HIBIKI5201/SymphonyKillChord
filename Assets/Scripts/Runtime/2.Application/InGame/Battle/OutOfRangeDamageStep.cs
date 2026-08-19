using KillChord.Runtime.Domain.InGame.Battle;
using System;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     射程外の対象へ命中したときにダメージを減衰させる攻撃処理ステップ。
    ///     減衰は計算式の最後に掛けるため、パイプラインの末尾に配置すること。
    /// </summary>
    [Serializable]
    public class OutOfRangeDamageStep : IAttackStep
    {
        /// <summary>
        ///     攻撃処理ステップを実行する。
        /// </summary>
        /// <param name="context"> 攻撃処理の文脈。 </param>
        /// <returns> 減衰を適用した攻撃処理の文脈。 </returns>
        public AttackStepContext Execute(in AttackStepContext context)
        {
            if (!context.IsOutOfRange)
            {
                return context;
            }

            float resultDamage = context.Damage.Value * context.AttackDefinition.OutOfRangeDamageMultiplier;

            return new AttackStepContext(new Damage(resultDamage), context.CriticalCount, context);
        }
    }
}
