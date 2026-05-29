namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     砲弾を生成するインタフェース。
    /// </summary>
    public interface IShellSpawner
    {
        /// <summary>
        ///     砲弾を生成する。
        /// </summary>
        /// <param name="enemyBattleState"></param>
        public void SpawnShell(EnemyBattleState enemyBattleState);
    }
}
