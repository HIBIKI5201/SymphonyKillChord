using KillChord.Runtime.View.OutGame.SkillBuild;
using System;
using System.Collections.Generic;
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

        private readonly Button _backButton;
        private readonly Button _skillBuildSaveButton;
        private readonly Button _skillLevelUpButton;
        private readonly VisualElement _skillElementList;

        private SkillListView _skillListView;
        private SkillBuildViewModel _viewModel;

        /// <summary>
        ///     ボタンのコールバックを登録します。
        /// </summary>
        private void RegisterButtonCallback()
        {
            _backButton.RegisterCallback<ClickEvent>(HandleBackButtonClickedHandler);
            _skillBuildSaveButton.RegisterCallback<ClickEvent>(HandleSkillBuildSaveButtonClickedHandler);
            _skillLevelUpButton.RegisterCallback<ClickEvent>(HandleSkillLevelUpButtonClickedHandler);
        }

        /// <summary>
        ///     ボタンのコールバックを解除します。
        /// </summary>
        private void UnregisterButtonCallback()
        {
            _backButton.UnregisterCallback<ClickEvent>(HandleBackButtonClickedHandler);
            _skillBuildSaveButton.UnregisterCallback<ClickEvent>(HandleSkillBuildSaveButtonClickedHandler);
            _skillLevelUpButton.UnregisterCallback<ClickEvent>(HandleSkillLevelUpButtonClickedHandler);
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
        ///     画面を閉じるボタンがクリックされたときの処理です。
        /// </summary>
        /// <param name="evt"> クリックイベント。 </param>
        private void HandleBackButtonClickedHandler(ClickEvent evt)
        {
            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     スキルビルド保存ボタンがクリックされたときの処理です。
        /// </summary>
        /// <param name="evt"> クリックイベント。 </param>
        private void HandleSkillBuildSaveButtonClickedHandler(ClickEvent evt)
        {
            OutGameUIEvent.OnSkillBuildSaved?.Invoke();
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
    }
}