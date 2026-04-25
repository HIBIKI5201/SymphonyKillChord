using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Application.InGame.Mission
{
    /// <summary>
    ///     ミッション定義取得リポジトリのインターフェース。
    /// </summary>
    public interface IMissionDefinitionRepository
    {
        MissionDefinition Get(MissionId missionId);
    }
}
