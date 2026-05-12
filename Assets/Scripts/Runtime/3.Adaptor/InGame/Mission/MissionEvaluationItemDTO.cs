using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     評価ミッション1件分の情報を保持するDTO構造体。
    /// </summary>
    public readonly struct MissionEvaluationItemDTO
    {
        /// <summary>
        ///     MissionEvaluationItemDTO 構造体の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="description"> 表示文。 </param>
        /// <param name="isAchieved"> 条件達成しているか。 </param>
        public MissionEvaluationItemDTO(string description, bool isAchieved)
        {
            Description = description;
            IsAchieved = isAchieved;
        }

        /// <summary> 表示文を取得します。 </summary>
        public string Description { get; }

        /// <summary> 条件達成しているかを取得します。 </summary>
        public bool IsAchieved { get; }
    }
}
