using KillChord.Runtime.Domain.InGame.Battle;
using System;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using SymphonyFrameWork.System.ServiceLocate;
#endif

namespace KillChord.Runtime.Application.InGame.Battle
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

        /// <summary>
        ///     攻撃処理の文脈を受け取り、各攻撃処理ステップを順番に実行し、指定した観測者に記録させる。
        /// </summary>
        /// <param name="context"> 最初のステップに渡す攻撃処理の文脈。 </param>
        /// <param name="tracer"> 実行を記録する観測者。記録しない場合はnull。 </param>
        /// <returns> 攻撃結果。 </returns>
        public AttackResult Execute(in AttackStepContext context, IAttackPipelineTracer tracer)
        {
            AttackStepContext currentContext = context;
            tracer?.RecordAttackStarted(currentContext, _attackSteps.Length);

            for (int i = 0; i < _attackSteps.Length; i++)
            {
                AttackStepContext nextContext = _attackSteps[i].Execute(currentContext);
                tracer?.RecordStepExecuted(i, _attackSteps[i], currentContext, nextContext);
                currentContext = nextContext;
            }

            // ダメージの内部値は丸めない。端数の処理は表示側の責務とする。
            AttackResult result = new AttackResult(currentContext);
            tracer?.RecordAttackCompleted(result);
            return result;
        }

        /// <summary>
        ///     攻撃処理の文脈を受け取り、各攻撃処理ステップを順番に実行する。
        ///     エディタと開発ビルドでは、ServiceLocatorに観測者が登録されていれば実行を記録させる。
        /// </summary>
        /// <param name="context"></param>
        /// <returns> 攻撃結果。 </returns>
        public AttackResult Execute(in AttackStepContext context)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // 観測者はデバッグ用のウィンドウが開いている間だけ登録される。
            if (ServiceLocator.TryGetInstance(out IAttackPipelineTracer tracer))
            {
                return Execute(context, tracer);
            }
#endif
            return Execute(context, null);
        }

        private readonly IAttackStep[] _attackSteps;
    }
}
