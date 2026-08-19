using KillChord.Runtime.Adaptor.OutGame.SkillBuild;
using R3;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KillChord.Runtime.View.OutGame.SkillBuild
{
    /// <summary>
    ///     改造画面の UI 表示状態を保持する ViewModel クラス。
    /// </summary>
    public sealed class SkillBuildViewModel :
        ISkillBuildViewModel,
        ISkillBuildViewModelWriter,
        IDisposable
    {
        /// <summary>
        ///     SkillBuildViewModel を初期化する。
        /// </summary>
        /// <param name="command"> スキル編成コマンド。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillBuildViewModel(ISkillBuildCommand command)
        {
            _command = command ?? throw new ArgumentNullException(nameof(command));
        }

        /// <summary> 所持スキル一覧。 </summary>
        public ReadOnlyReactiveProperty<IReadOnlyList<SkillViewData>> Skills => _skills;

        /// <summary> スロット状態一覧。 </summary>
        public ReadOnlyReactiveProperty<IReadOnlyList<SkillBuildSlotState>> Slots => _slots;

        /// <summary> ユーザーが明示的に選択したスキル ID。 </summary>
        public ReadOnlyReactiveProperty<int?> ExplicitlySelectedSkillId => _explicitlySelectedSkillId;

        /// <summary> 詳細領域へ表示するスキル。 </summary>
        public ReadOnlyReactiveProperty<SkillViewData?> DisplayedSkill => _displayedSkill;

        /// <summary> 所持ポイント。 </summary>
        public ReadOnlyReactiveProperty<int> OwnedPoints => _ownedPoints;

        /// <summary>
        ///     DTO から UI 表示状態を反映する。
        /// </summary>
        /// <param name="dto"> 表示更新 DTO。 </param>
        public void Apply(in SkillBuildViewDTO dto)
        {
            SkillViewData[] skills = dto.Skills.ToArray();
            SkillBuildSlotState[] slots = new SkillBuildSlotState[dto.Slots.Length];

            for (int i = 0; i < dto.Slots.Length; i++)
            {
                SkillBuildSlotData slot = dto.Slots[i];
                slots[i] = new SkillBuildSlotState(slot.SlotIndex, slot.SkillId, slot.SkillId);
            }

            _skills.Value = skills;
            _slots.Value = slots;
            _ownedPoints.Value = dto.OwnedPoints;

            if (_explicitlySelectedSkillId.Value.HasValue &&
                !TryFindSkill(_explicitlySelectedSkillId.Value.Value, out _))
            {
                _explicitlySelectedSkillId.Value = null;
            }

            ResolveDisplayedSkill();
        }

        /// <summary>
        ///     指定したスキルを明示的に選択する。
        /// </summary>
        /// <param name="skillId"> スキル ID。 </param>
        public void SelectSkill(int skillId)
        {
            if (!TryFindSkill(skillId, out SkillViewData skill))
            {
                return;
            }

            _explicitlySelectedSkillId.Value = skillId;
            _displayedSkill.Value = skill;
        }

        /// <summary>
        ///     明示選択を解除して装備スロット1を既定表示する。
        /// </summary>
        public void ResetDetailToDefault()
        {
            _explicitlySelectedSkillId.Value = null;
            ResolveDisplayedSkill();
        }

        /// <summary>
        ///     ドロップ操作を編集中のスロット状態へ一括反映する。
        /// </summary>
        /// <param name="skillId"> スキル ID。 </param>
        /// <param name="destinationSlotIndex">
        ///     移動先スロット番号。一覧へ戻す場合は null。
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void ApplyDrop(int skillId, int? destinationSlotIndex)
        {
            IReadOnlyList<SkillBuildSlotState> currentSlots = _slots.Value;
            SkillBuildSlotState[] updatedSlots = CopySlots(currentSlots);
            int sourceStateIndex = FindSkillSlotStateIndex(updatedSlots, skillId);

            if (!destinationSlotIndex.HasValue)
            {
                if (sourceStateIndex < 0)
                {
                    return;
                }

                updatedSlots[sourceStateIndex] =
                    updatedSlots[sourceStateIndex].ChangeCurrentSkill(EMPTY_SKILL_ID);
            }
            else
            {
                int destinationStateIndex =
                    FindSlotStateIndex(updatedSlots, destinationSlotIndex.Value);
                if (sourceStateIndex == destinationStateIndex)
                {
                    return;
                }

                int displacedSkillId = updatedSlots[destinationStateIndex].CurrentSkillId;
                updatedSlots[destinationStateIndex] =
                    updatedSlots[destinationStateIndex].ChangeCurrentSkill(skillId);

                if (sourceStateIndex >= 0)
                {
                    updatedSlots[sourceStateIndex] =
                        updatedSlots[sourceStateIndex].ChangeCurrentSkill(displacedSkillId);
                }
            }

            // スワップを含む全変更を確定してから一度だけ通知し、
            // View に中間状態を公開しない。
            _slots.Value = updatedSlots;
            ResolveDisplayedSkill();
        }

        /// <summary>
        ///     未保存の変更があるかを判定する。
        /// </summary>
        /// <returns> 未保存の変更がある場合は true。 </returns>
        public bool HasUnsavedChanges()
        {
            IReadOnlyList<SkillBuildSlotState> slots = _slots.Value;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].HasUnsavedChanges)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     現在の編集内容を保存する。
        /// </summary>
        /// <returns> 保存に成功した場合は true。 </returns>
        public async Task<bool> SaveAsync()
        {
            int[] skillIds = BuildCurrentSkillIds();
            bool isSaved = await _command.SaveAsync(skillIds);
            if (!isSaved)
            {
                return false;
            }

            MarkCurrentAsSaved();
            return true;
        }

        /// <summary>
        ///     スロットを保存済み状態へ戻す。
        /// </summary>
        public void ResetSlots()
        {
            IReadOnlyList<SkillBuildSlotState> currentSlots = _slots.Value;
            SkillBuildSlotState[] resetSlots = new SkillBuildSlotState[currentSlots.Count];

            for (int i = 0; i < currentSlots.Count; i++)
            {
                resetSlots[i] = currentSlots[i].ResetToInitial();
            }

            _slots.Value = resetSlots;
            ResolveDisplayedSkill();
        }

        /// <summary>
        ///     リソースを解放する。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _skills.Dispose();
            _slots.Dispose();
            _explicitlySelectedSkillId.Dispose();
            _displayedSkill.Dispose();
            _ownedPoints.Dispose();
            _isDisposed = true;
        }

        private const int DEFAULT_DETAIL_SLOT_INDEX = 0;
        private const int EMPTY_SKILL_ID = -1;

        private readonly ISkillBuildCommand _command;
        private readonly ReactiveProperty<IReadOnlyList<SkillViewData>> _skills =
            new(Array.Empty<SkillViewData>());
        private readonly ReactiveProperty<IReadOnlyList<SkillBuildSlotState>> _slots =
            new(Array.Empty<SkillBuildSlotState>());
        private readonly ReactiveProperty<int?> _explicitlySelectedSkillId = new(null);
        private readonly ReactiveProperty<SkillViewData?> _displayedSkill = new(null);
        private readonly ReactiveProperty<int> _ownedPoints = new(0);
        private bool _isDisposed;

        /// <summary>
        ///     現在のスロット状態からスキル ID 配列を構築する。
        /// </summary>
        /// <returns> スロット番号順のスキル ID 配列。 </returns>
        /// <exception cref="InvalidOperationException"></exception>
        private int[] BuildCurrentSkillIds()
        {
            IReadOnlyList<SkillBuildSlotState> slots = _slots.Value;
            int[] result = new int[slots.Count];
            bool[] assigned = new bool[slots.Count];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = EMPTY_SKILL_ID;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                SkillBuildSlotState slot = slots[i];
                if (slot.SlotIndex < 0 || slot.SlotIndex >= result.Length)
                {
                    throw new InvalidOperationException($"スロット番号が不正です。 slotIndex={slot.SlotIndex}");
                }

                if (assigned[slot.SlotIndex])
                {
                    throw new InvalidOperationException($"重複したスロット番号が存在します。 slotIndex={slot.SlotIndex}");
                }

                result[slot.SlotIndex] = slot.CurrentSkillId;
                assigned[slot.SlotIndex] = true;
            }

            return result;
        }

        /// <summary>
        ///     現在のスロット状態を保存済み状態として確定する。
        /// </summary>
        private void MarkCurrentAsSaved()
        {
            IReadOnlyList<SkillBuildSlotState> currentSlots = _slots.Value;
            SkillBuildSlotState[] committedSlots = new SkillBuildSlotState[currentSlots.Count];

            for (int i = 0; i < currentSlots.Count; i++)
            {
                committedSlots[i] = currentSlots[i].CommitCurrentAsInitial();
            }

            _slots.Value = committedSlots;
            ResolveDisplayedSkill();
        }

        /// <summary>
        ///     現在の選択状態とスロット状態から詳細表示対象を解決する。
        /// </summary>
        private void ResolveDisplayedSkill()
        {
            if (_explicitlySelectedSkillId.Value.HasValue &&
                TryFindSkill(_explicitlySelectedSkillId.Value.Value, out SkillViewData selectedSkill))
            {
                _displayedSkill.Value = selectedSkill;
                return;
            }

            if (TryFindSlot(DEFAULT_DETAIL_SLOT_INDEX, out SkillBuildSlotState defaultSlot) &&
                defaultSlot.CurrentSkillId != EMPTY_SKILL_ID &&
                TryFindSkill(defaultSlot.CurrentSkillId, out SkillViewData defaultSkill))
            {
                _displayedSkill.Value = defaultSkill;
                return;
            }

            _displayedSkill.Value = null;
        }

        /// <summary>
        ///     スキル ID に対応する表示データを検索する。
        /// </summary>
        /// <param name="skillId"> スキル ID。 </param>
        /// <param name="skill"> 見つかった表示データ。 </param>
        /// <returns> 見つかった場合は true。 </returns>
        private bool TryFindSkill(int skillId, out SkillViewData skill)
        {
            IReadOnlyList<SkillViewData> skills = _skills.Value;
            for (int i = 0; i < skills.Count; i++)
            {
                if (skills[i].SkillId == skillId)
                {
                    skill = skills[i];
                    return true;
                }
            }

            skill = default;
            return false;
        }

        /// <summary>
        ///     スロット番号に対応する状態を検索する。
        /// </summary>
        /// <param name="slotIndex"> スロット番号。 </param>
        /// <param name="slot"> 見つかった状態。 </param>
        /// <returns> 見つかった場合は true。 </returns>
        private bool TryFindSlot(int slotIndex, out SkillBuildSlotState slot)
        {
            IReadOnlyList<SkillBuildSlotState> slots = _slots.Value;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].SlotIndex == slotIndex)
                {
                    slot = slots[i];
                    return true;
                }
            }

            slot = default;
            return false;
        }

        /// <summary>
        ///     スロット一覧を新しい配列へコピーする。
        /// </summary>
        /// <param name="slots"> コピー元。 </param>
        /// <returns> 新しい配列。 </returns>
        private SkillBuildSlotState[] CopySlots(IReadOnlyList<SkillBuildSlotState> slots)
        {
            SkillBuildSlotState[] result = new SkillBuildSlotState[slots.Count];
            for (int i = 0; i < slots.Count; i++)
            {
                result[i] = slots[i];
            }

            return result;
        }

        /// <summary>
        ///     スロット番号に対応する配列位置を取得する。
        /// </summary>
        /// <param name="slots"> 検索対象。 </param>
        /// <param name="slotIndex"> スロット番号。 </param>
        /// <returns> 配列位置。 </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private int FindSlotStateIndex(IReadOnlyList<SkillBuildSlotState> slots, int slotIndex)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].SlotIndex == slotIndex)
                {
                    return i;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(slotIndex), $"指定されたスロットが見つかりません。 slotIndex={slotIndex}");
        }

        /// <summary>
        ///     指定したスキルを編集中のスロットから検索する。
        /// </summary>
        /// <param name="slots"> 検索対象。 </param>
        /// <param name="skillId"> スキル ID。 </param>
        /// <returns> 配列位置。装備されていない場合は -1。 </returns>
        private int FindSkillSlotStateIndex(
            IReadOnlyList<SkillBuildSlotState> slots,
            int skillId)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].CurrentSkillId == skillId)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
