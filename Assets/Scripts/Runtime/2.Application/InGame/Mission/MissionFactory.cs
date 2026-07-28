using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Application.InGame.Mission
{
    /// <summary>
    ///     ミッション関連のオブジェクトを生成するファクトリクラス。
    /// </summary>
    public class MissionFactory
    {
        /// <summary>
        ///     ミッション進行状況オブジェクトを生成します。
        /// </summary>
        /// <returns>進行状況オブジェクト。</returns>
        public MissionProgress CreateMissionProgress()
        {
            return new MissionProgress();
        }
    }
}
