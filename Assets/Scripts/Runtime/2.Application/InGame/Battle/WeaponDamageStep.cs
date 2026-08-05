using KillChord.Runtime.Domain.InGame.Battle;
using System;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     武器ごとのダメージ倍率を適用する攻撃処理ステップ。
    ///     拍子ごとの倍率は攻撃定義データが持つため、このステップは拍子の数値を参照しない。
    /// </summary>
    [Serializable]
    public class WeaponDamageStep : IAttackStep
    {
        /// <summary>
        ///     攻撃処理ステップを実行する。
        /// </summary>
        /// <param name="context"> 攻撃処理の文脈。 </param>
        /// <returns> 倍率を適用した攻撃処理の文脈。 </returns>
        public AttackStepContext Execute(in AttackStepContext context)
        {
            float resultDamage = context.Damage.Value * context.AttackDefinition.WeaponDamageMultiplier;

            if (context.IsJustHit)
            {
                resultDamage *= context.AttackDefinition.JustDamageMultiplier;
            }

            return new AttackStepContext(new Damage(resultDamage), context.CriticalCount, context);
        }
    }
}
