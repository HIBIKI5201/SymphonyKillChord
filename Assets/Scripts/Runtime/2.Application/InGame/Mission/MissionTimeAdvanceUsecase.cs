using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Application.InGame.Mission
{
    /// <summary>
    ///     経過時間を進めるユースケースクラス。
    /// </summary>
    public class MissionTimeAdvanceUsecase
    {
        public void Execute(MissionProgress progress, float deltaTime)
        {
            progress.ElapsedTime.AdvanceTime(deltaTime);
        }
    }
}
