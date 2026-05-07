using KillChord.Runtime.Domain.InGame.Music;
using System;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    /// スキル結果を表すデータ転送オブジェクト（DTO）。
    /// </summary>
    public readonly struct SkillResultDTO
    {
        /// <summary>
        /// コンストラクタ。スキルIDとパターン配列からDTOを構築する。
        /// </summary>
        public SkillResultDTO(int skillId, int[] skillPattern)
        {
            SkillId = skillId;
            SkillPattern = skillPattern;
        }

        /// <summary>
        /// スキルID。
        /// </summary>
        public int SkillId { get; }

        /// <summary>
        /// スキル入力パターン（読み取り専用メモリ）。
        /// </summary>
        public ReadOnlyMemory<int> SkillPattern { get; }
    }
}
