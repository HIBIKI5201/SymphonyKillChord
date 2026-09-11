using KillChord.Runtime.Adaptor.OutGame.SkillTree;
using KillChord.Runtime.View.OutGame.Common;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.Screen;
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
        public SkillDetailScreenView(VisualElement rootElement, OutGameUIEvent outGameUIEvent) : base(rootElement, outGameUIEvent)
        {
            _skillName = rootElement.Q<Label>(name: E_NAME_SKILL_NAME_LABEL);
            _skillHeaderGenreIcon = rootElement.Q<Image>(name: E_NAME_SKILL_HEADER_GENRE_ICON);
            _skillCommand = rootElement.Q<Label>(name: E_NAME_SKILL_COMMAND_LABEL);
            _comboRow = rootElement.Q<VisualElement>(name: E_NAME_COMBO_ROW);
            _genreRow = rootElement.Q<VisualElement>(name: E_NAME_GENRE_ROW);
            _skillGenre = rootElement.Q<Label>(name: E_NAME_SKILL_GENRE_LABEL);
            _skillGenreIcon = rootElement.Q<Image>(name: E_NAME_SKILL_GENRE_ICON);
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
        }

        /// <summary>
        ///     画面表示用データを反映する。
        /// </summary>
        /// <param name="dto"></param>
        public void Apply(SkillDetailDTO dto)
        {
            _currentNodeId = dto.SkillNodeId;
            _skillName.text = dto.HasSkill ? dto.SkillName : STRING_STATUS_BOOST_TITLE;
            _skillHeaderGenreIcon.sprite = dto.SkillGenreIcon;
            _skillHeaderGenreIcon.style.display = dto.SkillGenreIcon == null ? DisplayStyle.None : DisplayStyle.Flex;
            _skillCommand.text = dto.SkillCommand;
            ComboHexRowBuilder.Build(
                _comboRow,
                dto.HasSkill ? dto.ComboStepColors : Array.Empty<Color>(),
                COMBO_HEX_CLASS_NAME,
                COMBO_HEX_CAP_TOP_CLASS_NAME,
                COMBO_HEX_RECT_CLASS_NAME,
                COMBO_HEX_CAP_BOTTOM_CLASS_NAME);
            _skillGenre.text = dto.SkillGenre;
            _skillGenreIcon.sprite = dto.SkillGenreIcon;
            _skillGenreIcon.style.display = dto.SkillGenreIcon == null ? DisplayStyle.None : DisplayStyle.Flex;
            _skillDetail.text = dto.SkillDetail;

            DisplayStyle skillOnlyDisplay = dto.HasSkill ? DisplayStyle.Flex : DisplayStyle.None;
            _genreRow.style.display = skillOnlyDisplay;
            _effectCaptionLabel.style.display = skillOnlyDisplay;
            _previewVideoButton.style.display = skillOnlyDisplay;
            _dividerTop.style.display = dto.HasSkill ? DisplayStyle.None : DisplayStyle.Flex;
            _dividerBottom.style.display = dto.HasSkill ? DisplayStyle.None : DisplayStyle.Flex;

            TextAnchor textAlign = dto.HasSkill ? TextAnchor.UpperLeft : TextAnchor.UpperCenter;
            _skillName.style.unityTextAlign = textAlign;
            _skillDetail.style.unityTextAlign = textAlign;

            bool unlockButtonEnable = !dto.Unlocked && dto.CanUnlock;
            _unlockButton.text = dto.Unlocked ? STRING_UNLOCK_BUTTON_TEXT_ALREADY_UNLOCKED
                : STRING_UNLOCK_BUTTON_TEXT_UNLOCK_COST + dto.UnlockCost.ToString();
            _unlockButton.SetEnabled(unlockButtonEnable);
            _previewVideoButton.SetEnabled(dto.HasPreviewVideo);
        }

        public override void Dispose()
        {
            base.Dispose();
            _unlockButton.UnregisterCallback<ClickEvent>(OnUnlockButtonClicked);
            _backButton.UnregisterCallback<ClickEvent>(OnBackButtonClicked);
            _previewVideoButton.UnregisterCallback<ClickEvent>(OnPreviewButtonClicked);

            if (_skillDetailDragScrollManipulator != null)
            {
                _skillDetailDragScrollManipulator.target = null;
                _skillDetailDragScrollManipulator = null;
            }
        }

        private const string E_NAME_SKILL_NAME_LABEL = "SkillNameLabel";
        private const string E_NAME_SKILL_HEADER_GENRE_ICON = "SkillHeaderGenreIcon";
        private const string E_NAME_SKILL_COMMAND_LABEL = "SkillCommandLabel";
        private const string E_NAME_COMBO_ROW = "ComboRow";
        private const string COMBO_HEX_CLASS_NAME = "skilltree-combo-hex";
        private const string COMBO_HEX_CAP_TOP_CLASS_NAME = "skilltree-combo-hex-cap-top";
        private const string COMBO_HEX_RECT_CLASS_NAME = "skilltree-combo-hex-rect";
        private const string COMBO_HEX_CAP_BOTTOM_CLASS_NAME = "skilltree-combo-hex-cap-bottom";
        private const string E_NAME_GENRE_ROW = "GenreRow";
        private const string E_NAME_SKILL_GENRE_LABEL = "SkillGenreLabel";
        private const string E_NAME_SKILL_GENRE_ICON = "SkillGenreIcon";
        private const string E_NAME_EFFECT_CAPTION_LABEL = "EffectCaptionLabel";
        private const string E_NAME_SKILL_DETAIL_SCROLL_VIEW = "SkillDetailScrollView";
        private const string E_NAME_SKILL_DETAIL_LABEL = "SkillDetailLabel";
        private const string E_NAME_DIVIDER_TOP = "DividerTop";
        private const string E_NAME_DIVIDER_BOTTOM = "DividerBottom";
        private const string E_NAME_PREVIEW_BUTTON = "PreviewButton";
        private const string E_NAME_UNLOCK_BUTTON = "UnlockButton";
        private const string E_NAME_BACK_BUTTON = "BackButton";
        private const string STRING_UNLOCK_BUTTON_TEXT_UNLOCK_COST = "解放する　必要ポイント：";
        private const string STRING_UNLOCK_BUTTON_TEXT_ALREADY_UNLOCKED = "解放済み";
        private const string STRING_STATUS_BOOST_TITLE = "ステータス強化";

        private Label _skillName;
        private Image _skillHeaderGenreIcon;
        private Label _skillCommand;
        private VisualElement _comboRow;
        private VisualElement _genreRow;
        private Label _skillGenre;
        private Image _skillGenreIcon;
        private Label _effectCaptionLabel;
        private VisualElement _skillDetailScrollView;
        private ScrollViewDragManipulator _skillDetailDragScrollManipulator;
        private Label _skillDetail;
        private VisualElement _dividerTop;
        private VisualElement _dividerBottom;
        private Button _previewVideoButton;
        private Button _unlockButton;
        /// <inheritdoc />
        protected override VisualElement CancelTargetElement => _backButton;

        private Button _backButton;
        private OutGameUIEvent _outGameUIEvent;
        private int _currentNodeId;

        /// <summary>
        ///     各画面要素のイベント登録を行う。
        /// </summary>
        private void RegisterEvents()
        {
            _unlockButton.RegisterCallback<ClickEvent>(OnUnlockButtonClicked);
            _backButton.RegisterCallback<ClickEvent>(OnBackButtonClicked);
            _previewVideoButton.RegisterCallback<ClickEvent>(OnPreviewButtonClicked);

            // 処理を ClickEvent で受けているため、コントローラーのフォーカス移動対象にして決定操作もクリックとして流し込む。
            _unlockButton.MakeNavigable();
            // キャンセル操作で戻れるため、フォーカス移動の対象からは外す。
            _backButton.ExcludeFromNavigation();
            _previewVideoButton.MakeNavigable();
        }

        /// <summary>
        ///     スキル解放ボタン押下時の処理。
        /// </summary>
        /// <param name="ctx"></param>
        private void OnUnlockButtonClicked(ClickEvent ctx)
        {
            _outGameUIEvent.OnSkillUnlocked?.Invoke();
        }

        /// <summary>
        ///     スキル詳細の戻るボタンを押下時の処理。
        /// </summary>
        /// <param name="ctx"></param>
        private void OnBackButtonClicked(ClickEvent ctx)
        {
            _outGameUIEvent.OnSkillDetailClosed?.Invoke(_currentNodeId);
        }

        /// <summary>
        ///     スキルプレビューボタンを押下時の処理。
        /// </summary>
        /// <param name="ctx"></param>
        private void OnPreviewButtonClicked(ClickEvent ctx)
        {
            _outGameUIEvent.OnSkillPreviewButtonClicked?.Invoke();
        }
    }
}
