using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Application.InGame.Mission
{
    /// <summary>
    ///     ミッション定義取得リポジトリのインターフェース。
    /// </summary>
    public interface IMissionDefinitionRepository
    {
        /// <summary>
        ///     指定したIDのミッション定義を取得します。
        /// </summary>
        /// <param name="missionId">ミッションID。</param>
        /// <returns>ミッション定義。</returns>
        public MissionDefinition Get(MissionId missionId);
    }
}
