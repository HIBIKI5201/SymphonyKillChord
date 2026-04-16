using KillChord.Runtime.Domain;
using KillChord.Runtime.Domain.InGame.Music;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     スキルの結果を表すデータ転送オブジェクト（DTO）。
    /// </summary>
    public readonly struct SkillResultDTO
    {
        public SkillResultDTO(int skillId, ReadOnlySpan<BeatType> skillPattern)
        {
            SkillId = skillId;
            SkillPattern = skillPattern.ToArray();
        }

        public int SkillId { get; }

        //非同期処理でも使えるため、ReadOnlyMemoryを使用している。
        public ReadOnlyMemory<BeatType> SkillPattern { get; }
    }
}
