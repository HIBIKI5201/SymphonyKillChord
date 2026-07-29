using KillChord.Runtime.View.OutGame.SkillBuild;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     改造画面 View クラス。
    /// </summary>
    public sealed class SkillBuildScreenView : ScreenViewBase
    {

        /// <summary>
        ///     View を初期化します。
        /// </summary>
        /// <param name="rootElement"> 画面ルート要素。 </param>
        /// <param name="outGameUIEvent"> 画面イベント。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillBuildScreenView(VisualElement rootElement, OutGameUIEvent outGameUIEvent)
            : base(rootElement, outGameUIEvent)
        {
            _backButton = rootElement.Q<Button>(BACKBUTTON_NAME)
                ?? throw new ArgumentNullException(
                    $"[{nameof(SkillBuildScreenView)}] {BACKBUTTON_NAME} が見つかりませんでした。");

            _skillElementList = rootElement.Q<VisualElement>(className: SKILL_ELEMENT_LIST_CLASS_NAME)
                ?? throw new ArgumentNullException(
                    $"[{nameof(SkillBuildScreenView)}] class={SKILL_ELEMENT_LIST_CLASS_NAME} が見つかりませんでした。");

            _skillBuildSaveButton = rootElement.Q<Button>(SKILLBUILD_SAVEBUTTON_NAME)
                ?? throw new ArgumentNullException(
                    $"[{nameof(SkillBuildScreenView)}] {SKILLBUILD_SAVEBUTTON_NAME} が見つかりませんでした。");

            _skillLevelUpButton = rootElement.Q<Button>(SKILLLEVELUP_BUTTON_NAME)
                ?? throw new ArgumentNullException(
                    $"[{nameof(SkillBuildScreenView)}] {SKILLLEVELUP_BUTTON_NAME} が見つかりませんでした。");

            _skillBuildDialog = rootElement.Q<VisualElement>(SKILLBUILD_DIALOG_NAME)
                ?? throw new ArgumentNullException(
                    $"[{nameof(SkillBuildScreenView)}] {SKILLBUILD_DIALOG_NAME} が見つかりませんでした。");

            _unsavedChangesDialogOverlay = _skillBuildDialog.Q<VisualElement>(DIALOG_BACKGROUND_NAME)
                ?? throw new ArgumentNullException(
                    $"[{nameof(SkillBuildScreenView)}] {DIALOG_BACKGROUND_NAME} が見つかりませんでした。");

            _unsavedDiscardAndCloseButton = _skillBuildDialog.Q<Button>(DISCARD_AND_CLOSE_BUTTON_NAME)
                ?? throw new ArgumentNullException(
                    $"[{nameof(SkillBuildScreenView)}] {DISCARD_AND_CLOSE_BUTTON_NAME} が見つかりませんでした。");

            _unsavedSaveAndCloseButton = _skillBuildDialog.Q<Button>(SAVE_AND_CLOSE_BUTTON_NAME)
                ?? throw new ArgumentNullException(
                    $"[{nameof(SkillBuildScreenView)}] {SAVE_AND_CLOSE_BUTTON_NAME} が見つかりませんでした。");

            _dialogPanel = GetDialogPanel(_unsavedChangesDialogOverlay);
            HideUnsavedChangesDialog();

            RegisterButtonCallback();
        }

        /// <summary>
        ///     スキル一覧表示を初期化する。
        /// </summary>
        /// <param name="skillElementTemplate"> スキル要素テンプレート。 </param>
        /// <param name="onSkillElementCreated"> スキル要素生成時に呼ばれるコールバック（D&amp;D のセットアップ等に使用）。 </param>
        public void InitializeSkillList(VisualTreeAsset skillElementTemplate, Action<VisualElement> onSkillElementCreated = null)
        {
            _skillListView ??= new SkillListView(_skillElementList, skillElementTemplate, onSkillElementCreated);
        }

        /// <summary>
        ///     ViewModel をバインドする。
        /// </summary>
        /// <param name="viewModel"> バインド対象の ViewModel。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Bind(SkillBuildViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            // 既存の ViewModel の購読を解除する。
            Unbind();
            _viewModel = viewModel;
            _viewModel.OnSkillListChanged += HandleSkillListChangedHandler;
        }

        /// <summary>
        ///     スキル一覧表示を更新する。
        /// </summary>
        /// <param name="skills"> 表示するスキル一覧。 </param>
        /// <exception cref="InvalidOperationException"></exception>
        public void SetSkillList(IReadOnlyList<(int skillId, string skillLabel)> skills)
        {
            if (_skillListView == null)
            {
                throw new InvalidOperationException("先に SkillListView の初期化が必要です。");
            }

            _skillListView.SetSkills(skills);
            SyncEquippedSkillsToSlots();
        }

        /// <summary>
        ///     ViewModel の購読を解除する。
        /// </summary>
        public void Unbind()
        {
            if (_viewModel == null)
            {
                return;
            }

            _viewModel.OnSkillListChanged -= HandleSkillListChangedHandler;
            _viewModel = null;
        }

        /// <summary>
        ///     リソースを解放する。
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            Unbind();
            UnregisterButtonCallback();
        }

        private const string BACKBUTTON_NAME = "BackButton";
        private const string SKILLBUILD_SAVEBUTTON_NAME = "SkillBuildSaveButton";
        private const string SKILLLEVELUP_BUTTON_NAME = "SkillLevelUpButton";
        private const string SKILL_ELEMENT_LIST_CLASS_NAME = "skill-element-list";

        private const string SKILLBUILD_DIALOG_NAME = "SkillBuildDialog";
        private const string DIALOG_BACKGROUND_NAME = "BackGround";
        private const string DISCARD_AND_CLOSE_BUTTON_NAME = "DiscardAndCloseButton";
        private const string SAVE_AND_CLOSE_BUTTON_NAME = "SaveAndCloseButton";

        private readonly Button _backButton;
        private readonly Button _skillBuildSaveButton;
        private readonly Button _skillLevelUpButton;
        private readonly VisualElement _skillElementList;

        private readonly VisualElement _skillBuildDialog;
        private readonly VisualElement _dialogPanel;
        private readonly Button _unsavedSaveAndCloseButton;
        private readonly Button _unsavedDiscardAndCloseButton;
        private readonly VisualElement _unsavedChangesDialogOverlay;

        private SkillListView _skillListView;
        private SkillBuildViewModel _viewModel;

        private bool _isSavingSkillBuild;

        /// <summary>
        ///     ボタンのコールバックを登録します。
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
        ///     ボタンのコールバックを解除します。
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
        ///     現在の装備済みスキルを対応スロットへ反映する。
        /// </summary>
        private void SyncEquippedSkillsToSlots()
        {
            if (_skillListView == null || _viewModel == null)
            {
                return;
            }

            _skillListView.MoveEquippedSkillsToSlots(RootElement, _viewModel.Slots.Span);
        }

        /// <summary>
        ///     戻る時に未保存変更があれば確認ダイアログを出す。
        /// </summary>
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
        ///     スキルビルド保存ボタンがクリックされたときの処理です。
        /// </summary>
        /// <param name="evt"> クリックイベント。 </param>
        private async void HandleSkillBuildSaveButtonClickedHandler(ClickEvent evt)
        {
            await TrySaveCurrentSkillBuildAsync();
        }

        /// <summary>
        ///     スキルレベルアップボタンがクリックされたときの処理です。
        /// </summary>
        /// <param name="evt"> クリックイベント。 </param>
        private void HandleSkillLevelUpButtonClickedHandler(ClickEvent evt)
        {
            OutGameUIEvent.OnSkillLevelUp?.Invoke();
        }

        /// <summary>
        ///     ViewModel からのスキル一覧更新を反映する。
        /// </summary>
        /// <param name="skills"> 表示するスキル一覧。 </param>
        private void HandleSkillListChangedHandler(IReadOnlyList<(int skillId, string skillLabel)> skills)
        {
            SetSkillList(skills);
        }

        /// <summary>
        ///     「保存して戻る」押下時の処理。
        /// </summary>
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
        ///     「破棄して戻る」押下時の処理。
        /// </summary>
        private void HandleUnsavedDiscardAndCloseButtonClickedHandler(ClickEvent evt)
        {
            HideUnsavedChangesDialog();
            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     ダイアログ背景クリック時の処理（確認画面のみ閉じる）。
        /// </summary>
        private void HandleUnsavedDialogBackgroundClickedHandler(ClickEvent evt)
        {
            HideUnsavedChangesDialog();
        }

        /// <summary>
        ///     ダイアログ本体クリック時は背景へイベントを伝播させない。
        /// </summary>
        private void HandleUnsavedDialogPanelClickedHandler(ClickEvent evt)
        {
            evt.StopPropagation();
        }

        /// <summary>
        ///    未保存変更確認ダイアログを表示する。
        /// </summary>
        private void ShowUnsavedChangesDialog()
        {
            _skillBuildDialog.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        ///    未保存変更確認ダイアログを非表示にする。
        /// </summary>
        private void HideUnsavedChangesDialog()
        {
            _skillBuildDialog.style.display = DisplayStyle.None;
        }

        /// <summary>
        ///     BackGround 直下の最初の子要素をダイアログ本体として取得する。
        /// </summary>
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
        ///     現在のスキルビルド保存を試行し、成功時は保存済み状態に更新する。
        /// </summary>
        /// <returns> 保存が成功したかどうか。 </returns>
        private async ValueTask<bool> TrySaveCurrentSkillBuildAsync()
        {
            if (_isSavingSkillBuild)
            {
                return false;
            }

            _isSavingSkillBuild = true;
            try
            {
                bool isSaved = await InvokeSkillBuildSavedAsync();
                if (!isSaved)
                {
                    return false;
                }
            }
            finally
            {
                _isSavingSkillBuild = false;
            }

            _viewModel?.MarkCurrentAsSaved();
            return true;
        }

        /// <summary>
        ///    スキルビルド保存イベントを非同期で呼び出す。
        /// </summary>
        /// <returns> 保存が成功したかどうか。 </returns>
        private async ValueTask<bool> InvokeSkillBuildSavedAsync()
        {
            if (OutGameUIEvent.OnSkillBuildSaved == null)
            {
                return false;
            }

            // 登録されているすべてのイベントハンドラを順番に呼び出す。
            Delegate[] handlers = OutGameUIEvent.OnSkillBuildSaved.GetInvocationList();
            for (int i = 0; i < handlers.Length; i++)
            {
                try
                {
                    Func<Task<bool>> handler = (Func<Task<bool>>)handlers[i];
                    bool isSaved = await handler();
                    if (!isSaved)
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
#if UNITY_EDITOR
                    Debug.LogError($"スキルビルド保存イベント実行中に例外が発生しました: {ex}");
#endif
                    return false;
                }
            }

            return true;
        }
    }
}