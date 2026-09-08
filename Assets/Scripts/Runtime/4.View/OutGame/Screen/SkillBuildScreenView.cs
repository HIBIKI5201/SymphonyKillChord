
using KillChord.Runtime.Adaptor.OutGame.Audio;
using KillChord.Runtime.Adaptor.OutGame.SkillBuild;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.SkillBuild;
using R3;
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

            VisualElement skillGenreFilterBarRoot = rootElement.Q<VisualElement>(SKILL_GENRE_FILTER_BAR_NAME)
                ?? throw new ArgumentNullException($"[{nameof(SkillBuildScreenView)}] {SKILL_GENRE_FILTER_BAR_NAME} が見つかりませんでした。");
            _skillGenreFilterBarView = new SkillGenreFilterBarView(skillGenreFilterBarRoot);
            _skillGenreFilterBarView.OnGenreFilterSelected += HandleGenreFilterBarSelectedHandler;

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
        /// <param name="soundEffectCommand"> UI操作音の再生コマンド。 </param>
        public void InitializeSkillList(
            VisualTreeAsset skillElementTemplate,
            Action<VisualElement> onSkillElementCreated,
            IUISoundEffectCommand soundEffectCommand)
        {
            if (_skillListView != null)
            {
                return;
            }

            _skillListView = new SkillListView(
                _skillElementList,
                skillElementTemplate,
                onSkillElementCreated,
                soundEffectCommand);
            _skillBuildSlotLayout = new SkillBuildSlotLayout(
                RootElement,
                ResolveSkillData,
                HandleSlotTappedHandler);
            _skillListView.OnSkillSelected += HandleSkillSelectedHandler;
            _skillListView.OnGenreBadgeSelected += HandleGenreBadgeSelectedHandler;
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
            _currentSkills = Array.Empty<SkillViewData>();
            _currentSelectedSkillId = null;
            _activeGenreFilter = null;
            _skillGenreFilterBarView.SetActiveGenre(null);
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
                _skillListView.OnGenreBadgeSelected -= HandleGenreBadgeSelectedHandler;
                _skillListView.Dispose();
                _skillListView = null;
            }

            _skillBuildSlotLayout?.Dispose();
            _skillBuildSlotLayout = null;

            _skillGenreFilterBarView.OnGenreFilterSelected -= HandleGenreFilterBarSelectedHandler;
            _skillGenreFilterBarView.Dispose();
        }

        /// <inheritdoc />
        protected override VisualElement InitialFocusElement => _skillBuildSaveButton;

        private const string BACKBUTTON_NAME = "BackButton";
        private const string SKILLBUILD_SAVEBUTTON_NAME = "SkillBuildSaveButton";
        private const string SKILLLEVELUP_BUTTON_NAME = "SkillLevelUpButton";
        private const string SKILL_DETAIL_NAME = "SkillDetail";
        private const string SKILL_GENRE_FILTER_BAR_NAME = "SkillGenreFilterBar";
        private const string SKILL_ELEMENT_LIST_CLASS_NAME = "skill-element-list";
        private const string SKILLBUILD_DIALOG_NAME = "SkillBuildDialog";
        private const string DIALOG_BACKGROUND_NAME = "BackGround";
        private const string DISCARD_AND_CLOSE_BUTTON_NAME = "DiscardAndCloseButton";
        private const string SAVE_AND_CLOSE_BUTTON_NAME = "SaveAndCloseButton";
        private const string OWNED_POINTS_LABEL_NAME = "OwnedPointsLabel";

        /// <inheritdoc />
        protected override VisualElement CancelTargetElement => _backButton;

        private readonly Button _backButton;
        private readonly Button _skillBuildSaveButton;
        private readonly Button _skillLevelUpButton;
        private readonly Label _ownedPointsLabel;
        private readonly VisualElement _skillElementList;
        private readonly SkillDetailView _skillDetailView;
        private readonly SkillGenreFilterBarView _skillGenreFilterBarView;
        private readonly VisualElement _skillBuildDialog;
        private readonly VisualElement _dialogPanel;
        private readonly Button _unsavedSaveAndCloseButton;
        private readonly Button _unsavedDiscardAndCloseButton;
        private readonly VisualElement _unsavedChangesDialogOverlay;

        /// <summary> 未保存確認ダイアログ表示中、フォーカスを内側へ閉じ込める。 </summary>
        private readonly ModalNavigationScope _dialogNavigationScope = new();

        private SkillListView _skillListView;
        private SkillBuildSlotLayout _skillBuildSlotLayout;
        private ISkillBuildViewModel _viewModel;
        private CompositeDisposable _subscriptions;
        private IReadOnlyList<SkillBuildSlotState> _currentSlots =
            Array.Empty<SkillBuildSlotState>();
        private IReadOnlyList<SkillViewData> _currentSkills =
            Array.Empty<SkillViewData>();
        private int? _currentSelectedSkillId;
        private int? _activeGenreFilter;
        private bool _isSavingSkillBuild;
        private IDisposable _backButtonActivation;
        private IDisposable _skillBuildSaveButtonActivation;
        private IDisposable _skillLevelUpButtonActivation;
        private IDisposable _unsavedSaveAndCloseButtonActivation;
        private IDisposable _unsavedDiscardAndCloseButtonActivation;

        /// <summary>
        ///     ボタンのコールバックを登録する。
        /// </summary>
        private void RegisterButtonCallback()
        {
            _unsavedChangesDialogOverlay.RegisterCallback<ClickEvent>(HandleUnsavedDialogBackgroundClickedHandler);
            _dialogPanel.RegisterCallback<ClickEvent>(HandleUnsavedDialogPanelClickedHandler);

            // オーバーレイとダイアログ本体は背景クリックの判定用であり、フォーカス対象にしない。
            // キャンセル操作で戻れるため、フォーカス移動の対象からは外す。
            _backButton.ExcludeFromNavigation();
            _skillBuildSaveButton.MakeNavigable();
            _skillLevelUpButton.MakeNavigable();
            _unsavedSaveAndCloseButton.MakeNavigable();
            _unsavedDiscardAndCloseButton.MakeNavigable();

            _backButtonActivation = _backButton.RegisterActivation(HandleBackButtonActivationHandler);
            _skillBuildSaveButtonActivation =
                _skillBuildSaveButton.RegisterActivation(HandleSkillBuildSaveButtonActivationHandler);
            _skillLevelUpButtonActivation =
                _skillLevelUpButton.RegisterActivation(HandleSkillLevelUpButtonActivationHandler);
            _unsavedSaveAndCloseButtonActivation =
                _unsavedSaveAndCloseButton.RegisterActivation(HandleUnsavedSaveAndCloseButtonActivationHandler);
            _unsavedDiscardAndCloseButtonActivation =
                _unsavedDiscardAndCloseButton.RegisterActivation(HandleUnsavedDiscardAndCloseButtonActivationHandler);
        }

        /// <summary>
        ///     ボタンのコールバックを解除する。
        /// </summary>
        private void UnregisterButtonCallback()
        {
            _backButtonActivation?.Dispose();
            _skillBuildSaveButtonActivation?.Dispose();
            _skillLevelUpButtonActivation?.Dispose();
            _unsavedSaveAndCloseButtonActivation?.Dispose();
            _unsavedDiscardAndCloseButtonActivation?.Dispose();
            _unsavedChangesDialogOverlay.UnregisterCallback<ClickEvent>(HandleUnsavedDialogBackgroundClickedHandler);
            _dialogPanel.UnregisterCallback<ClickEvent>(HandleUnsavedDialogPanelClickedHandler);
        }

        /// <summary>
        ///     所持スキル一覧を表示へ反映する。
        /// </summary>
        /// <param name="skills"> 所持スキル一覧。 </param>
        private void HandleSkillsChangedHandler(IReadOnlyList<SkillViewData> skills)
        {
            _currentSkills = skills ?? Array.Empty<SkillViewData>();
            _skillGenreFilterBarView.SetAvailableGenres(ExtractDistinctGenres(_currentSkills));
            RefreshSkillListOrder();
        }

        /// <summary>
        ///     スキル一覧から、絞り込みボタン表示用のジャンル一覧を重複排除して抽出する。
        /// </summary>
        /// <param name="skills"> スキル一覧。 </param>
        /// <returns> ジャンル ID とアイコンの一覧(ジャンル ID 昇順)。 </returns>
        private static List<(int GenreId, Sprite Icon)> ExtractDistinctGenres(
            IReadOnlyList<SkillViewData> skills)
        {
            SortedDictionary<int, Sprite> genreIcons = new();
            for (int i = 0; i < skills.Count; i++)
            {
                SkillViewData skill = skills[i];
                if (skill.GenreIds == null || skill.GenreIds.Length == 0)
                {
                    continue;
                }

                int genreId = skill.GenreIds[0];
                if (!genreIcons.ContainsKey(genreId))
                {
                    genreIcons[genreId] = skill.GenreIcon;
                }
            }

            List<(int GenreId, Sprite Icon)> result = new(genreIcons.Count);
            foreach (KeyValuePair<int, Sprite> entry in genreIcons)
            {
                result.Add((entry.Key, entry.Value));
            }

            return result;
        }

        /// <summary>
        ///     スロット状態を表示へ反映する。
        ///     装備/解除のたびに一覧の並び(解放済み/未開放のソート・区切り線)も再構築する。
        /// </summary>
        /// <param name="slots"> スロット状態。 </param>
        private void HandleSlotsChangedHandler(IReadOnlyList<SkillBuildSlotState> slots)
        {
            _currentSlots = slots ?? Array.Empty<SkillBuildSlotState>();
            SyncEquippedSkillsToSlots();

            // ドラッグ完了処理の呼び出し元(要素自身のイベントコールバック)から
            // 同期的に要素を破棄すると危険なため、一覧の再構築は次フレームへ遅延させる。
            RootElement.schedule.Execute(RefreshSkillListOrder);
        }

        /// <summary>
        ///     現在のスキル一覧を再構築し、選択状態・ジャンル絞り込み・装備バッジを再適用する。
        /// </summary>
        private void RefreshSkillListOrder()
        {
            if (RootElement.panel == null)
            {
                return;
            }

            _skillListView.SetSkills(_currentSkills);
            _skillListView.SetSelectedSkill(_currentSelectedSkillId);
            _skillListView.ApplyGenreFilter(_activeGenreFilter);
            SyncEquippedSkillsToSlots();
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
        ///     現在の装備状態をスロットおよび一覧のバッジへ反映する。
        /// </summary>
        private void SyncEquippedSkillsToSlots()
        {
            _skillBuildSlotLayout?.Apply(_currentSlots);
            _skillListView.SetEquippedSkillIds(ComputeEquippedSkillIds(_currentSlots));
        }

        /// <summary>
        ///     スロット状態から装備中スキル ID の集合を構築する。
        /// </summary>
        /// <param name="slots"> スロット状態。 </param>
        /// <returns> 装備中スキル ID の集合。 </returns>
        private static HashSet<int> ComputeEquippedSkillIds(IReadOnlyList<SkillBuildSlotState> slots)
        {
            const int EMPTY_SKILL_ID = -1;
            HashSet<int> result = new();
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].CurrentSkillId != EMPTY_SKILL_ID)
                {
                    result.Add(slots[i].CurrentSkillId);
                }
            }

            return result;
        }

        /// <summary>
        ///     スキル ID から表示データを検索する。
        /// </summary>
        /// <param name="skillId"> スキル ID。 </param>
        /// <returns> 表示データ。見つからない場合は null。 </returns>
        private SkillViewData? ResolveSkillData(int skillId)
        {
            for (int i = 0; i < _currentSkills.Count; i++)
            {
                if (_currentSkills[i].SkillId == skillId)
                {
                    return _currentSkills[i];
                }
            }

            return null;
        }

        /// <summary>
        ///     スロットタップによる装備解除を処理する。
        /// </summary>
        /// <param name="skillId"> 解除するスキル ID。 </param>
        private void HandleSlotTappedHandler(int skillId)
        {
            _viewModel?.ApplyDrop(skillId, null);
        }

        /// <summary>
        ///     ジャンルバッジの選択によりジャンル絞り込みを切り替える。
        /// </summary>
        /// <param name="genreId"> 選択されたジャンル ID。 </param>
        private void HandleGenreBadgeSelectedHandler(int genreId)
        {
            SetActiveGenreFilter(_activeGenreFilter == genreId ? null : genreId);
        }

        /// <summary>
        ///     ジャンルフィルタバーの選択を処理する。
        /// </summary>
        /// <param name="genreId"> 選択されたジャンル ID。全てボタンの場合は null。 </param>
        private void HandleGenreFilterBarSelectedHandler(int? genreId)
        {
            if (!genreId.HasValue)
            {
                SetActiveGenreFilter(null);
                return;
            }

            SetActiveGenreFilter(_activeGenreFilter == genreId.Value ? null : genreId.Value);
        }

        /// <summary>
        ///     ジャンル絞り込み状態を更新し、一覧表示とフィルタバーの両方へ反映する。
        /// </summary>
        /// <param name="genreId"> 絞り込むジャンル ID。全件表示の場合は null。 </param>
        private void SetActiveGenreFilter(int? genreId)
        {
            _activeGenreFilter = genreId;
            _skillListView.ApplyGenreFilter(_activeGenreFilter);
            _skillGenreFilterBarView.SetActiveGenre(_activeGenreFilter);
        }

        /// <summary>
        ///     戻るボタンを処理する。
        /// </summary>
        private void HandleBackButtonActivationHandler()
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
        private async void HandleSkillBuildSaveButtonActivationHandler()
        {
            await TrySaveCurrentSkillBuildAsync();
        }

        /// <summary>
        ///     レベルアップボタンを処理する。
        /// </summary>
        private void HandleSkillLevelUpButtonActivationHandler()
        {
            OutGameUIEvent.OnSkillLevelUp?.Invoke();
        }

        /// <summary>
        ///     保存して閉じる操作を処理する。
        /// </summary>
        private async void HandleUnsavedSaveAndCloseButtonActivationHandler()
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
        private void HandleUnsavedDiscardAndCloseButtonActivationHandler()
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

            // 背面のスキル一覧やスロットへフォーカスが抜けないようにする。
            _dialogNavigationScope.Activate(_skillBuildDialog);
        }

        /// <summary>
        ///     未保存変更ダイアログを隠す。
        /// </summary>
        private void HideUnsavedChangesDialog()
        {
            _dialogNavigationScope.Deactivate();
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
