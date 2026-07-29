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
        /// <param name="ownedPoints"> 所持ポイント。 </param>
        public SkillBuildViewDTO(
            ReadOnlySpan<SkillBuildSlotData> slots,
            ReadOnlySpan<SkillViewData> skills,
            int ownedPoints)
        {
            Slots = slots;
            Skills = skills;
            OwnedPoints = ownedPoints;
        }

        /// <summary> スロット一覧。 </summary>
        public ReadOnlySpan<SkillBuildSlotData> Slots { get; }

        /// <summary> 入手済みスキル表示一覧。 </summary>
        public ReadOnlySpan<SkillViewData> Skills { get; }

        /// <summary> 所持ポイント。 </summary>
        public int OwnedPoints { get; }
    }
}
