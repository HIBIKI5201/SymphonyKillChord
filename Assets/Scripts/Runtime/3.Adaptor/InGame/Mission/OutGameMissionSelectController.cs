using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     アウトゲームでミッション選択を行うためのコントローラークラス。
    /// </summary>
    public class OutGameMissionSelectController
    {
        /// <summary>
        ///     OutGameMissionSelectController クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="selectedMissionState">選択されたミッションの状態。</param>
        public OutGameMissionSelectController(SelectedMissionState selectedMissionState)
        {
            _selectedMissionState = selectedMissionState;
        }

        /// <summary>
        ///     ミッションを選択します。
        /// </summary>
        /// <param name="missionIdText">ミッションID文字列。</param>
        public void Select(string missionIdText)
        {
            MissionId missionId = new MissionId(missionIdText);
            _selectedMissionState.SelectMission(missionId);
        }

        /// <summary> 選択されたミッションの状態。 </summary>
        private readonly SelectedMissionState _selectedMissionState;
    }
}
