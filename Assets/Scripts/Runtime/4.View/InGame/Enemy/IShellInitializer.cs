using KillChord.Runtime.Adaptor.InGame.Enemy;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     砲弾初期化処理のインタフェース。
    /// </summary>
    public interface IShellInitializer
    {
        public void Initialize(ShellView shellView, EnemyBattleState enemyBattleState, EnemyMoveView enemyMoveView);
    }
}
