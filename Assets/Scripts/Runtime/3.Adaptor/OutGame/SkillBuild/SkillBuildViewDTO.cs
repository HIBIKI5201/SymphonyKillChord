using System;

namespace KillChord.Runtime.Adaptor.OutGame.SkillBuild
{
    /// <summary>
    ///     スキルの装備スロット全体の表示 DTO。
    /// </summary>
    public readonly ref struct SkillBuildViewDTO
    {
        /// <summary>
        ///     DTO を初期化する。
        /// </summary>
        /// <param name="slots"> スロット一覧。 </param>
        /// <param name="skills"> 入手済みスキル表示一覧。 </param>
        public SkillBuildViewDTO(ReadOnlySpan<SkillBuildSlotDTO> slots, ReadOnlySpan<(int skillId, string label)> skills)
        {
            Slots = slots;
            Skills = skills;
        }

        /// <summary> スロット一覧。 </summary>
        public ReadOnlySpan<SkillBuildSlotDTO> Slots { get; }

        /// <summary> 入手済みスキル表示一覧。 </summary>
        public ReadOnlySpan<(int skillId, string label)> Skills { get; }
    }
}
