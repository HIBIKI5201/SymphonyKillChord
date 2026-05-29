using KillChord.Runtime.Adaptor.InGame.Enemy;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     敵のAttackControllerを生成するインタフェース。
    /// </summary>
    public interface IEnemyAttackControllerGenerator
    {
        /// <summary>
        ///     敵の攻撃コントローラーを生成する。
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public IEnemyAttackController Generate(EnemyAttackControllerContext ctx);
    }
}
