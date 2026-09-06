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
            _unlockButtonActivation?.Dispose();
            _backButtonActivation?.Dispose();
            _previewVideoButtonActivation?.Dispose();
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
        private IDisposable _unlockButtonActivation;
        private IDisposable _backButtonActivation;
        private IDisposable _previewVideoButtonActivation;

        /// <summary>
        ///     各画面要素のイベント登録を行う。
        /// </summary>
        private void RegisterEvents()
        {
            _unlockButton.MakeNavigable();
            // キャンセル操作で戻れるため、フォーカス移動の対象からは外す。
            _backButton.ExcludeFromNavigation();
            _previewVideoButton.MakeNavigable();

            _unlockButtonActivation = _unlockButton.RegisterActivation(HandleUnlockButtonActivationHandler);
            _backButtonActivation = _backButton.RegisterActivation(HandleBackButtonActivationHandler);
            _previewVideoButtonActivation = _previewVideoButton.RegisterActivation(HandlePreviewButtonActivationHandler);
        }

        /// <summary>
        ///     スキル解放ボタン押下時の処理。
        /// </summary>
        private void HandleUnlockButtonActivationHandler()
        {
            _outGameUIEvent.OnSkillUnlocked?.Invoke();
        }

        /// <summary>
        ///     スキル詳細の戻るボタンを押下時の処理。
        /// </summary>
        private void HandleBackButtonActivationHandler()
        {
            _outGameUIEvent.OnSkillDetailClosed?.Invoke(_currentNodeId);
        }

        /// <summary>
        ///     スキルプレビューボタンを押下時の処理。
        /// </summary>
        private void HandlePreviewButtonActivationHandler()
        {
            _outGameUIEvent.OnSkillPreviewButtonClicked?.Invoke();
        }
    }
}
