using KillChord.Runtime.Domain.InGame.Music;
using System;

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

        //非同期処理でも使えるため、ReadOnlyMemoryを使用している。
        public ReadOnlyMemory<int> SkillPattern { get; }
    }
}
