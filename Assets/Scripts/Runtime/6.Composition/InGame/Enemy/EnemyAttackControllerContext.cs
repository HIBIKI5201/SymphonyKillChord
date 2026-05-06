using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.View.InGame.Enemy;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     敵のAttackControllerを生成するためのコンテキストクラス。
    /// </summary>
    public class EnemyAttackControllerContext
    {
        public EnemyAttackControllerContext(EnemyAttackUsecase attackUsecase, EnemyBattleState battleState, ShellSpawner shellSpawner)
        {
            _attackUsecase = attackUsecase;
            _battleState = battleState;
            _shellSpawner = shellSpawner;
        }

        public EnemyAttackUsecase AttackUsecase => _attackUsecase;
        public EnemyBattleState BattleState => _battleState;
        public ShellSpawner ShellSpawner => _shellSpawner;

        private readonly EnemyAttackUsecase _attackUsecase;
        private readonly EnemyBattleState _battleState;
        private readonly ShellSpawner _shellSpawner;
    }
}
