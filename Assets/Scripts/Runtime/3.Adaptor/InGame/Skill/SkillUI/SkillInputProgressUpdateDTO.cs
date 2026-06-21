using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     入力進捗を更新するための情報を保持するDTO。
    /// </summary>
    public readonly ref struct SkillInputProgressUpdateDTO
    {
        public SkillInputProgressUpdateDTO(int patternMatchCount, float currentTimestamp, float skillReadyTimestamp, bool skillTriggeredFlg)
        {
            PatternMatchCount = patternMatchCount;
            CurrentTimestamp = currentTimestamp;
            SkillReadyTimestamp = skillReadyTimestamp;
            SkillTriggeredFlg = skillTriggeredFlg;
        }
        
        /// <summary> マッチしている入力拍子数 </summary>
        public readonly int PatternMatchCount;
        /// <summary> 更新当時の時間 </summary>
        public readonly float CurrentTimestamp;
        /// <summary> クールダウン終了時間 </summary>
        public readonly float SkillReadyTimestamp;
        /// <summary> スキル発動したか </summary>
        public readonly bool SkillTriggeredFlg;
    }
}
