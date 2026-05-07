using KillChord.Runtime.Adaptor.InGame.Enemy;
using System;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     歩兵のAttackControllerを生成するクラス。
    /// </summary>
    public class EnemyInfantryAttackControllerGenerator : IEnemyAttackControllerGenerator
    {
        /// <summary>
        ///     歩兵の攻撃コントローラーを生成する。
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IEnemyAttackController Generate(EnemyAttackControllerContext ctx)
        {
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx), "生成コンテキストが設定されていません。");

            if (ctx.AttackUsecase == null)
                throw new ArgumentNullException(nameof(ctx.AttackUsecase), "AttackUsecaseが設定されていません。");

            if (ctx.BattleState == null)
                throw new ArgumentNullException(nameof(ctx.BattleState), "BattleStateが設定されていません。");

            return new EnemyInfantryAttackController(ctx.AttackUsecase, ctx.BattleState);
        }
    }
}
