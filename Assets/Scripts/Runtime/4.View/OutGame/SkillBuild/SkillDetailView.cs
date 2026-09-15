using KillChord.Runtime.Adaptor.OutGame.SkillBuild;
using KillChord.Runtime.View.OutGame.Common;
using System;
using UnityEngine;
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
        /// <param name="comboHexIcon"> 発動コマンド表示に使う六角形スプライト(UI_hexagon)。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillDetailView(VisualElement rootElement, Sprite comboHexIcon)
        {
            if (rootElement == null)
            {
                throw new ArgumentNullException(nameof(rootElement));
            }

            _comboHexIcon = comboHexIcon;

            _icon = rootElement.Q<Image>(ICON_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillDetailView)}] {ICON_NAME} が見つかりませんでした。");
            _nameLabel = rootElement.Q<Label>(NAME_LABEL_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillDetailView)}] {NAME_LABEL_NAME} が見つかりませんでした。");
            _comboLabel = rootElement.Q<Label>(COMBO_LABEL_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillDetailView)}] {COMBO_LABEL_NAME} が見つかりませんでした。");
            _comboRow = rootElement.Q<VisualElement>(COMBO_ROW_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillDetailView)}] {COMBO_ROW_NAME} が見つかりませんでした。");
            _skillTypeIcon = rootElement.Q<Image>(SKILL_TYPE_ICON_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillDetailView)}] {SKILL_TYPE_ICON_NAME} が見つかりませんでした。");
            _descriptionLabel = rootElement.Q<Label>(DESCRIPTION_LABEL_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillDetailView)}] {DESCRIPTION_LABEL_NAME} が見つかりませんでした。");
            _tipsHeadingLabel = rootElement.Q<Label>(TIPS_HEADING_LABEL_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillDetailView)}] {TIPS_HEADING_LABEL_NAME} が見つかりませんでした。");
            _tipsLabel = rootElement.Q<Label>(TIPS_LABEL_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillDetailView)}] {TIPS_LABEL_NAME} が見つかりませんでした。");
            _levelLabel = rootElement.Q<Label>(LEVEL_LABEL_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillDetailView)}] {LEVEL_LABEL_NAME} が見つかりませんでした。");
            _levelupPointLabel = rootElement.Q<Label>(LEVELUP_POINT_LABEL_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillDetailView)}] {LEVELUP_POINT_LABEL_NAME} が見つかりませんでした。");
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
            SetComboSteps(data.ComboStepColors);
            _skillTypeIcon.sprite = data.GenreIcon;
            _descriptionLabel.text = data.HasEffectDescription
                ? data.EffectDescription
                : EMPTY_VALUE_LABEL;
            bool hasTips = !string.IsNullOrWhiteSpace(data.Tips);
            _tipsLabel.text = hasTips ? data.Tips : string.Empty;
            DisplayStyle tipsDisplay = hasTips ? DisplayStyle.Flex : DisplayStyle.None;
            _tipsHeadingLabel.style.display = tipsDisplay;
            _tipsLabel.style.display = tipsDisplay;
            _hasSkill = true;
            _levelLabel.text = $"{data.Level} → {data.Level + 1}";
            RefreshLevelupPointLabel();
        }

        /// <summary>
        ///     現在の所持ポイントを反映し、強化後のポイント推移表示を更新する。
        ///     表示中のスキルが無い場合は何も表示しない。
        /// </summary>
        /// <param name="ownedPoints"> 現在の所持ポイント。 </param>
        public void SetOwnedPoints(int ownedPoints)
        {
            _lastOwnedPoints = ownedPoints;
            RefreshLevelupPointLabel();
        }

        /// <summary>
        ///     未選択表示へ戻す。
        /// </summary>
        public void Clear()
        {
            _icon.sprite = null;
            _nameLabel.text = EMPTY_SELECTION_LABEL;
            _comboLabel.text = string.Empty;
            SetComboSteps(Array.Empty<Color>());
            _skillTypeIcon.sprite = null;
            _descriptionLabel.text = string.Empty;
            _tipsLabel.text = string.Empty;
            _tipsHeadingLabel.style.display = DisplayStyle.None;
            _tipsLabel.style.display = DisplayStyle.None;
            _hasSkill = false;
            _levelLabel.text = string.Empty;
            _levelupPointLabel.text = string.Empty;
        }

        private const string ICON_NAME = "skill-detail-icon";
        private const string NAME_LABEL_NAME = "skill-name-label";
        private const string COMBO_LABEL_NAME = "skill-combo-label";
        private const string COMBO_ROW_NAME = "ComboRow";
        private const string COMBO_HEX_CLASS_NAME = "skillbuild-combo-hex";
        private const string SKILL_TYPE_ICON_NAME = "skill-type-icon";
        private const string DESCRIPTION_LABEL_NAME = "skill-description-label";
        private const string TIPS_HEADING_LABEL_NAME = "skill-tips-heading";
        private const string TIPS_LABEL_NAME = "skill-tips-label";
        private const string LEVEL_LABEL_NAME = "skill-level-label";
        private const string LEVELUP_POINT_LABEL_NAME = "skill-levelup-point-label";
        private const string EMPTY_SELECTION_LABEL = "スキルを選択してください";
        private const string EMPTY_VALUE_LABEL = "—";

        private readonly Image _icon;
        private readonly Label _nameLabel;
        private readonly Label _comboLabel;
        private readonly VisualElement _comboRow;
        private readonly Sprite _comboHexIcon;
        private readonly Image _skillTypeIcon;
        private readonly Label _descriptionLabel;
        private readonly Label _tipsHeadingLabel;
        private readonly Label _tipsLabel;
        private readonly Label _levelLabel;
        private readonly Label _levelupPointLabel;
        private bool _hasSkill;
        private int _lastOwnedPoints;

        /// <summary>
        ///     現在の表示状態(スキル選択有無・所持ポイント)から、強化後の改造P推移表示を更新する。
        /// </summary>
        private void RefreshLevelupPointLabel()
        {
            _levelupPointLabel.text = _hasSkill
                ? $"{_lastOwnedPoints} → {_lastOwnedPoints - 1}"
                : string.Empty;
        }

        /// <summary>
        ///     発動コマンドの各入力を、実際の発動色で塗った六角形アイコンの行として反映する。
        /// </summary>
        /// <param name="stepColors"> 発動コマンドの入力順に並んだ色一覧。 </param>
        private void SetComboSteps(Color[] stepColors)
        {
            ComboHexRowBuilder.Build(_comboRow, stepColors, _comboHexIcon, COMBO_HEX_CLASS_NAME);
        }
    }
}
