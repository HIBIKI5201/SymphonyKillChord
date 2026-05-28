using System;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキル1個分の入力進行の情報を表すDTO。
    ///     スキルIDと、スキルの入力パターンに対して、現在どこまで入力が進んでいるかを表すステップの配列を持つ。
    /// </summary>
    public readonly ref struct SkillInputProgressRowDTO
    {
        /// <summary>
        ///     スキル1個分の入力進行の情報を表すDTOを生成する。
        /// </summary>
        /// <param name="skillId"> スキルID。 </param>
        /// <param name="steps"> スキル入力進行のステップの配列。 </param>
        public SkillInputProgressRowDTO(
            int skillId,
            ReadOnlySpan<SkillInputProgressStepDTO> steps)
        {
            SkillId = skillId;
            Steps = steps;
        }

        /// <summary> スキルID。 </summary>
        public int SkillId { get; }

        /// <summary> スキル入力進行のステップの配列。 </summary>
        public ReadOnlySpan<SkillInputProgressStepDTO> Steps { get; }
    }
}
