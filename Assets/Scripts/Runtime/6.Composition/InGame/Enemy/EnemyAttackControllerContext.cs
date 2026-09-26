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
        /// <summary>
        ///     攻撃コントローラーの生成に必要なユースケース・状態・スポナーを指定して生成する。
        /// </summary>
        public EnemyAttackControllerContext(EnemyAttackUseCase attackUsecase, EnemyTripleShotAttackUseCase tripleShotAttackUsecase, EnemyBattleState battleState, ShellSpawner shellSpawner)
        {
            _attackUsecase = attackUsecase;
            _tripleShotAttackUsecase = tripleShotAttackUsecase;
            _battleState = battleState;
            _shellSpawner = shellSpawner;
        }

        /// <summary> 敵の攻撃ユースケース </summary>
        public EnemyAttackUseCase AttackUsecase => _attackUsecase;
        /// <summary> 敵の攻撃ユースケース </summary>
        public EnemyTripleShotAttackUseCase TripleShotAttackUsecase => _tripleShotAttackUsecase;
        /// <summary> 敵の戦闘関連状態 </summary>
        public EnemyBattleState BattleState => _battleState;
        /// <summary> 砲弾のスポナー </summary>
        public ShellSpawner ShellSpawner => _shellSpawner;

        private readonly EnemyTripleShotAttackUseCase _tripleShotAttackUsecase;
        private readonly EnemyAttackUseCase _attackUsecase;
        private readonly EnemyBattleState _battleState;
        private readonly ShellSpawner _shellSpawner;
    }
}
