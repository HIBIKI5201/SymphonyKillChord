using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillBuild
{
    /// <summary>
    ///     入手済みスキル一覧を表示する View クラス。
    /// </summary>
    public sealed class SkillListView
    {
        /// <summary>
        ///     SkillListView を初期化する。
        /// </summary>
        /// <param name="scrollView"> 表示先の ScrollView。 </param>
        /// <param name="skillElementTemplate"> 各要素のテンプレート。 </param>
        /// <param name="onSkillElementCreated"> スキル要素生成時に呼ばれるコールバック（ドラッグアンドドロップのセットアップ等に使用）。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillListView(VisualElement scrollView, VisualTreeAsset skillElementTemplate, Action<VisualElement> onSkillElementCreated = null)
        {
            _scrollView = scrollView ?? throw new ArgumentNullException(nameof(scrollView));
            _skillElementTemplate = skillElementTemplate ?? throw new ArgumentNullException(nameof(skillElementTemplate));
            _onSkillElementCreated = onSkillElementCreated;

            ConfigureScrollView();
        }

        /// <summary>
        ///     スキル一覧を表示内容に反映する。
        /// </summary>
        /// <param name="skills"> 表示するスキル一覧。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SetSkills(IReadOnlyList<(int skillId, string skillLabel)> skills)
        {
            if (skills == null)
            {
                throw new ArgumentNullException(nameof(skills), "スキル一覧が null です。");
            }

            Clear();

            for (int i = 0; i < skills.Count; i++)
            {
                VisualElement skillElement = CreateSkillElement(skills[i].skillId, skills[i].skillLabel ?? string.Empty);
                _scrollView.Add(skillElement);
            }
        }

        /// <summary>
        ///     装備済みスキルを入手済みスキル一覧から探し、対応スロットへ移動する。
        /// </summary>
        /// <param name="rootElement"> スキル編成画面のルート要素。 </param>
        /// <param name="slots"> 現在のスロット表示状態。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public void MoveEquippedSkillsToSlots(VisualElement rootElement, ReadOnlySpan<SkillBuildSlotView> slots)
        {
            if (rootElement == null)
            {
                throw new ArgumentNullException(nameof(rootElement));
            }

            List<VisualElement> slotElements =
                rootElement.Query<VisualElement>(className: SKILL_ELEMENT_SLOT_CLASS_NAME).ToList();

            ClearSlotSkillElements(slotElements);

            for (int i = 0; i < slots.Length; i++)
            {
                SkillBuildSlotView slotView = slots[i];
                if (slotView.CurrentSkillId == EMPTY_SKILL_ID)
                {
                    continue;
                }

                if (slotView.SlotIndex < 0 || slotView.SlotIndex >= slotElements.Count)
                {
                    continue;
                }

                VisualElement skillElement = FindSkillElement(slotView.CurrentSkillId);
                if (skillElement == null)
                {
                    continue;
                }

                MoveSkillToSlot(skillElement, slotElements[slotView.SlotIndex]);
            }
        }

        /// <summary>
        ///     表示内容をクリアする。
        /// </summary>
        public void Clear()
        {
            _scrollView.Clear();
        }

        /// <summary> ドラッグ対象の USS クラス名。 </summary>
        private const string DRAGGABLE_CLASS_NAME = "draggable";

        /// <summary> スキルスロットの USS クラス名。 </summary>
        private const string SKILL_ELEMENT_SLOT_CLASS_NAME = "skill-element-slot";

        /// <summary> スキル名表示ラベル名。 </summary>
        private const string SKILL_LABEL_NAME = "skill-label";

        /// <summary> スキル名フォントサイズ。 </summary>
        private const int SKILL_LABEL_FONT_SIZE = 24;

        /// <summary> 空スキル ID。 </summary>
        private const int EMPTY_SKILL_ID = -1;
        private const string SKILL_ELEMENT_PREFIX = "SkillElement_";
        private readonly VisualElement _scrollView;
        private readonly VisualTreeAsset _skillElementTemplate;

        /// <summary> スキル要素生成時に呼び出されるコールバック。 </summary>
        private readonly Action<VisualElement> _onSkillElementCreated;

        /// <summary>
        ///     ScrollView の基本設定を行う。
        /// </summary>
        private void ConfigureScrollView()
        {
            if (_scrollView is ScrollView scrollView)
            {
                scrollView.mode = ScrollViewMode.Horizontal;
                scrollView.horizontalScrollerVisibility = ScrollerVisibility.Auto;
                scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            }
        }

        /// <summary>
        ///     各スキル要素を生成する。
        ///     生成後に _onSkillElementCreated コールバックを呼び出す。(ドラッグアンドドロップのセットアップ等に使用)
        /// </summary>
        /// <param name="skillId"> スキル ID。 </param>
        /// <param name="skillLabel"> 表示するスキル名。 </param>
        /// <returns> 生成した要素。 </returns>
        private VisualElement CreateSkillElement(int skillId, string skillLabel)
        {
            TemplateContainer skillElement = _skillElementTemplate.Instantiate();
            skillElement.AddToClassList(DRAGGABLE_CLASS_NAME);

            StringBuilder skillElementName = new StringBuilder(SKILL_ELEMENT_PREFIX);
            skillElementName.Append(skillLabel);

            skillElement.name = skillElementName.ToString();

            skillElement.userData = skillId;

            Label label = skillElement.Q<Label>(SKILL_LABEL_NAME);
            if (label == null)
            {
                label = CreateSkillLabel();
                skillElement.Add(label);
            }

            label.text = skillLabel;

            // 生成した要素に対してドラッグアンドドロップのセットアップ等の外部処理を呼び出す。
            _onSkillElementCreated?.Invoke(skillElement);

            return skillElement;
        }

        /// <summary>
        ///     スロット内に残っている既存スキル要素をクリアする。
        /// </summary>
        /// <param name="slotElements"> 対象スロット一覧。 </param>
        private void ClearSlotSkillElements(IReadOnlyList<VisualElement> slotElements)
        {
            for (int i = 0; i < slotElements.Count; i++)
            {
                VisualElement existingSkill = slotElements[i].Q<VisualElement>(className: DRAGGABLE_CLASS_NAME);
                existingSkill?.RemoveFromHierarchy();
            }
        }

        /// <summary>
        ///     指定したスキル ID に一致する入手済みスキル要素を取得する。
        /// </summary>
        /// <param name="skillId"> 検索対象のスキル ID。 </param>
        /// <returns> 一致したスキル要素。見つからない場合は null。 </returns>
        private VisualElement FindSkillElement(int skillId)
        {
            List<VisualElement> skillElements =
                _scrollView.Query<VisualElement>(className: DRAGGABLE_CLASS_NAME).ToList();

            for (int i = 0; i < skillElements.Count; i++)
            {
                if (skillElements[i].userData is int storedSkillId && storedSkillId == skillId)
                {
                    return skillElements[i];
                }
            }

            return null;
        }

        /// <summary>
        ///     スキル要素を指定スロットへ移動する。
        /// </summary>
        /// <param name="skillElement"> 移動対象のスキル要素。 </param>
        /// <param name="slotElement"> 移動先スロット。 </param>
        private void MoveSkillToSlot(VisualElement skillElement, VisualElement slotElement)
        {
            if (skillElement == null || slotElement == null)
            {
                return;
            }

            slotElement.Add(skillElement);
            skillElement.style.position = Position.Absolute;

            skillElement.schedule.Execute(() =>
            {
                Vector2 skillElementSize = new Vector2(
                    skillElement.resolvedStyle.width,
                    skillElement.resolvedStyle.height);

                Vector2 desiredWorld = slotElement.worldBound.center - (skillElementSize * 0.5f);
                Vector2 desiredLocal = slotElement.WorldToLocal(desiredWorld);

                skillElement.style.left = desiredLocal.x;
                skillElement.style.top = desiredLocal.y;
            });
        }

        /// <summary>
        ///     スキル表示用ラベルを生成する。
        /// </summary>
        /// <returns> 生成したラベル。 </returns>
        private Label CreateSkillLabel()
        {
            Label label = new Label
            {
                name = SKILL_LABEL_NAME
            };

            label.style.position = Position.Absolute;
            label.style.left = 0;
            label.style.right = 0;
            label.style.top = 0;
            label.style.bottom = 0;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.fontSize = SKILL_LABEL_FONT_SIZE;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;

            return label;
        }
    }
}