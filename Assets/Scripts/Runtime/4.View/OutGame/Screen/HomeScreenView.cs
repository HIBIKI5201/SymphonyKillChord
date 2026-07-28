using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     ホーム画面 View。
    /// </summary>
    public sealed class HomeScreenView : ScreenViewBase
    {
        /// <summary>
        ///     View を初期化します。
        /// </summary>
        public HomeScreenView(VisualElement rootElement, OutGameUIEvent outGameUIEvent)
            : base(rootElement, outGameUIEvent)
        {
            _stageSelectButton = RootElement.Q<Button>(STAGE_SELECT_BUTTON_NAME)
                ?? throw new System.InvalidOperationException(
                    $"{STAGE_SELECT_BUTTON_NAME} が見つかりません。");

            _skillTreeButton = RootElement.Q<Button>(SKILL_TREE_BUTTON_NAME)
                ?? throw new System.InvalidOperationException(
                    $"{SKILL_TREE_BUTTON_NAME} が見つかりません。");

            _skillBuildButton = RootElement.Q<Button>(SKILL_BUILD_BUTTON_NAME)
                ?? throw new System.InvalidOperationException(
                    $"{SKILL_BUILD_BUTTON_NAME} が見つかりません。");

            _settingButton = RootElement.Q<Button>(SETTING_BUTTON_NAME)
                ?? throw new System.InvalidOperationException(
                    $"{SETTING_BUTTON_NAME} が見つかりません。");

            RegisterButtonCallbacks();
        }

        /// <summary>
        ///     リソースを解放します。
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            UnregisterButtonCallbacks();
        }

        /// <summary>
        ///     各ボタンのコールバックを登録します。
        /// </summary>
        private void RegisterButtonCallbacks()
        {
            _stageSelectButton.RegisterCallback<ClickEvent>(OnStageSelectClicked);
            _skillTreeButton.RegisterCallback<ClickEvent>(OnSkillTreeClicked);
            _skillBuildButton.RegisterCallback<ClickEvent>(OnSkillBuildClicked);
            _settingButton.RegisterCallback<ClickEvent>(OnSettingClicked);
        }

        /// <summary>
        ///     各ボタンのコールバックを登録解除します。
        /// </summary>
        private void UnregisterButtonCallbacks()
        {
            _stageSelectButton?.UnregisterCallback<ClickEvent>(OnStageSelectClicked);
            _skillTreeButton?.UnregisterCallback<ClickEvent>(OnSkillTreeClicked);
            _skillBuildButton?.UnregisterCallback<ClickEvent>(OnSkillBuildClicked);
            _settingButton?.UnregisterCallback<ClickEvent>(OnSettingClicked);
        }

        /// <summary>
        ///     作戦ボタンがクリックリされたときのコールバックです。
        ///     作戦画面を表示するイベントを発行します。
        /// </summary>
        /// <param name="evt"> クリックイベントの情報。 </param>
        private void OnStageSelectClicked(ClickEvent evt)
        {
            OutGameUIEvent.OnShownStageSelectionScreen?.Invoke();
        }

        /// <summary>
        ///     研究ボタンがクリックされたときのコールバックです。
        ///     研究画面を表示するイベントを発行します。
        /// </summary>
        /// <param name="evt"> クリックイベントの情報。 </param>
        private void OnSkillTreeClicked(ClickEvent evt)
        {
            OutGameUIEvent.OnShownSkillTreeScreen?.Invoke();
        }

        /// <summary>
        ///     改造ボタンがクリックされたときのコールバックです。
        ///     改造画面を表示するイベントを発行します。
        /// </summary>
        /// <param name="evt"> クリックイベントの情報。 </param>
        private void OnSkillBuildClicked(ClickEvent evt)
        {
            OutGameUIEvent.OnShownSkillBuildScreen?.Invoke();
        }

        /// <summary>
        ///     設定ボタンがクリックされたときのコールバックです。
        ///     設定画面を表示するイベントを発行します。
        /// </summary>
        /// <param name="evt"> クリックイベントの情報。 </param>
        private void OnSettingClicked(ClickEvent evt)
        {
            OutGameUIEvent.OnShownSettingScreen?.Invoke();
        }


        private const string STAGE_SELECT_BUTTON_NAME = "StageSelect";
        private const string SKILL_TREE_BUTTON_NAME = "SkillTree";
        private const string SKILL_BUILD_BUTTON_NAME = "SkillBuild";
        private const string SETTING_BUTTON_NAME = "Setting";

        private readonly Button _stageSelectButton;
        private readonly Button _skillTreeButton;
        private readonly Button _skillBuildButton;
        private readonly Button _settingButton;
    }
}