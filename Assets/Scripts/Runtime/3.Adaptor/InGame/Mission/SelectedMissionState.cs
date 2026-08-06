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
                    throw new InvalidOperationException("ミッションが選択されていません。");
                }

                return _currentMissionId.Value;
            }
        }

        /// <summary> ミッションが選択されているかどうかを取得します。 </summary>
        public bool HasSelectedMission
            => _currentMissionId.HasValue;

        /// <summary>
        ///     ミッションを選択します。
        /// </summary>
        /// <param name="missionId"> 選択するミッションID。 </param>
        public void SelectMission(MissionId missionId)
        {
            _currentMissionId = missionId;
        }

        /// <summary>
        ///     選択情報をクリアします。
        /// </summary>
        public void Clear()
        {
            _currentMissionId = null;
        }

        /// <summary> 現在選択されているミッションID。 </summary>
        private MissionId? _currentMissionId;
    }
}
