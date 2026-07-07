using KillChord.Runtime.Application.InGame.Enemy;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     特殊攻撃1（3方向攻撃）用の攻撃コントローラー。
    /// </summary>
    public sealed class EnemyTripleShotAttackController : IEnemyAttackController
    {
        public EnemyTripleShotAttackController(
            EnemyTripleShotAttackUsecase enemyAttackUsecase,
            EnemyBattleState enemyBattleState)
        {
            _enemyAttackUsecase = enemyAttackUsecase;
            _enemyBattleState = enemyBattleState;
        }

        /// <summary>
        ///     三方向に攻撃を実行する。
        /// </summary>
        public void ExecuteAttack()
        {
            _enemyAttackUsecase.ExecuteAttack(
                _enemyBattleState.CurrentAttack,
                _enemyBattleState.Attacker,
                _enemyBattleState.Target);
        }

        private readonly EnemyTripleShotAttackUsecase _enemyAttackUsecase;
        private readonly EnemyBattleState _enemyBattleState;
    }
}
