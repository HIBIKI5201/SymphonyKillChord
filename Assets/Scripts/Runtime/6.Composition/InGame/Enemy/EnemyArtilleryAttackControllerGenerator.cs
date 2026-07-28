using KillChord.Runtime.Adaptor.InGame.Enemy;
using System;

namespace KillChord.Runtime.Composition.InGame.Enemy
{   
    /// <summary>
    ///     砲兵のAttackControllerを生成するクラス。
    /// </summary>
    public class EnemyArtilleryAttackControllerGenerator : IEnemyAttackControllerGenerator
    {
        /// <summary>
        ///     砲兵の攻撃コントローラーを生成する。
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IEnemyAttackController Generate(EnemyAttackControllerContext ctx)
        {
            if(ctx == null)
                throw new ArgumentNullException(nameof(ctx), "生成コンテキストが設定されていません。");

            if(ctx.ShellSpawner == null)
                throw new ArgumentNullException(nameof(ctx.ShellSpawner), "ShellSpawnerが設定されていません。");

            if(ctx.BattleState == null)
                throw new ArgumentNullException(nameof(ctx.BattleState), "BattleStateが設定されていません。");

            return new EnemyArtilleryAttackController(ctx.ShellSpawner, ctx.BattleState);
        }
    }
}
