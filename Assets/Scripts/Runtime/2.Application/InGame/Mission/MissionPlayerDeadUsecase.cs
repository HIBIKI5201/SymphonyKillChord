using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Application.InGame.Mission
{
    /// <summary>
    ///     プレイヤーの死亡イベントを処理するユースケース。
    /// </summary>
    public class MissionPlayerDeadUsecase
    {
        /// <summary>
        ///     ユースケースを実行します。
        /// </summary>
        /// <param name="progress">進行状況。</param>
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
