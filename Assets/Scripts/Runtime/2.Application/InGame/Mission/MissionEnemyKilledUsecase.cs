using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Application.InGame.Mission
{
    /// <summary>
    ///     敵撃破を記録するユースケース。
    /// </summary>
    public class MissionEnemyKilledUsecase
    {
        /// <summary>
        ///     ユースケースを実行します。
        /// </summary>
        /// <param name="progress">進行状況。</param>
        /// <param name="enemyKey">敵のキー。</param>
        public void Execute(MissionProgress progress, EnemyMissionKey enemyKey)
        {
            if (progress.IsFinished)
            {
                return;
            }
            progress.EnemyKillRecord.RecordKill(enemyKey);
        }
    }
}
