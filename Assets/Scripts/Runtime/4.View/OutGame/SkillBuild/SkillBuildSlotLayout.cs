using KillChord.Runtime.Adaptor.OutGame.SkillBuild;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillBuild
{
    /// <summary>
    ///     スキル装備スロットの状態を UI 配置へ反映する View。
    /// </summary>
    public sealed class SkillBuildSlotLayout
    {
        /// <summary>
        ///     スロットレイアウトを初期化する。
        /// </summary>
        /// <param name="rootElement"> 改造画面ルート。 </param>
        /// <param name="skillListElement"> スキル一覧要素。 </param>
        /// <param name="skillElementResolver"> スキル ID に対応する要素の取得処理。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillBuildSlotLayout(
            VisualElement rootElement,
            VisualElement skillListElement,
            Func<int, VisualElement> skillElementResolver)
        {
            _rootElement = rootElement ?? throw new ArgumentNullException(nameof(rootElement));
            _skillListElement = skillListElement ?? throw new ArgumentNullException(nameof(skillListElement));
            _skillElementResolver = skillElementResolver ?? throw new ArgumentNullException(nameof(skillElementResolver));
        }

        /// <summary>
        ///     装備済みスキルを対応スロットへ移動する。
        /// </summary>
        /// <param name="slots"> スロット状態。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public void ApplyAll(IReadOnlyList<SkillBuildSlotState> slots)
        {
            if (slots == null)
            {
                throw new ArgumentNullException(nameof(slots));
            }

            List<VisualElement> slotElements = GetSlotElements();
            ClearSlotSkillElements(slotElements);

            for (int i = 0; i < slots.Count; i++)
            {
                SkillBuildSlotState slot = slots[i];
                if (slot.CurrentSkillId == EMPTY_SKILL_ID ||
                    slot.SlotIndex < 0 ||
                    slot.SlotIndex >= slotElements.Count)
                {
                    continue;
                }

                VisualElement skillElement = _skillElementResolver(slot.CurrentSkillId);
                if (skillElement != null)
                {
                    MoveSkillToSlot(skillElement, slotElements[slot.SlotIndex]);
                }
            }
        }

        /// <summary>
        ///     新旧スロット状態を比較し、変更されたスロットだけ表示へ反映する。
        /// </summary>
        /// <param name="previousSlots"> 変更前のスロット状態。 </param>
        /// <param name="currentSlots"> 変更後のスロット状態。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public void ApplyChanges(
            IReadOnlyList<SkillBuildSlotState> previousSlots,
            IReadOnlyList<SkillBuildSlotState> currentSlots)
        {
            if (previousSlots == null)
            {
                throw new ArgumentNullException(nameof(previousSlots));
            }

            if (currentSlots == null)
            {
                throw new ArgumentNullException(nameof(currentSlots));
            }

            List<VisualElement> slotElements = GetSlotElements();
            bool[] changedSlots = BuildChangedSlotMap(
                previousSlots,
                currentSlots,
                slotElements.Count);

            // 先に変更対象を空けることで、スロット間交換でも移動順に依存しない。
            for (int slotIndex = 0; slotIndex < changedSlots.Length; slotIndex++)
            {
                if (changedSlots[slotIndex])
                {
                    MoveSlotSkillToList(slotElements[slotIndex]);
                }
            }

            for (int i = 0; i < currentSlots.Count; i++)
            {
                SkillBuildSlotState slot = currentSlots[i];
                if (slot.SlotIndex < 0 ||
                    slot.SlotIndex >= changedSlots.Length ||
                    !changedSlots[slot.SlotIndex] ||
                    slot.CurrentSkillId == EMPTY_SKILL_ID)
                {
                    continue;
                }

                VisualElement skillElement = _skillElementResolver(slot.CurrentSkillId);
                if (skillElement != null)
                {
                    MoveSkillToSlot(skillElement, slotElements[slot.SlotIndex]);
                }
            }
        }

        private const string SKILL_ELEMENT_SLOT_CLASS_NAME = "skill-element-slot";
        private const string DRAGGABLE_CLASS_NAME = "draggable";
        private const int EMPTY_SKILL_ID = -1;

        private readonly VisualElement _rootElement;
        private readonly VisualElement _skillListElement;
        private readonly Func<int, VisualElement> _skillElementResolver;

        /// <summary>
        ///     スロット要素一覧を取得する。
        /// </summary>
        /// <returns> スロット要素一覧。 </returns>
        private List<VisualElement> GetSlotElements()
        {
            return _rootElement
                .Query<VisualElement>(className: SKILL_ELEMENT_SLOT_CLASS_NAME)
                .ToList();
        }

        /// <summary>
        ///     全スロット内のスキル要素を一覧へ戻す。
        /// </summary>
        /// <param name="slotElements"> スロット要素。 </param>
        private void ClearSlotSkillElements(IReadOnlyList<VisualElement> slotElements)
        {
            for (int i = 0; i < slotElements.Count; i++)
            {
                MoveSlotSkillToList(slotElements[i]);
            }
        }

        /// <summary>
        ///     スロット内のスキル要素を一覧へ戻す。
        /// </summary>
        /// <param name="slotElement"> 対象スロット。 </param>
        private void MoveSlotSkillToList(VisualElement slotElement)
        {
            VisualElement existingSkill =
                slotElement.Q<VisualElement>(className: DRAGGABLE_CLASS_NAME);
            if (existingSkill == null)
            {
                return;
            }

            _skillListElement.Add(existingSkill);
            ResetListStyle(existingSkill);
        }

        /// <summary>
        ///     新旧状態から変更されたスロット番号のマップを構築する。
        /// </summary>
        /// <param name="previousSlots"> 変更前のスロット状態。 </param>
        /// <param name="currentSlots"> 変更後のスロット状態。 </param>
        /// <param name="slotElementCount"> スロット要素数。 </param>
        /// <returns> 変更されたスロット番号のマップ。 </returns>
        private bool[] BuildChangedSlotMap(
            IReadOnlyList<SkillBuildSlotState> previousSlots,
            IReadOnlyList<SkillBuildSlotState> currentSlots,
            int slotElementCount)
        {
            bool[] result = new bool[slotElementCount];

            for (int i = 0; i < previousSlots.Count; i++)
            {
                SkillBuildSlotState previous = previousSlots[i];
                if (previous.SlotIndex < 0 || previous.SlotIndex >= result.Length)
                {
                    continue;
                }

                if (!TryFindSlot(currentSlots, previous.SlotIndex, out SkillBuildSlotState current) ||
                    previous.CurrentSkillId != current.CurrentSkillId)
                {
                    result[previous.SlotIndex] = true;
                }
            }

            for (int i = 0; i < currentSlots.Count; i++)
            {
                SkillBuildSlotState current = currentSlots[i];
                if (current.SlotIndex < 0 || current.SlotIndex >= result.Length)
                {
                    continue;
                }

                if (!TryFindSlot(previousSlots, current.SlotIndex, out SkillBuildSlotState previous) ||
                    previous.CurrentSkillId != current.CurrentSkillId)
                {
                    result[current.SlotIndex] = true;
                }
            }

            return result;
        }

        /// <summary>
        ///     指定番号のスロット状態を検索する。
        /// </summary>
        /// <param name="slots"> 検索対象。 </param>
        /// <param name="slotIndex"> スロット番号。 </param>
        /// <param name="slot"> 見つかった状態。 </param>
        /// <returns> 見つかった場合は true。 </returns>
        private bool TryFindSlot(
            IReadOnlyList<SkillBuildSlotState> slots,
            int slotIndex,
            out SkillBuildSlotState slot)
        {
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
        ///     スキル要素を指定スロットへ移動する。
        /// </summary>
        /// <param name="skillElement"> スキル要素。 </param>
        /// <param name="slotElement"> スロット要素。 </param>
        private void MoveSkillToSlot(VisualElement skillElement, VisualElement slotElement)
        {
            slotElement.Add(skillElement);
            skillElement.style.position = Position.Absolute;
            skillElement.schedule.Execute(() =>
            {
                Vector2 size = new(
                    skillElement.resolvedStyle.width,
                    skillElement.resolvedStyle.height);
                Vector2 desiredWorld = slotElement.worldBound.center - (size * 0.5f);
                Vector2 desiredLocal = slotElement.WorldToLocal(desiredWorld);
                skillElement.style.left = desiredLocal.x;
                skillElement.style.top = desiredLocal.y;
            });
        }

        /// <summary>
        ///     一覧配置用のスタイルへ戻す。
        /// </summary>
        /// <param name="skillElement"> スキル要素。 </param>
        private void ResetListStyle(VisualElement skillElement)
        {
            skillElement.style.position = Position.Relative;
            skillElement.style.left = StyleKeyword.Null;
            skillElement.style.top = StyleKeyword.Null;
        }
    }
}
