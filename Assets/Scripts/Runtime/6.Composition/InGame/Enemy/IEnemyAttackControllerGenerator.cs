using KillChord.Runtime.Adaptor.InGame.Enemy;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     敵のAttackControllerを生成するインタフェース。
    /// </summary>
    public interface IEnemyAttackControllerGenerator
    {
        public IEnemyAttackController Generate(EnemyAttackControllerContext ctx);
    }
}
