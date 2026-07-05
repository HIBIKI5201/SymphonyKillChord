using KillChord.Runtime.Adaptor.OutGame.SkillBuild;
using KillChord.Runtime.View.OutGame.Screen;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.View.OutGame.SkillBuild
{
    /// <summary>
    ///     改造画面の UI 表示状態を保持する ViewModel クラス。
    /// </summary>
    public sealed class SkillBuildViewModel : ISkillBuildViewModel, IDisposable
    {
        /// <summary>
        ///     SkillBuildViewModel を初期化する。
        /// </summary>
        /// <param name="outGameUIEvent"> アウトゲーム UI イベント。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillBuildViewModel(OutGameUIEvent outGameUIEvent)
        {
            _outGameUIEvent = outGameUIEvent ?? throw new ArgumentNullException(nameof(outGameUIEvent));
            _outGameUIEvent.OnSkillBuildSaved += HandleSkillBuildSavedHandler;
        }

        /// <summary> 保存要求イベント。 </summary>
        public event Action<ReadOnlyMemory<int>> OnSaveRequested;

        /// <summary> スキル一覧更新イベント。 </summary>
        public event Action<IReadOnlyList<(int skillId, string skillLabel)>> OnSkillListChanged;

        /// <summary> スロット一覧更新イベント。 </summary>
        public event Action<ReadOnlyMemory<SkillBuildSlotView>> OnSlotsChanged;

        /// <summary> 現在のスロット一覧。 </summary>
        public ReadOnlyMemory<SkillBuildSlotView> Slots => _slotViews;

        /// <summary>
        ///     DTO から UI 表示状態を反映する。
        /// </summary>
        /// <param name="dto"> 表示更新 DTO。 </param>
        public void Apply(in SkillBuildViewDTO dto)
        {
            _slotViews = new SkillBuildSlotView[dto.Slots.Length];
            for (int i = 0; i < dto.Slots.Length; i++)
            {
                SkillBuildSlotDTO slot = dto.Slots[i];
                _slotViews[i] = new SkillBuildSlotView(slot.SlotIndex, slot.SkillId, slot.SkillLabel);
            }

            _skills = new (int skillId, string skillLabel)[dto.Skills.Length];
            for (int i = 0; i < dto.Skills.Length; i++)
            {
                _skills[i] = dto.Skills[i];
            }

            OnSlotsChanged?.Invoke(_slotViews);
            OnSkillListChanged?.Invoke(_skills);
        }

        /// <summary>
        ///     指定スロットの現在表示状態を更新する。
        /// </summary>
        /// <param name="slotIndex"> 対象スロット番号。 </param>
        /// <param name="skillId"> 新しいスキル ID。 </param>
        /// <param name="skillLabel"> 新しい表示ラベル。 </param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public void UpdateSlot(int slotIndex, int skillId, string skillLabel)
        {
            if (skillLabel == null)
            {
                throw new ArgumentNullException(nameof(skillLabel));
            }

            SkillBuildSlotView slotView = FindSlotView(slotIndex);
            slotView.ApplySkill(skillId, skillLabel);
            OnSlotsChanged?.Invoke(_slotViews);
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

            _outGameUIEvent.OnSkillBuildSaved -= HandleSkillBuildSavedHandler;
            _isDisposed = true;
        }

        private readonly OutGameUIEvent _outGameUIEvent;
        private SkillBuildSlotView[] _slotViews = Array.Empty<SkillBuildSlotView>();
        private (int skillId, string skillLabel)[] _skills = Array.Empty<(int, string)>();
        private bool _isDisposed;

        /// <summary>
        ///     保存ボタン押下イベントを処理する。
        /// </summary>
        private void HandleSkillBuildSavedHandler()
        {
            int[] skillIds = BuildCurrentSkillIds();
            OnSaveRequested?.Invoke(skillIds);
        }

        /// <summary>
        ///     現在のスロット状態からスキル ID 配列を構築する。
        /// </summary>
        /// <returns> 現在のスキル ID 配列。 </returns>
        /// <exception cref="InvalidOperationException"></exception>
        private int[] BuildCurrentSkillIds()
        {
            int[] result = new int[_slotViews.Length];
            bool[] assigned = new bool[_slotViews.Length];

            for (int i = 0; i < _slotViews.Length; i++)
            {
                SkillBuildSlotView slotView = _slotViews[i];
                int slotIndex = slotView.SlotIndex;

                if (slotIndex < 0 || slotIndex >= result.Length)
                {
                    throw new InvalidOperationException($"スロット番号が不正です。 slotIndex={slotIndex}");
                }

                if (assigned[slotIndex])
                {
                    throw new InvalidOperationException($"重複したスロット番号が存在します。 slotIndex={slotIndex}");
                }

                result[slotIndex] = slotView.CurrentSkillId;
                assigned[slotIndex] = true;
            }

            return result;
        }

        /// <summary>
        ///     指定スロット番号に対応する View を取得する。
        /// </summary>
        /// <param name="slotIndex"> スロット番号。 </param>
        /// <returns> 対象スロット View。 </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private SkillBuildSlotView FindSlotView(int slotIndex)
        {
            for (int i = 0; i < _slotViews.Length; i++)
            {
                if (_slotViews[i].SlotIndex == slotIndex)
                {
                    return _slotViews[i];
                }
            }

            throw new ArgumentOutOfRangeException(nameof(slotIndex), $"指定されたスロットが見つかりません。 slotIndex={slotIndex}");
        }

        /// <summary>
        ///     未保存の変更があるかを判定する。
        /// </summary>
        public bool HasUnsavedChanges()
        {
            for (int i = 0; i < _slotViews.Length; i++)
            {
                SkillBuildSlotView slotView = _slotViews[i];
                if (slotView.CurrentSkillId != slotView.InitialSkillId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     現在のスロット状態を保存済み状態として確定する。
        /// </summary>
        public void MarkCurrentAsSaved()
        {
            for (int i = 0; i < _slotViews.Length; i++)
            {
                _slotViews[i].CommitCurrentAsInitial();
            }
        }
    }
}