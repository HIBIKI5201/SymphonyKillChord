namespace KillChord.Runtime.Adaptor.OutGame.SkillBuild
{
    /// <summary>
    ///     Presenter から転送するスキル装備スロットデータ。
    /// </summary>
    public readonly struct SkillBuildSlotData
    {
        /// <summary>
        ///     スロットデータを初期化する。
        /// </summary>
        /// <param name="slotIndex"> スロット番号。 </param>
        /// <param name="skillId"> スキル ID。 </param>
        public SkillBuildSlotData(int slotIndex, int skillId)
        {
            SlotIndex = slotIndex;
            SkillId = skillId;
        }

        /// <summary> スロット番号。 </summary>
        public int SlotIndex { get; }

        /// <summary> スキル ID。 </summary>
        public int SkillId { get; }
    }
}
