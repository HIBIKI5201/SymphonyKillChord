using System;

namespace KillChord.Runtime.Adaptor.OutGame.SkillBuild
{
    /// <summary>
    ///     スキル装備スロットの保存済み状態と編集中状態を保持する。
    /// </summary>
    public readonly struct SkillBuildSlotState : IEquatable<SkillBuildSlotState>
    {
        /// <summary>
        ///     スロット状態を初期化する。
        /// </summary>
        /// <param name="slotIndex"> スロット番号。 </param>
        /// <param name="initialSkillId"> 保存済みスキル ID。 </param>
        /// <param name="currentSkillId"> 編集中スキル ID。 </param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public SkillBuildSlotState(int slotIndex, int initialSkillId, int currentSkillId)
        {
            if (slotIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slotIndex), "スロット番号は 0 以上である必要があります。");
            }

            SlotIndex = slotIndex;
            InitialSkillId = initialSkillId;
            CurrentSkillId = currentSkillId;
        }

        /// <summary> スロット番号。 </summary>
        public int SlotIndex { get; }

        /// <summary> 保存済みスキル ID。 </summary>
        public int InitialSkillId { get; }

        /// <summary> 編集中スキル ID。 </summary>
        public int CurrentSkillId { get; }

        /// <summary> 未保存の変更がある場合は true。 </summary>
        public bool HasUnsavedChanges => InitialSkillId != CurrentSkillId;

        /// <summary>
        ///     編集中スキルを変更した新しい状態を返す。
        /// </summary>
        /// <param name="skillId"> 新しいスキル ID。 </param>
        /// <returns> 更新後の状態。 </returns>
        public SkillBuildSlotState ChangeCurrentSkill(int skillId)
        {
            return new SkillBuildSlotState(SlotIndex, InitialSkillId, skillId);
        }

        /// <summary>
        ///     編集中状態を保存済み状態として確定する。
        /// </summary>
        /// <returns> 確定後の状態。 </returns>
        public SkillBuildSlotState CommitCurrentAsInitial()
        {
            return new SkillBuildSlotState(SlotIndex, CurrentSkillId, CurrentSkillId);
        }

        /// <summary>
        ///     編集中状態を保存済み状態へ戻す。
        /// </summary>
        /// <returns> 復元後の状態。 </returns>
        public SkillBuildSlotState ResetToInitial()
        {
            return new SkillBuildSlotState(SlotIndex, InitialSkillId, InitialSkillId);
        }

        /// <summary>
        ///     等値比較を行う。
        /// </summary>
        /// <param name="other"> 比較対象。 </param>
        /// <returns> 同じ状態の場合は true。 </returns>
        public bool Equals(SkillBuildSlotState other)
        {
            return SlotIndex == other.SlotIndex &&
                   InitialSkillId == other.InitialSkillId &&
                   CurrentSkillId == other.CurrentSkillId;
        }

        /// <summary>
        ///     等値比較を行う。
        /// </summary>
        /// <param name="obj"> 比較対象。 </param>
        /// <returns> 同じ状態の場合は true。 </returns>
        public override bool Equals(object obj)
        {
            return obj is SkillBuildSlotState other && Equals(other);
        }

        /// <summary>
        ///     ハッシュコードを取得する。
        /// </summary>
        /// <returns> ハッシュコード。 </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(SlotIndex, InitialSkillId, CurrentSkillId);
        }
    }
}
