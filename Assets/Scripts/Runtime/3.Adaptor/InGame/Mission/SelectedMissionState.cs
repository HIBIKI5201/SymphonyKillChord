using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     OutGame側で選択されているミッションの状態を管理するクラス。
    /// </summary>
    public class SelectedMissionState
    {
        public MissionId CurrentMissionId { get; private set; }

        public void SelectMission(MissionId missionId)
        {
            CurrentMissionId = missionId;
        }
    }
}
