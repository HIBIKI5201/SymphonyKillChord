using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Enemy;

namespace DevelopProducts.Boss
{
    /// <summary>
    ///     ボスの攻撃パターン1種をまとめた束。
    ///     攻撃定義・発動タイミング・実行Controllerをセットで保持する。
    ///     攻撃定義は専用BattleStateに固定して保持する
    ///     （実行時の SetCurrentAttack 差し替えは行わない）。
    /// </summary>
    public sealed class BossAttackPattern
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="definition"> 攻撃定義（ダメージ・パイプライン等）。 </param>
        /// <param name="timing"> 発動する音楽同期タイミング（拍子・拍目）。 </param>
        /// <param name="controller"> 実際に攻撃を実行するController。 </param>
        public BossAttackPattern(
            AttackDefinition definition,
            EnemyMusicSpec timing,
            IEnemyAttackController controller)
        {
            Definition = definition;
            Timing = timing;
            Controller = controller;
        }

        /// <summary> 攻撃定義。 </summary>
        public AttackDefinition Definition { get; }

        /// <summary>
        ///     発動タイミング。
        ///     通常1=4拍子3拍目 / 通常2=3拍子2拍目 / 特殊1=2拍子1拍目 を想定。
        /// </summary>
        public EnemyMusicSpec Timing { get; }

        /// <summary> この攻撃を実行するController。 </summary>
        public IEnemyAttackController Controller { get; }
    }
}
