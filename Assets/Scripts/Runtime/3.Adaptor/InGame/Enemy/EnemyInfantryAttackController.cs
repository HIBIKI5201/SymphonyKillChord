using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Application.InGame.Enemy;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     歩兵の攻撃コントローラー。
    /// </summary>
    public class EnemyInfantryAttackController : IEnemyAttackController
    {
        public EnemyInfantryAttackController(EnemyAttackUsecase enemyAttackUsecase, EnemyBattleState enemyBattleState)
        {
            _enemyAttackUsecase = enemyAttackUsecase;
            _enemyBattleState = enemyBattleState;
        }
        /// <summary>
        ///     攻撃を実行する。
        /// </summary>
        public void ExecuteAttack()
        {
            _enemyAttackUsecase.ExecuteAttack(
                _enemyBattleState.CurrentAttack,
                _enemyBattleState.Attacker,
                _enemyBattleState.Target);
        }
        private EnemyAttackUsecase _enemyAttackUsecase;
        private EnemyBattleState _enemyBattleState;
    }
}
