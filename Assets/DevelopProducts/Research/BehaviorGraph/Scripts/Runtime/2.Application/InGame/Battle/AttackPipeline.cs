using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Battle;
using System;

namespace DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     複数の攻撃処理ステップを順番に実行するクラス。
    /// </summary>
    public class AttackPipeline : IAttackPipeline
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

        public AttackPipeline()
        {
            _attackSteps = new IAttackStep[] { new CriticalStep(), new ConfirmedDamage() };
        }

        /// <summary>
        ///     攻撃処理の文脈を受け取り、各攻撃処理ステップを順番に実行する。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public AttackResult Execute(in AttackStepContext context)
        {
            AttackStepContext currentContext = context;

            for (int i = 0; i < _attackSteps.Length; i++)
            {
                currentContext = _attackSteps[i].Execute(currentContext);
            }

            return new AttackResult(currentContext);
        }

        private readonly IAttackStep[] _attackSteps;
    }
}
