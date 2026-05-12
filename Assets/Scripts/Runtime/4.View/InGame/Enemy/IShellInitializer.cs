using KillChord.Runtime.Adaptor.InGame.Enemy;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     砲弾初期化処理のインタフェース。
    /// </summary>
    public interface IShellInitializer
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="shellView"></param>
        /// <param name="enemyBattleState"></param>
        /// <param name="enemyMoveView"></param>
        public void Initialize(ShellView shellView, EnemyBattleState enemyBattleState, EnemyMoveView enemyMoveView);
    }
}
