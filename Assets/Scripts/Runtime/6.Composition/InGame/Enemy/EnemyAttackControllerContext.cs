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

        /// <summary> 敵の攻撃ユースケース </summary>
        public EnemyAttackUsecase AttackUsecase => _attackUsecase;
        /// <summary> 敵の戦闘関連状態 </summary>
        public EnemyBattleState BattleState => _battleState;
        /// <summary> 砲弾のスポナー </summary>
        public ShellSpawner ShellSpawner => _shellSpawner;

        private readonly EnemyAttackUsecase _attackUsecase;
        private readonly EnemyBattleState _battleState;
        private readonly ShellSpawner _shellSpawner;
    }
}
