using R3;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.SkillBuild
{
    /// <summary>
    ///     改造画面の ViewModel インターフェース。
    /// </summary>
    public interface ISkillBuildViewModel
    {
        /// <summary> 所持スキル一覧。 </summary>
        public ReadOnlyReactiveProperty<IReadOnlyList<SkillViewData>> Skills { get; }

        /// <summary> スロット状態一覧。 </summary>
        public ReadOnlyReactiveProperty<IReadOnlyList<SkillBuildSlotState>> Slots { get; }

        /// <summary> ユーザーが明示的に選択したスキル ID。 </summary>
        public ReadOnlyReactiveProperty<int?> ExplicitlySelectedSkillId { get; }

        /// <summary> 詳細領域へ表示するスキル。 </summary>
        public ReadOnlyReactiveProperty<SkillViewData?> DisplayedSkill { get; }

        /// <summary> 所持ポイント。 </summary>
        public ReadOnlyReactiveProperty<int> OwnedPoints { get; }

        /// <summary>
        ///     指定したスキルを明示的に選択する。
        /// </summary>
        /// <param name="skillId"> スキル ID。 </param>
        public void SelectSkill(int skillId);

        /// <summary>
        ///     明示選択を解除して装備スロット1を既定表示する。
        /// </summary>
        public void ResetDetailToDefault();

        /// <summary>
        ///     ドロップ操作を編集中のスロット状態へ一括反映する。
        /// </summary>
        /// <param name="skillId"> スキル ID。 </param>
        /// <param name="destinationSlotIndex">
        ///     移動先スロット番号。一覧へ戻す場合は null。
        /// </param>
        public void ApplyDrop(int skillId, int? destinationSlotIndex);

        /// <summary>
        ///     未保存の変更があるかを判定する。
        /// </summary>
        /// <returns> 未保存の変更がある場合は true。 </returns>
        public bool HasUnsavedChanges();

        /// <summary>
        ///     現在の編集内容を保存する。
        /// </summary>
        /// <returns> 保存に成功した場合は true。 </returns>
        public Task<bool> SaveAsync();

        /// <summary>
        ///     スロットを保存済み状態へ戻す。
        /// </summary>
        public void ResetSlots();
    }
}
