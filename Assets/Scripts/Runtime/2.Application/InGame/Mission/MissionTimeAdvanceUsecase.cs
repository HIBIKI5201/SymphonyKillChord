using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Application.InGame.Mission
{
    /// <summary>
    ///     経過時間を進めるユースケースクラス。
    /// </summary>
    public class MissionTimeAdvanceUsecase
    {
        /// <summary>
        ///     ユースケースを実行します。
        /// </summary>
        /// <param name="progress">進行状況。</param>
        /// <param name="deltaTime">経過時間。</param>
        public void Execute(MissionProgress progress, float deltaTime)
        {
            progress.AdvanceTime(deltaTime);
        }
    }
}
