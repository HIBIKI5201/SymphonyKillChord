using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Application.InGame.Mission
{
    /// <summary>
    ///     敵撃破を記録するユースケース。
    /// </summary>
    public class MissionEnemyKilledUsecase
    {
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
