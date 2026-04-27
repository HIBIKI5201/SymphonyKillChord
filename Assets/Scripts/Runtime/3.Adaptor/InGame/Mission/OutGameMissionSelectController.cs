using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     OutGameでミッション選択を行うためのコントローラークラス。
    /// </summary>
    public class OutGameMissionSelectController
    {
        public OutGameMissionSelectController(SelectedMissionState selectedMissionState)
        {
            _selectedMissionState = selectedMissionState;
        }

        public void Select(string missionIdText)
        {
            MissionId missionId = new MissionId(missionIdText);
            _selectedMissionState.SelectMission(missionId);
        }

        private readonly SelectedMissionState _selectedMissionState;
    }
}
