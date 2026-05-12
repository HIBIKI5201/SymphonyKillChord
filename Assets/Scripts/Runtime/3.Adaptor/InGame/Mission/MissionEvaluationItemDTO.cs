using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     評価ミッション1件分の情報を保持するDTOクラス。
    /// </summary>
    public readonly struct MissionEvaluationItemDTO
    {
        /// <summary>
        ///     ミッション評価項目DTOのコンストラクタ。
        /// </summary>
        /// <param name="description"> 表示文。 </param>
        /// <param name="isAchieved"> 条件達成しているか。 </param>
        public MissionEvaluationItemDTO(string description, bool isAchieved)
        {
            Description = description;
            IsAchieved = isAchieved;
        }

        /// <summary> 表示文。 </summary>
        public string Description { get; }

        /// <summary> 条件達成しているか。 </summary>
        public bool IsAchieved { get; }
    }
}
