namespace KillChord.Runtime.Adaptor.OutGame.SkillBuild
{
    /// <summary>
    ///     スキルの装備スロットの表示 DTO。
    /// </summary>
    public readonly struct SkillBuildSlotDTO
    {
        /// <summary>
        ///     DTO を初期化する。
        /// </summary>
        /// <param name="slotIndex"> スロット番号。 </param>
        /// <param name="skillId"> スキル ID。 </param>
        /// <param name="skillLabel"> 表示ラベル。 </param>
        public SkillBuildSlotDTO(int slotIndex, int skillId, string skillLabel)
        {
            SlotIndex = slotIndex;
            SkillId = skillId;
            SkillLabel = skillLabel;
        }

        /// <summary> スロット番号。 </summary>
        public int SlotIndex { get; }

        /// <summary> スキル ID。 </summary>
        public int SkillId { get; }

        /// <summary> 表示ラベル。 </summary>
        public string SkillLabel { get; }
    }
}
