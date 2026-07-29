using KillChord.Runtime.Adaptor.OutGame.SkillBuild;
using KillChord.Runtime.View.OutGame.SkillBuild;
using R3;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     改造画面 View クラス。
    /// </summary>
    public sealed class SkillBuildScreenView : ScreenViewBase
    {

        /// <summary>
        ///     View を初期化する。
        /// </summary>
        /// <param name="rootElement"> 画面ルート要素。 </param>
        /// <param name="outGameUIEvent"> 画面イベント。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillBuildScreenView(VisualElement rootElement, OutGameUIEvent outGameUIEvent)
            : base(rootElement, outGameUIEvent)
        {
            _backButton = rootElement.Q<Button>(BACKBUTTON_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillBuildScreenView)}] {BACKBUTTON_NAME} が見つかりませんでした。");
            _skillElementList = rootElement.Q<VisualElement>(className: SKILL_ELEMENT_LIST_CLASS_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillBuildScreenView)}] class={SKILL_ELEMENT_LIST_CLASS_NAME} が見つかりませんでした。");
            _skillBuildSaveButton = rootElement.Q<Button>(SKILLBUILD_SAVEBUTTON_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillBuildScreenView)}] {SKILLBUILD_SAVEBUTTON_NAME} が見つかりませんでした。");
            _skillLevelUpButton = rootElement.Q<Button>(SKILLLEVELUP_BUTTON_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillBuildScreenView)}] {SKILLLEVELUP_BUTTON_NAME} が見つかりませんでした。");
            _ownedPointsLabel = rootElement.Q<Label>(OWNED_POINTS_LABEL_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillBuildScreenView)}] {OWNED_POINTS_LABEL_NAME} が見つかりませんでした。");

            VisualElement skillDetailRoot = rootElement.Q<VisualElement>(SKILL_DETAIL_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillBuildScreenView)}] {SKILL_DETAIL_NAME} が見つかりませんでした。");
            _skillDetailView = new SkillDetailView(skillDetailRoot);
            _skillDetailView.Clear();

            _skillBuildDialog = rootElement.Q<VisualElement>(SKILLBUILD_DIALOG_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillBuildScreenView)}] {SKILLBUILD_DIALOG_NAME} が見つかりませんでした。");
            _unsavedChangesDialogOverlay = _skillBuildDialog.Q<VisualElement>(DIALOG_BACKGROUND_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillBuildScreenView)}] {DIALOG_BACKGROUND_NAME} が見つかりませんでした。");
            _unsavedDiscardAndCloseButton = _skillBuildDialog.Q<Button>(DISCARD_AND_CLOSE_BUTTON_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillBuildScreenView)}] {DISCARD_AND_CLOSE_BUTTON_NAME} が見つかりませんでした。");
            _unsavedSaveAndCloseButton = _skillBuildDialog.Q<Button>(SAVE_AND_CLOSE_BUTTON_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillBuildScreenView)}] {SAVE_AND_CLOSE_BUTTON_NAME} が見つかりませんでした。");

            _dialogPanel = GetDialogPanel(_unsavedChangesDialogOverlay);
            HideUnsavedChangesDialog();
            RegisterButtonCallback();
        }

        /// <summary>
        ///     スキル一覧表示を初期化する。
        /// </summary>
        /// <param name="skillElementTemplate"> スキル要素テンプレート。 </param>
        /// <param name="onSkillElementCreated"> スキル要素生成時コールバック。 </param>
        public void InitializeSkillList(
            VisualTreeAsset skillElementTemplate,
            Action<VisualElement> onSkillElementCreated = null)
        {
            if (_skillListView != null)
            {
                return;
            }

            _skillListView = new SkillListView(
                _skillElementList,
                skillElementTemplate,
                onSkillElementCreated);
            _skillBuildSlotLayout = new SkillBuildSlotLayout(
                RootElement,
                _skillElementList,
                _skillListView.FindSkillElementRoot);
            _skillListView.OnSkillSelected += HandleSkillSelectedHandler;
        }

        /// <summary>
        ///     ViewModel をバインドする。
        /// </summary>
        /// <param name="viewModel"> バインド対象。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void Bind(ISkillBuildViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            if (_skillListView == null)
            {
                throw new InvalidOperationException("先に SkillListView の初期化が必要です。");
            }

            Unbind();
            _viewModel = viewModel;
            _subscriptions = new CompositeDisposable();

            _viewModel.Skills
                .Subscribe(HandleSkillsChangedHandler)
                .AddTo(_subscriptions);
            _viewModel.Slots
                .Subscribe(HandleSlotsChangedHandler)
                .AddTo(_subscriptions);
            _viewModel.ExplicitlySelectedSkillId
                .Subscribe(HandleSelectedSkillChangedHandler)
                .AddTo(_subscriptions);
            _viewModel.DisplayedSkill
                .Subscribe(HandleDisplayedSkillChangedHandler)
                .AddTo(_subscriptions);
            _viewModel.OwnedPoints
                .Subscribe(HandleOwnedPointsChangedHandler)
                .AddTo(_subscriptions);
        }

        /// <summary>
        ///     ViewModel の購読を解除する。
        /// </summary>
        public void Unbind()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _viewModel = null;
            _currentSlots = Array.Empty<SkillBuildSlotState>();
            _currentSelectedSkillId = null;
        }

        /// <summary>
        ///     リソースを解放する。
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            Unbind();
            UnregisterButtonCallback();

            if (_skillListView != null)
            {
                _skillListView.OnSkillSelected -= HandleSkillSelectedHandler;
                _skillListView.Dispose();
                _skillListView = null;
            }

            _skillBuildSlotLayout = null;
        }

        private const string BACKBUTTON_NAME = "BackButton";
        private const string SKILLBUILD_SAVEBUTTON_NAME = "SkillBuildSaveButton";
        private const string SKILLLEVELUP_BUTTON_NAME = "SkillLevelUpButton";
        private const string SKILL_DETAIL_NAME = "SkillDetail";
        private const string SKILL_ELEMENT_LIST_CLASS_NAME = "skill-element-list";
        private const string SKILLBUILD_DIALOG_NAME = "SkillBuildDialog";
        private const string DIALOG_BACKGROUND_NAME = "BackGround";
        private const string DISCARD_AND_CLOSE_BUTTON_NAME = "DiscardAndCloseButton";
        private const string SAVE_AND_CLOSE_BUTTON_NAME = "SaveAndCloseButton";
        private const string OWNED_POINTS_LABEL_NAME = "OwnedPointsLabel";

        private readonly Button _backButton;
        private readonly Button _skillBuildSaveButton;
        private readonly Button _skillLevelUpButton;
        private readonly Label _ownedPointsLabel;
        private readonly VisualElement _skillElementList;
        private readonly SkillDetailView _skillDetailView;
        private readonly VisualElement _skillBuildDialog;
        private readonly VisualElement _dialogPanel;
        private readonly Button _unsavedSaveAndCloseButton;
        private readonly Button _unsavedDiscardAndCloseButton;
        private readonly VisualElement _unsavedChangesDialogOverlay;

        private SkillListView _skillListView;
        private SkillBuildSlotLayout _skillBuildSlotLayout;
        private ISkillBuildViewModel _viewModel;
        private CompositeDisposable _subscriptions;
        private IReadOnlyList<SkillBuildSlotState> _currentSlots =
            Array.Empty<SkillBuildSlotState>();
        private int? _currentSelectedSkillId;
        private bool _isSavingSkillBuild;

        /// <summary>
        ///     ボタンのコールバックを登録する。
        /// </summary>
        private void RegisterButtonCallback()
        {
            _backButton.RegisterCallback<ClickEvent>(HandleBackButtonClickedHandler);
            _skillBuildSaveButton.RegisterCallback<ClickEvent>(HandleSkillBuildSaveButtonClickedHandler);
            _skillLevelUpButton.RegisterCallback<ClickEvent>(HandleSkillLevelUpButtonClickedHandler);
            _unsavedSaveAndCloseButton.RegisterCallback<ClickEvent>(HandleUnsavedSaveAndCloseButtonClickedHandler);
            _unsavedDiscardAndCloseButton.RegisterCallback<ClickEvent>(HandleUnsavedDiscardAndCloseButtonClickedHandler);
            _unsavedChangesDialogOverlay.RegisterCallback<ClickEvent>(HandleUnsavedDialogBackgroundClickedHandler);
            _dialogPanel.RegisterCallback<ClickEvent>(HandleUnsavedDialogPanelClickedHandler);
        }

        /// <summary>
        ///     ボタンのコールバックを解除する。
        /// </summary>
        private void UnregisterButtonCallback()
        {
            _backButton.UnregisterCallback<ClickEvent>(HandleBackButtonClickedHandler);
            _skillBuildSaveButton.UnregisterCallback<ClickEvent>(HandleSkillBuildSaveButtonClickedHandler);
            _skillLevelUpButton.UnregisterCallback<ClickEvent>(HandleSkillLevelUpButtonClickedHandler);
            _unsavedSaveAndCloseButton.UnregisterCallback<ClickEvent>(HandleUnsavedSaveAndCloseButtonClickedHandler);
            _unsavedDiscardAndCloseButton.UnregisterCallback<ClickEvent>(HandleUnsavedDiscardAndCloseButtonClickedHandler);
            _unsavedChangesDialogOverlay.UnregisterCallback<ClickEvent>(HandleUnsavedDialogBackgroundClickedHandler);
            _dialogPanel.UnregisterCallback<ClickEvent>(HandleUnsavedDialogPanelClickedHandler);
        }

        /// <summary>
        ///     所持スキル一覧を表示へ反映する。
        /// </summary>
        /// <param name="skills"> 所持スキル一覧。 </param>
        private void HandleSkillsChangedHandler(IReadOnlyList<SkillViewData> skills)
        {
            _skillListView.SetSkills(skills);
            _skillListView.SetSelectedSkill(_currentSelectedSkillId);
            SyncEquippedSkillsToSlots();
        }

        /// <summary>
        ///     スロット状態を表示へ反映する。
        /// </summary>
        /// <param name="slots"> スロット状態。 </param>
        private void HandleSlotsChangedHandler(IReadOnlyList<SkillBuildSlotState> slots)
        {
            IReadOnlyList<SkillBuildSlotState> previousSlots = _currentSlots;
            _currentSlots = slots ?? Array.Empty<SkillBuildSlotState>();
            _skillBuildSlotLayout?.ApplyChanges(
                previousSlots,
                _currentSlots);
        }

        /// <summary>
        ///     明示選択表示を更新する。
        /// </summary>
        /// <param name="skillId"> 明示選択 ID。 </param>
        private void HandleSelectedSkillChangedHandler(int? skillId)
        {
            _currentSelectedSkillId = skillId;
            _skillListView.SetSelectedSkill(skillId);
        }

        /// <summary>
        ///     詳細表示を更新する。
        /// </summary>
        /// <param name="skill"> 表示対象。 </param>
        private void HandleDisplayedSkillChangedHandler(SkillViewData? skill)
        {
            if (!skill.HasValue)
            {
                _skillDetailView.Clear();
                return;
            }

            SkillViewData data = skill.Value;
            _skillDetailView.Apply(in data);
        }

        /// <summary>
        ///     所持ポイントを表示へ反映する。
        /// </summary>
        /// <param name="ownedPoints"> 所持ポイント。 </param>
        private void HandleOwnedPointsChangedHandler(int ownedPoints)
        {
            _ownedPointsLabel.text = ownedPoints.ToString();
        }

        /// <summary>
        ///     一覧要素の選択を ViewModel へ渡す。
        /// </summary>
        /// <param name="skillId"> スキル ID。 </param>
        private void HandleSkillSelectedHandler(int skillId)
        {
            _viewModel?.SelectSkill(skillId);
        }

        /// <summary>
        ///     現在の装備状態をスロットへ反映する。
        /// </summary>
        private void SyncEquippedSkillsToSlots()
        {
            _skillBuildSlotLayout?.ApplyAll(_currentSlots);
        }

        /// <summary>
        ///     戻るボタンを処理する。
        /// </summary>
        /// <param name="evt"> クリックイベント。 </param>
        private void HandleBackButtonClickedHandler(ClickEvent evt)
        {
            if (_viewModel != null && _viewModel.HasUnsavedChanges())
            {
                ShowUnsavedChangesDialog();
                return;
            }

            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     保存ボタンを処理する。
        /// </summary>
        /// <param name="evt"> クリックイベント。 </param>
        private async void HandleSkillBuildSaveButtonClickedHandler(ClickEvent evt)
        {
            await TrySaveCurrentSkillBuildAsync();
        }

        /// <summary>
        ///     レベルアップボタンを処理する。
        /// </summary>
        /// <param name="evt"> クリックイベント。 </param>
        private void HandleSkillLevelUpButtonClickedHandler(ClickEvent evt)
        {
            OutGameUIEvent.OnSkillLevelUp?.Invoke();
        }

        /// <summary>
        ///     保存して閉じる操作を処理する。
        /// </summary>
        /// <param name="evt"> クリックイベント。 </param>
        private async void HandleUnsavedSaveAndCloseButtonClickedHandler(ClickEvent evt)
        {
            bool isSaved = await TrySaveCurrentSkillBuildAsync();
            if (!isSaved)
            {
                return;
            }

            HideUnsavedChangesDialog();
            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     変更を破棄して閉じる操作を処理する。
        /// </summary>
        /// <param name="evt"> クリックイベント。 </param>
        private void HandleUnsavedDiscardAndCloseButtonClickedHandler(ClickEvent evt)
        {
            _viewModel?.ResetSlots();
            HideUnsavedChangesDialog();
            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     ダイアログ背景クリックを処理する。
        /// </summary>
        /// <param name="evt"> クリックイベント。 </param>
        private void HandleUnsavedDialogBackgroundClickedHandler(ClickEvent evt)
        {
            HideUnsavedChangesDialog();
        }

        /// <summary>
        ///     ダイアログ本体から背景への伝播を止める。
        /// </summary>
        /// <param name="evt"> クリックイベント。 </param>
        private void HandleUnsavedDialogPanelClickedHandler(ClickEvent evt)
        {
            evt.StopPropagation();
        }

        /// <summary>
        ///     未保存変更ダイアログを表示する。
        /// </summary>
        private void ShowUnsavedChangesDialog()
        {
            _skillBuildDialog.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        ///     未保存変更ダイアログを隠す。
        /// </summary>
        private void HideUnsavedChangesDialog()
        {
            _skillBuildDialog.style.display = DisplayStyle.None;
        }

        /// <summary>
        ///     ダイアログ本体を取得する。
        /// </summary>
        /// <param name="dialogBackground"> ダイアログ背景。 </param>
        /// <returns> ダイアログ本体。 </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        private VisualElement GetDialogPanel(VisualElement dialogBackground)
        {
            if (dialogBackground == null)
            {
                throw new ArgumentNullException(nameof(dialogBackground));
            }

            if (dialogBackground.childCount <= 0)
            {
                throw new InvalidOperationException(
                    $"[{nameof(SkillBuildScreenView)}] {DIALOG_BACKGROUND_NAME} 配下にダイアログ本体が見つかりませんでした。");
            }

            return dialogBackground.ElementAt(0);
        }

        /// <summary>
        ///     現在のスキルビルド保存を試行する。
        /// </summary>
        /// <returns> 保存に成功した場合は true。 </returns>
        private async ValueTask<bool> TrySaveCurrentSkillBuildAsync()
        {
            if (_isSavingSkillBuild || _viewModel == null)
            {
                return false;
            }

            _isSavingSkillBuild = true;
            try
            {
                return await _viewModel.SaveAsync();
            }
            finally
            {
                _isSavingSkillBuild = false;
            }
        }
    }
}
