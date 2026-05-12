using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Application.InGame.Mission
{
    /// <summary>
    ///     プレイヤーの死亡イベントを処理するユースケース。
    /// </summary>
    public class MissionPlayerDeadUsecase
    {
        public void Execute(MissionProgress progress)
        {
            if (progress.IsFinished)
            {
                return;
            }
            progress.MarkPlayerDead();
        }
    }
}
