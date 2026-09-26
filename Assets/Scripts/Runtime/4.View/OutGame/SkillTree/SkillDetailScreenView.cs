using KillChord.Runtime.Adaptor.OutGame.SkillTree;
using KillChord.Runtime.View.OutGame.Common;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.Persistent.Localization;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillTree
{
    /// <summary>
    ///     スキル詳細画面のViewクラス。
    /// </summary>
    public class SkillDetailScreenView : ScreenViewBase, ISkillDetailShowable, ISkillDetailViewModel, IDisposable
    {
        public SkillDetailScreenView(VisualElement rootElement, OutGameUIEvent outGameUIEvent, Sprite comboHexIcon) : base(rootElement, outGameUIEvent)
        {
            _comboHexIcon = comboHexIcon;
            _skillName = rootElement.Q<Label>(name: E_NAME_SKILL_NAME_LABEL);
            _skillHeaderGenreIcon = rootElement.Q<Image>(name: E_NAME_SKILL_HEADER_GENRE_ICON);
            _skillHeaderGenreIcon.scaleMode = ScaleMode.ScaleToFit;
            _skillCommand = rootElement.Q<Label>(name: E_NAME_SKILL_COMMAND_LABEL);
            _comboRow = rootElement.Q<VisualElement>(name: E_NAME_COMBO_ROW);
            _skillGenre = rootElement.Q<Label>(name: E_NAME_SKILL_GENRE_LABEL);
            _skillGenreIcon = rootElement.Q<Image>(name: E_NAME_SKILL_GENRE_ICON);
            _skillGenreIcon.scaleMode = ScaleMode.ScaleToFit;
            _skillTypeColumn = rootElement.Q<VisualElement>(name: E_NAME_SKILL_TYPE_COLUMN);
            _effectCaptionLabel = rootElement.Q<Label>(name: E_NAME_EFFECT_CAPTION_LABEL);
            _skillDetailScrollView = rootElement.Q<VisualElement>(name: E_NAME_SKILL_DETAIL_SCROLL_VIEW);
            _skillDetail = rootElement.Q<Label>(name: E_NAME_SKILL_DETAIL_LABEL);
            if (_skillDetailScrollView is ScrollView skillDetailScrollView)
            {
                _skillDetailDragScrollManipulator = new ScrollViewDragManipulator(skillDetailScrollView);
            }
            _dividerTop = rootElement.Q<VisualElement>(name: E_NAME_DIVIDER_TOP);
            _dividerBottom = rootElement.Q<VisualElement>(name: E_NAME_DIVIDER_BOTTOM);
            _previewVideoButton = rootElement.Q<Button>(name: E_NAME_PREVIEW_BUTTON);
            _unlockButton = rootElement.Q<Button>(name: E_NAME_UNLOCK_BUTTON);
            _backButton = rootElement.Q<Button>(name: E_NAME_BACK_BUTTON);
            _outGameUIEvent = outGameUIEvent;

            RegisterEvents();
            _statusBoostLocalizedText = new LocalizedElementText(
                UI_COMMON_TABLE, "ui.skill_detail.status_boost", text =>
                {
                    _statusBoostTitle = text;
                    if (!_hasSkill)
                    {
                        _skillName.text = text;
                    }
                }, STRING_STATUS_BOOST_TITLE);
            _previewButtonLocalizedText = new LocalizedElementText(
                UI_COMMON_TABLE, "ui.skill_detail.preview", text => _previewVideoButton.text = text);
            _backButtonLocalizedText = new LocalizedElementText(
                UI_COMMON_TABLE, "ui.skill_detail.back", text => _backButton.text = text);
            Label localizedInfoHeaderLabel = rootElement.Q<Label>("InfoHeaderLabel");
            Label localizedSkillTypeHeading = rootElement.Q<Label>("SkillTypeHeading");
            Label localizedActivationComboHeading = rootElement.Q<Label>("ActivationComboHeading");
            Label localizedSkillGenreHeading = rootElement.Q<Label>("SkillGenreHeading");
            Label localizedEffectCaptionLabel = rootElement.Q<Label>("EffectCaptionLabel");
            _headingLocalizedTexts = new[]
            {
                new LocalizedElementText("UICommon", "ui.skill_detail.info", text => localizedInfoHeaderLabel.text = text, localizedInfoHeaderLabel.text),
                new LocalizedElementText("UICommon", "ui.skill.type", text => localizedSkillTypeHeading.text = text, localizedSkillTypeHeading.text),
                new LocalizedElementText("UICommon", "ui.skill_detail.activation_combo", text => localizedActivationComboHeading.text = text, localizedActivationComboHeading.text),
                new LocalizedElementText("UICommon", "ui.skill_detail.genre", text => localizedSkillGenreHeading.text = text, localizedSkillGenreHeading.text),
                new LocalizedElementText("UICommon", "ui.skill_detail.effect", text => localizedEffectCaptionLabel.text = text, localizedEffectCaptionLabel.text)
            };
        }

        /// <summary>
        ///     画面表示用データを反映する。
        /// </summary>
        /// <param name="dto"></param>
        public void Apply(SkillDetailDTO dto)
        {
            _currentNodeId = dto.SkillNodeId;
            _hasSkill = dto.HasSkill;
            _skillName.text = dto.HasSkill ? dto.SkillName : _statusBoostTitle;
            _skillHeaderGenreIcon.sprite = dto.HasSkill ? dto.SkillIcon : null;
            _skillHeaderGenreIcon.style.display = _skillHeaderGenreIcon.sprite == null ? DisplayStyle.None : DisplayStyle.Flex;
            _skillCommand.text = dto.SkillCommand;
            ComboHexRowBuilder.Build(
                _comboRow,
                dto.HasSkill ? dto.ComboStepColors : Array.Empty<Color>(),
                _comboHexIcon,
                COMBO_HEX_CLASS_NAME);
            _skillGenre.text = dto.SkillGenre;
            _skillGenreIcon.sprite = dto.SkillGenreIcon;
            _skillGenreIcon.style.display = dto.SkillGenreIcon == null ? DisplayStyle.None : DisplayStyle.Flex;
            _skillDetail.text = dto.SkillDetail;

            DisplayStyle skillOnlyDisplay = dto.HasSkill ? DisplayStyle.Flex : DisplayStyle.None;
            _effectCaptionLabel.style.display = skillOnlyDisplay;
            // プレビュー再生ボタンは一時的に非表示にしている。
            _previewVideoButton.style.display = DisplayStyle.None;
            _skillTypeColumn.style.display = skillOnlyDisplay;
            _dividerTop.style.display = dto.HasSkill ? DisplayStyle.None : DisplayStyle.Flex;
            _dividerBottom.style.display = dto.HasSkill ? DisplayStyle.None : DisplayStyle.Flex;

            TextAnchor textAlign = dto.HasSkill ? TextAnchor.UpperLeft : TextAnchor.UpperCenter;
            _skillName.style.unityTextAlign = textAlign;
            _skillDetail.style.unityTextAlign = textAlign;

            bool unlockButtonEnable = !dto.Unlocked && dto.CanUnlock;
            _unlockButtonLocalizedText?.Dispose();
            _unlockButtonLocalizedText = dto.Unlocked
                ? new LocalizedElementText(
                    UI_COMMON_TABLE, "ui.skill_detail.unlocked", text => _unlockButton.text = text)
                : new LocalizedElementText(
                    UI_COMMON_TABLE,
                    "ui.skill_detail.unlock_cost_format",
                    text => _unlockButton.text = text,
                    arguments: new object[] { dto.UnlockCost });
            _unlockButton.SetEnabled(unlockButtonEnable);
            _previewVideoButton.SetEnabled(dto.HasPreviewVideo);
            IsUnlockAvailable = unlockButtonEnable;
        }

        public override void Dispose()
        {
            foreach (LocalizedElementText localizedText in _headingLocalizedTexts)
            {
                localizedText.Dispose();
            }
            base.Dispose();
            _statusBoostLocalizedText.Dispose();
            _unlockButtonActivation?.Dispose();
            _backButtonActivation?.Dispose();
            _previewButtonLocalizedText?.Dispose();
            _backButtonLocalizedText?.Dispose();
            _unlockButtonLocalizedText?.Dispose();

            if (_skillDetailDragScrollManipulator != null)
            {
                _skillDetailDragScrollManipulator.target = null;
                _skillDetailDragScrollManipulator = null;
            }
        }

        private readonly LocalizedElementText[] _headingLocalizedTexts;
        private readonly LocalizedElementText _statusBoostLocalizedText;
        private string _statusBoostTitle = STRING_STATUS_BOOST_TITLE;
        private bool _hasSkill;

        private const string E_NAME_SKILL_NAME_LABEL = "SkillNameLabel";
        private const string E_NAME_SKILL_HEADER_GENRE_ICON = "SkillHeaderGenreIcon";
        private const string E_NAME_SKILL_COMMAND_LABEL = "SkillCommandLabel";
        private const string E_NAME_COMBO_ROW = "ComboRow";
        private const string COMBO_HEX_CLASS_NAME = "skilltree-combo-hex";
        private const string E_NAME_SKILL_GENRE_LABEL = "SkillGenreLabel";
        private const string E_NAME_SKILL_GENRE_ICON = "SkillGenreIcon";
        private const string E_NAME_SKILL_TYPE_COLUMN = "SkillTypeColumn";
        private const string E_NAME_EFFECT_CAPTION_LABEL = "EffectCaptionLabel";
        private const string E_NAME_SKILL_DETAIL_SCROLL_VIEW = "SkillDetailScrollView";
        private const string E_NAME_SKILL_DETAIL_LABEL = "SkillDetailLabel";
        private const string E_NAME_DIVIDER_TOP = "DividerTop";
        private const string E_NAME_DIVIDER_BOTTOM = "DividerBottom";
        private const string E_NAME_PREVIEW_BUTTON = "PreviewButton";
        private const string E_NAME_UNLOCK_BUTTON = "UnlockButton";
        private const string E_NAME_BACK_BUTTON = "BackButton";
        private const string STRING_STATUS_BOOST_TITLE = "ステータス強化";
        private const string UI_COMMON_TABLE = "UICommon";

        private Label _skillName;
        private Image _skillHeaderGenreIcon;
        private Label _skillCommand;
        private VisualElement _comboRow;
        private readonly Sprite _comboHexIcon;
        private Label _skillGenre;
        private Image _skillGenreIcon;
        private VisualElement _skillTypeColumn;
        private Label _effectCaptionLabel;
        private VisualElement _skillDetailScrollView;
        private ScrollViewDragManipulator _skillDetailDragScrollManipulator;
        private Label _skillDetail;
        private VisualElement _dividerTop;
        private VisualElement _dividerBottom;
        private Button _previewVideoButton;
        private Button _unlockButton;
        private LocalizedElementText _previewButtonLocalizedText;
        private LocalizedElementText _backButtonLocalizedText;
        private LocalizedElementText _unlockButtonLocalizedText;

        /// <inheritdoc />
        /// <remarks>
        ///     解放ボタンが押せる状態ならそこへ自動フォーカスする。
        ///     押せない場合(解放済み等)はフォーカス対象なし。呼び出し側
        ///     (<see cref="IsUnlockAvailable"/> を参照)がそもそもパネルへ
        ///     フォーカスを移さない判断をするため、ここでは代替先を探さない。
        /// </remarks>
        protected override VisualElement InitialFocusElement =>
            _unlockButton.enabledInHierarchy ? _unlockButton : null;

        /// <summary> 現在の選択ノードが解放操作可能かどうか。 </summary>
        public bool IsUnlockAvailable { get; private set; }

        /// <inheritdoc />
        protected override VisualElement CancelTargetElement => _backButton;

        private Button _backButton;
        private OutGameUIEvent _outGameUIEvent;
        private int _currentNodeId;
        private IDisposable _unlockButtonActivation;
        private IDisposable _backButtonActivation;

        /// <summary>
        ///     各画面要素のイベント登録を行う。
        /// </summary>
        private void RegisterEvents()
        {
            _unlockButton.MakeNavigable();
            // キャンセル操作で戻れるため、フォーカス移動の対象からは外す。
            _backButton.ExcludeFromNavigation();
            // プレビュー再生ボタンは一時的に非表示にしており、フォーカス対象からも外す。
            _previewVideoButton.ExcludeFromNavigation();

            // クリックと決定操作(NavigationSubmitEvent)の両方を1つの処理へ統合する。
            // Button.clicked はコントローラーの決定操作には反応しないため、これが必須。
            _unlockButtonActivation = _unlockButton.RegisterActivation(OnUnlockButtonActivated);
            _backButtonActivation = _backButton.RegisterActivation(OnBackButtonActivated);
        }

        /// <summary>
        ///     スキル解放ボタン作動時の処理。解放確認ダイアログを開く。
        /// </summary>
        private void OnUnlockButtonActivated()
        {
            _outGameUIEvent.OnSkillUnlockConfirmationRequested?.Invoke();
        }

        /// <summary>
        ///     スキル詳細の戻るボタン作動時の処理。
        /// </summary>
        private void OnBackButtonActivated()
        {
            _outGameUIEvent.OnSkillDetailClosed?.Invoke(_currentNodeId);
        }
    }
}
