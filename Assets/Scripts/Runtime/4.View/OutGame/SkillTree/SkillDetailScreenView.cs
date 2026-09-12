using KillChord.Runtime.Adaptor.OutGame.SkillTree;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.Screen;
using System;
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
            _skillDetail = rootElement.Q<Label>(name: E_NAME_SKILL_DETAIL_LABEL);
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
            _skillDetail.text = dto.SkillDetail;
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
        }

        private const string E_NAME_SKILL_DETAIL_LABEL = "SkillDetailLabel";
        private const string E_NAME_PREVIEW_BUTTON = "PreviewButton";
        private const string E_NAME_UNLOCK_BUTTON = "UnlockButton";
        private const string E_NAME_BACK_BUTTON = "BackButton";
        private const string STRING_UNLOCK_BUTTON_TEXT_UNLOCK_COST = "解放する　必要ポイント：";
        private const string STRING_UNLOCK_BUTTON_TEXT_ALREADY_UNLOCKED = "解放済み";

        private Label _skillDetail;
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

            // 処理を ClickEvent で受けているため、決定操作もクリックとして流し込む。
            _unlockButton.EnableSubmitAsClick();
            // キャンセル操作で戻れるため、フォーカス移動の対象からは外す。
            _backButton.ExcludeFromNavigation();
            _previewVideoButton.EnableSubmitAsClick();
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
