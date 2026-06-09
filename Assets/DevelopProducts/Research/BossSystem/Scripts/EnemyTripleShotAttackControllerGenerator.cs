using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Composition.InGame.Enemy;
using System;

namespace DevelopProducts.Boss
{
    /// <summary>
    ///     特殊攻撃1（三方向直線）のAttackControllerを生成するクラス。
    /// </summary>
    public sealed class EnemyTripleShotAttackControllerGenerator : IEnemyAttackControllerGenerator
    {
        /// <summary>
        ///     三方向攻撃コントローラーを生成する。
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public IEnemyAttackController Generate(EnemyAttackControllerContext ctx)
        {
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx), "生成コンテキストが設定されていません。");

            if (ctx.AttackUsecase == null)
                throw new ArgumentNullException(nameof(ctx.AttackUsecase), "AttackUsecaseが設定されていません。");

            if (ctx.BattleState == null)
                throw new ArgumentNullException(nameof(ctx.BattleState), "BattleStateが設定されていません。");

            return new EnemyTripleShotAttackController(ctx.AttackUsecase, ctx.BattleState);
        }
    }
}
