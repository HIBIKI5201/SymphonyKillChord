namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     砲兵の攻撃コントローラー。
    /// </summary>
    public class EnemyArtilleryAttackController : IEnemyAttackController
    {
        public EnemyArtilleryAttackController(IShellSpawner shellSpawner, EnemyBattleState enemyBattleState)
        {
            _shellSpawner = shellSpawner;
            _enemyBattleState = enemyBattleState;
        }

        /// <summary>
        ///     攻撃を実行する。
        /// </summary>
        public void ExecuteAttack()
        {
            _shellSpawner.SpawnShell(_enemyBattleState);
        }

        private IShellSpawner _shellSpawner;
        private EnemyBattleState _enemyBattleState;
    }
}
