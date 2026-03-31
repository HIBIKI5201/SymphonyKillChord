using KillChord.Runtime.Domain.InGame.Battle;
using System;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     複数の攻撃処理ステップを順番に実行するクラス。
    /// </summary>
    public class AttackPipeline
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="attackSteps"></param>
        public AttackPipeline(IAttackStep[] attackSteps)
        {
            if (attackSteps == null)
            {
                throw new ArgumentNullException(nameof(attackSteps));
            }

            for (int i = 0; i < attackSteps.Length; i++)
            {
                if (attackSteps[i] == null)
                {
                    throw new ArgumentException($"attackSteps[{i}] is null.", nameof(attackSteps));
                }
            }

            _attackSteps = attackSteps;
        }

        /// <summary>
        ///     攻撃処理の文脈を受け取り、各攻撃処理ステップを順番に実行する。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public AttackResult Execute(AttackContext context)
        {
            for (int i = 0; i < _attackSteps.Length; i++)
            {
                _attackSteps[i].Execute(context);
            }

            return new AttackResult(context.CurrentDamage, context.IsCritical);
        }

        private readonly IAttackStep[] _attackSteps;
    }
}
