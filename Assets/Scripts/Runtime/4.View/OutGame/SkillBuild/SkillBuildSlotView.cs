using System;

namespace KillChord.Runtime.View.OutGame.SkillBuild
{
    /// <summary>
    ///     スキル編成スロットの表示状態を保持する View クラス。
    /// </summary>
    public sealed class SkillBuildSlotView
    {
        /// <summary>
        ///     スキル編成スロットを初期化する。
        /// </summary>
        /// <param name="slotIndex"> スロット番号。 </param>
        /// <param name="skillId"> 初期スキル ID。 </param>
        /// <param name="skillLabel"> 初期表示ラベル。 </param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillBuildSlotView(int slotIndex, int skillId, string skillLabel)
        {
            if (slotIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slotIndex), "スロット番号は 0 以上である必要があります。");
            }

            if (skillLabel == null)
            {
                throw new ArgumentNullException(nameof(skillLabel));
            }

            SlotIndex = slotIndex;
            InitialSkillId = skillId;
            InitialSkillLabel = skillLabel;
            CurrentSkillId = skillId;
            CurrentSkillLabel = skillLabel;
        }

        /// <summary> スロット番号。 </summary>
        public int SlotIndex { get; }

        /// <summary> 初期スキル ID。 </summary>
        public int InitialSkillId { get; private set; }

        /// <summary> 初期表示ラベル。 </summary>
        public string InitialSkillLabel { get; private set; }

        /// <summary> 現在のスキル ID。 </summary>
        public int CurrentSkillId { get; private set; }

        /// <summary> 現在の表示ラベル。 </summary>
        public string CurrentSkillLabel { get; private set; }

        /// <summary>
        ///     現在のスキル表示状態を更新する。
        /// </summary>
        /// <param name="skillId"> 新しいスキル ID。 </param>
        /// <param name="skillLabel"> 新しい表示ラベル。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public void ApplySkill(int skillId, string skillLabel)
        {
            if (skillLabel == null)
            {
                throw new ArgumentNullException(nameof(skillLabel));
            }

            CurrentSkillId = skillId;
            CurrentSkillLabel = skillLabel;
        }

        /// <summary>
        ///     現在状態を初期状態として確定する。
        /// </summary>
        public void CommitCurrentAsInitial()
        {
            InitialSkillId = CurrentSkillId;
            InitialSkillLabel = CurrentSkillLabel;
        }

        /// <summary>
        ///     現在の状態を初期状態へ戻す。
        /// </summary>
        public void Reset()
        {
            CurrentSkillId = InitialSkillId;
            CurrentSkillLabel = InitialSkillLabel;
        }
    }
}
