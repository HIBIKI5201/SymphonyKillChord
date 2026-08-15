using KillChord.Runtime.Adaptor.OutGame.SkillBuild;
using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillBuild
{
    /// <summary>
    ///     スキル詳細領域を管理する View。
    /// </summary>
    public sealed class SkillDetailView
    {
        /// <summary>
        ///     詳細 View を初期化する。
        /// </summary>
        /// <param name="rootElement"> 詳細領域のルート。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillDetailView(VisualElement rootElement)
        {
            if (rootElement == null)
            {
                throw new ArgumentNullException(nameof(rootElement));
            }

            _icon = rootElement.Q<Image>(ICON_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillDetailView)}] {ICON_NAME} が見つかりませんでした。");
            _nameLabel = rootElement.Q<Label>(NAME_LABEL_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillDetailView)}] {NAME_LABEL_NAME} が見つかりませんでした。");
            _comboLabel = rootElement.Q<Label>(COMBO_LABEL_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillDetailView)}] {COMBO_LABEL_NAME} が見つかりませんでした。");
            _skillTypeLabel = rootElement.Q<Label>(SKILL_TYPE_LABEL_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillDetailView)}] {SKILL_TYPE_LABEL_NAME} が見つかりませんでした。");
            _descriptionLabel = rootElement.Q<Label>(DESCRIPTION_LABEL_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillDetailView)}] {DESCRIPTION_LABEL_NAME} が見つかりませんでした。");
            _tipsHeadingLabel = rootElement.Q<Label>(TIPS_HEADING_LABEL_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillDetailView)}] {TIPS_HEADING_LABEL_NAME} が見つかりませんでした。");
            _tipsLabel = rootElement.Q<Label>(TIPS_LABEL_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillDetailView)}] {TIPS_LABEL_NAME} が見つかりませんでした。");
            _levelLabel = rootElement.Q<Label>(LEVEL_LABEL_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillDetailView)}] {LEVEL_LABEL_NAME} が見つかりませんでした。");
        }

        /// <summary>
        ///     スキル詳細を表示する。
        /// </summary>
        /// <param name="data"> 表示データ。 </param>
        public void Apply(in SkillViewData data)
        {
            _icon.sprite = data.Icon;
            _nameLabel.text = data.DisplayName;
            _comboLabel.text = data.ComboLabel;
            _skillTypeLabel.text = data.SkillTypeLabel;
            _descriptionLabel.text = data.HasEffectDescription
                ? data.EffectDescription
                : EMPTY_VALUE_LABEL;
            bool hasTips = !string.IsNullOrWhiteSpace(data.Tips);
            _tipsLabel.text = hasTips ? data.Tips : string.Empty;
            DisplayStyle tipsDisplay = hasTips ? DisplayStyle.Flex : DisplayStyle.None;
            _tipsHeadingLabel.style.display = tipsDisplay;
            _tipsLabel.style.display = tipsDisplay;
            _levelLabel.text = $"{LEVEL_LABEL_PREFIX}{data.Level}";
        }

        /// <summary>
        ///     未選択表示へ戻す。
        /// </summary>
        public void Clear()
        {
            _icon.sprite = null;
            _nameLabel.text = EMPTY_SELECTION_LABEL;
            _comboLabel.text = string.Empty;
            _skillTypeLabel.text = string.Empty;
            _descriptionLabel.text = string.Empty;
            _tipsLabel.text = string.Empty;
            _tipsHeadingLabel.style.display = DisplayStyle.None;
            _tipsLabel.style.display = DisplayStyle.None;
            _levelLabel.text = string.Empty;
        }

        private const string ICON_NAME = "skill-detail-icon";
        private const string NAME_LABEL_NAME = "skill-name-label";
        private const string COMBO_LABEL_NAME = "skill-combo-label";
        private const string SKILL_TYPE_LABEL_NAME = "skill-type-label";
        private const string DESCRIPTION_LABEL_NAME = "skill-description-label";
        private const string TIPS_HEADING_LABEL_NAME = "skill-tips-heading";
        private const string TIPS_LABEL_NAME = "skill-tips-label";
        private const string LEVEL_LABEL_NAME = "skill-level-label";
        private const string EMPTY_SELECTION_LABEL = "スキルを選択してください";
        private const string EMPTY_VALUE_LABEL = "—";
        private const string LEVEL_LABEL_PREFIX = "レベル: ";

        private readonly Image _icon;
        private readonly Label _nameLabel;
        private readonly Label _comboLabel;
        private readonly Label _skillTypeLabel;
        private readonly Label _descriptionLabel;
        private readonly Label _tipsHeadingLabel;
        private readonly Label _tipsLabel;
        private readonly Label _levelLabel;
    }
}
