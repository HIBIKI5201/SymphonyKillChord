using UnityEngine;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     評価ミッション1件分の情報を保持するViewModelクラス。
    /// </summary>
    public class MissionEvaluationItemViewModel
    {
        public MissionEvaluationItemViewModel(string description, bool isAchieved)
        {
            Description = description;
            IsAchieved = isAchieved;
        }

        /// <summary> 表示文。 </summary>
        public string Description { get; }

        /// <summary> 達成済みか。 </summary>
        public bool IsAchieved { get; }
    }
}
