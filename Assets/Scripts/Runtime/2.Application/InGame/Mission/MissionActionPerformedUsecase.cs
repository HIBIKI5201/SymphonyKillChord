using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Application.InGame.Mission
{
    /// <summary>
    ///     プレイヤー行動の発動を記録するユースケース。
    /// </summary>
    public class MissionActionPerformedUsecase
    {
        /// <summary>
        ///     ユースケースを実行します。
        /// </summary>
        /// <param name="progress">進行状況。</param>
        /// <param name="actionKind">発動した行動の種別。</param>
        public void Execute(MissionProgress progress, MissionActionKind actionKind)
        {
            if (progress.IsFinished)
            {
                return;
            }
            progress.RecordActionPerformed(actionKind);
        }
    }
}
