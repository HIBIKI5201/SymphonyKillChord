using KillChord.Runtime.Domain.InGame.Mission;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     アウトゲーム側で選択されているミッションの状態を管理するクラス。
    /// </summary>
    public class SelectedMissionState
    {
        /// <summary> 現在選択されているミッション定義。 </summary>
        public MissionDefinition CurrentMissionDefinition
        {
            get
            {
                if (!HasSelectedMission)
                {
                    throw new InvalidOperationException("ミッションが選択されていません。");
                }

                return _currentMissionDefinition;
            }
        }

        /// <summary> 現在選択されているミッションIDを取得します。 </summary>
        public MissionId CurrentMissionId =>
            _currentMissionDefinition.MissionId;

        /// <summary> ミッションが選択されているかどうかを取得します。 </summary>
        public bool HasSelectedMission
            => _currentMissionDefinition != null;

        /// <summary>
        ///     ミッションを選択します。
        /// </summary>
        /// <param name="missionDefinition"> 選択するミッション情報。 </param>
        public void SelectMission(MissionDefinition missionDefinition)
        {
            _currentMissionDefinition = missionDefinition;
        }

        /// <summary>
        ///     選択情報をクリアします。
        /// </summary>
        public void Clear()
        {
            _currentMissionDefinition = null;
        }

        /// <summary> 現在選択されているミッション情報。 </summary>
        private MissionDefinition _currentMissionDefinition;
    }
}
