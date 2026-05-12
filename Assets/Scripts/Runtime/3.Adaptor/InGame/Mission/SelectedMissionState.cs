using KillChord.Runtime.Domain.InGame.Mission;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     アウトゲーム側で選択されているミッションの状態を管理するクラス。
    /// </summary>
    public class SelectedMissionState
    {
        /// <summary> 現在選択されているミッションIDを取得します。 </summary>
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

        /// <summary> ミッションが選択されているかどうかを取得します。 </summary>
        public bool HasSelectedMission { get; private set; }

        /// <summary>
        ///     ミッションを選択します。
        /// </summary>
        /// <param name="missionId">選択するミッションID。</param>
        public void SelectMission(MissionId missionId)
        {
            _currentMissionId = missionId;
            HasSelectedMission = true;
        }

        /// <summary> 現在選択されているミッションID。 </summary>
        private MissionId _currentMissionId;
    }
}
