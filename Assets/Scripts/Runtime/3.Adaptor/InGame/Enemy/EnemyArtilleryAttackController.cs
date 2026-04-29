using KillChord.Runtime.Adaptor.InGame.Battle;

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

        public void InjectEnemyAIController(EnemyAIController enemyAIController)
        {
            _enemyAIController = enemyAIController;
        }
        public void ExecuteAttack()
        {
            _shellSpawner.SpawnShell(_enemyAIController);
        }

        private IShellSpawner _shellSpawner;
        private EnemyBattleState _enemyBattleState;
        private EnemyAIController _enemyAIController;
    }
}
