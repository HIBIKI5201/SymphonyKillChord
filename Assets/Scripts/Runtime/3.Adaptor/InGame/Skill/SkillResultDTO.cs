using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     スキルの結果を表すデータ転送オブジェクト（DTO）。
    /// </summary>
    public readonly struct SkillResultDTO
    {
        public SkillResultDTO(int skillId, int[] skillPattern)
        {
            SkillId = skillId;
            SkillPattern = skillPattern;
        }

        public int SkillId { get; }

        public int[] SkillPattern { get; }
    }
}
