using KillChord.Runtime.Domain.InGame.Battle;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     拍数を使ったダメージ計算を行う攻撃処理ステップ。
    /// </summary>
    public class BeatStep : IAttackStep
    {
        /// <summary>
        ///     攻撃処理ステップを実行する。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public AttackStepContext Execute(in AttackStepContext context)
        {
            float beatType = context.AttackDefinition.BeatType.HasValue
                ? (float)context.AttackDefinition.BeatType.Value
                : 0;

            if (beatType == 0)
                return context;

            float resultDamage = context.Damage.Value * beatType;
            return new AttackStepContext(new Damage(resultDamage), context.CriticalCount, context);
        }
    }
}