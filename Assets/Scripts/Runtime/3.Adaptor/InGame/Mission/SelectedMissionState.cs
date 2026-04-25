using KillChord.Runtime.Domain.InGame.Mission;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     OutGame側で選択されているミッションの状態を管理するクラス。
    /// </summary>
    public class SelectedMissionState
    {
        public MissionId CurrentMissionId
        {
            get
            {
                if (!HasSelectedMission)
                {
                    throw new InvalidOperationException("Mission has not been selected.");
                }

                return _currentMissionId;
            }
        }

        public bool HasSelectedMission { get; private set; }

        public void SelectMission(MissionId missionId)
        {
            _currentMissionId = missionId;
            HasSelectedMission = true;
        }

        private MissionId _currentMissionId;
    }
}
