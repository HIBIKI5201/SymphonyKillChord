
using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Battle;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     砲弾のライフサイクルを管理するインターフェース。
    /// </summary>
    public interface IShellLifeCycle
    {
        /// <summary>
        ///     有効化処理。
        /// </summary>
        /// <param name="enemyBattleState"></param>
        public void Activate(EnemyBattleState enemyBattleState);
    }
}
