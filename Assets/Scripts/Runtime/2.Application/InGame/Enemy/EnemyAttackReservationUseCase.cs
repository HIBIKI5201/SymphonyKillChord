using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Enemy;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     雑魚敵の攻撃を予約するユースケース。
    ///     予約処理は AttackReservationUseCase に任せ、遭遇時・戦闘時のタイミング選択だけを持つ。
    /// </summary>
    public class EnemyAttackReservationUseCase : AttackReservationUseCase
    {
        /// <summary>
        ///     敵の攻撃を予約するユースケースのコンストラクタ。
        /// </summary>
        /// <param name="enemyAttackMusicSpec"> 遭遇時・戦闘時の攻撃タイミング。 </param>
        /// <param name="musicActionScheduler"> 拍の予約に使うスケジューラー。 </param>
        public EnemyAttackReservationUseCase(
            EnemyAttackMusicSpec enemyAttackMusicSpec,
            IMusicActionScheduler musicActionScheduler
            ) : base(musicActionScheduler)
        {
            _enemyAttackMusicSpec = enemyAttackMusicSpec;
        }

        /// <summary>
        ///     Encounterタイミングで攻撃を予約する。
        /// </summary>
        public void ReserveEncounter()
        {
            Reserve(_enemyAttackMusicSpec.EncounterTiming);
        }

        /// <summary>
        ///     Battleタイミングで攻撃を予約する。
        /// </summary>
        public void ReserveBattle()
        {
            Reserve(_enemyAttackMusicSpec.BattleTiming);
        }

        private readonly EnemyAttackMusicSpec _enemyAttackMusicSpec;
    }
}
