using KillChord.Runtime.Adaptor.OutGame.SkillBuild;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillBuild
{
    /// <summary>
    ///     スキル装備スロットの表示をスキルデータへバインドする View。
    ///     カード要素自体は一覧に残したまま、スロット側は小さなアイコン・名前表示のみを持つ。
    /// </summary>
    public sealed class SkillBuildSlotLayout
    {
        /// <summary>
        ///     SkillBuildSlotLayout を初期化する。
        /// </summary>
        /// <param name="rootElement"> 画面ルート要素。 </param>
        /// <param name="skillDataResolver"> スキル ID から表示データを取得する関数。 </param>
        /// <param name="onSlotTapped"> スロットタップ時に、装備解除するスキル ID を通知するコールバック。 </param>
        public SkillBuildSlotLayout(
            VisualElement rootElement,
            Func<int, SkillViewData?> skillDataResolver,
            Action<int> onSlotTapped)
        {
            _rootElement = rootElement ?? throw new ArgumentNullException(nameof(rootElement));
            _skillDataResolver = skillDataResolver ?? throw new ArgumentNullException(nameof(skillDataResolver));
            _onSlotTapped = onSlotTapped;

            _slotElements = _rootElement.Query<VisualElement>(className: SKILL_ELEMENT_SLOT_CLASS_NAME).ToList();
            _slotSkillIds = new int[_slotElements.Count];
            for (int i = 0; i < _slotElements.Count; i++)
            {
                _slotSkillIds[i] = EMPTY_SKILL_ID;
                _slotElements[i].RegisterCallback<ClickEvent>(HandleSlotClickHandler);
            }
        }

        /// <summary>
        ///     スロット状態を表示へ反映する。
        /// </summary>
        /// <param name="slots"> スロット状態一覧。 </param>
        public void Apply(IReadOnlyList<SkillBuildSlotState> slots)
        {
            if (slots == null)
            {
                throw new ArgumentNullException(nameof(slots));
            }

            for (int i = 0; i < _slotElements.Count; i++)
            {
                _slotSkillIds[i] = EMPTY_SKILL_ID;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                SkillBuildSlotState slot = slots[i];
                if (slot.SlotIndex < 0 || slot.SlotIndex >= _slotElements.Count)
                {
                    continue;
                }

                _slotSkillIds[slot.SlotIndex] = slot.CurrentSkillId;
            }

            for (int i = 0; i < _slotElements.Count; i++)
            {
                BindSlot(_slotElements[i], _slotSkillIds[i]);
            }
        }

        /// <summary>
        ///     登録したイベント購読を解除する。
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < _slotElements.Count; i++)
            {
                _slotElements[i].UnregisterCallback<ClickEvent>(HandleSlotClickHandler);
            }
        }

        private const string SKILL_ELEMENT_SLOT_CLASS_NAME = "skill-element-slot";
        private const string SLOT_ICON_NAME = "skill-slot-icon";
        private const string SLOT_NAME_LABEL_NAME = "skill-slot-name";
        private const string SLOT_FILLED_CLASS_NAME = "is-filled";
        private const int EMPTY_SKILL_ID = -1;

        private readonly VisualElement _rootElement;
        private readonly Func<int, SkillViewData?> _skillDataResolver;
        private readonly Action<int> _onSlotTapped;
        private readonly List<VisualElement> _slotElements;
        private readonly int[] _slotSkillIds;

        /// <summary>
        ///     1つのスロットへスキル情報をバインドする。
        /// </summary>
        /// <param name="slotElement"> スロット要素。 </param>
        /// <param name="skillId"> 装備中スキル ID。空の場合は EMPTY_SKILL_ID。 </param>
        private void BindSlot(VisualElement slotElement, int skillId)
        {
            Image icon = slotElement.Q<Image>(SLOT_ICON_NAME);
            Label nameLabel = slotElement.Q<Label>(SLOT_NAME_LABEL_NAME);

            if (skillId == EMPTY_SKILL_ID)
            {
                slotElement.RemoveFromClassList(SLOT_FILLED_CLASS_NAME);
                if (icon != null)
                {
                    icon.style.display = DisplayStyle.None;
                    icon.sprite = null;
                }

                if (nameLabel != null)
                {
                    nameLabel.text = string.Empty;
                }

                return;
            }

            SkillViewData? data = _skillDataResolver(skillId);
            slotElement.AddToClassList(SLOT_FILLED_CLASS_NAME);
            if (icon != null)
            {
                icon.style.display = DisplayStyle.Flex;
                icon.sprite = data?.Icon;
            }

            if (nameLabel != null)
            {
                nameLabel.text = data?.DisplayName ?? string.Empty;
            }
        }

        /// <summary>
        ///     スロットタップによる装備解除を処理する。
        /// </summary>
        /// <param name="evt"> クリックイベント。 </param>
        private void HandleSlotClickHandler(ClickEvent evt)
        {
            if (evt.currentTarget is not VisualElement slotElement)
            {
                return;
            }

            int slotIndex = _slotElements.IndexOf(slotElement);
            if (slotIndex < 0 || _slotSkillIds[slotIndex] == EMPTY_SKILL_ID)
            {
                return;
            }

            _onSlotTapped?.Invoke(_slotSkillIds[slotIndex]);
        }
    }
}
