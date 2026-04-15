using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     スキルの結果を表すデータ転送オブジェクト（DTO）。
    /// </summary>
    public readonly struct SkillResultDTO
    {
        public SkillResultDTO(int skillId, int[] skillPattern, string skillName)
        {
            SkillId = skillId;
            SkillPattern = skillPattern;
            SkillName = skillName;
        }

        public int SkillId { get; }

        public int[] SkillPattern { get; }

        public string SkillName { get; }
    }
}
