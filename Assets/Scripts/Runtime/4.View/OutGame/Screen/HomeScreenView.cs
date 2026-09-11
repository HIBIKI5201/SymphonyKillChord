using KillChord.Runtime.View.OutGame.Navigation;
using System;
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
            _stageSelectButton.MakeNavigable();
            _skillTreeButton.MakeNavigable();
            _skillBuildButton.MakeNavigable();
            _settingButton.MakeNavigable();

            _stageSelectActivation = _stageSelectButton.RegisterActivation(HandleStageSelectActivationHandler);
            _skillTreeActivation = _skillTreeButton.RegisterActivation(HandleSkillTreeActivationHandler);
            _skillBuildActivation = _skillBuildButton.RegisterActivation(HandleSkillBuildActivationHandler);
            _settingActivation = _settingButton.RegisterActivation(HandleSettingActivationHandler);
        }

        /// <summary>
        ///     各ボタンのコールバックを登録解除します。
        /// </summary>
        private void UnregisterButtonCallbacks()
        {
            _stageSelectActivation?.Dispose();
            _skillTreeActivation?.Dispose();
            _skillBuildActivation?.Dispose();
            _settingActivation?.Dispose();
        }

        /// <summary>
        ///     作戦ボタンが作動したときのコールバックです。
        ///     作戦画面を表示するイベントを発行します。
        /// </summary>
        private void HandleStageSelectActivationHandler()
        {
            OutGameUIEvent.OnShownStageSelectionScreen?.Invoke();
        }

        /// <summary>
        ///     研究ボタンが作動したときのコールバックです。
        ///     研究画面を表示するイベントを発行します。
        /// </summary>
        private void HandleSkillTreeActivationHandler()
        {
            OutGameUIEvent.OnShownSkillTreeScreen?.Invoke();
        }

        /// <summary>
        ///     改造ボタンが作動したときのコールバックです。
        ///     改造画面を表示するイベントを発行します。
        /// </summary>
        private void HandleSkillBuildActivationHandler()
        {
            OutGameUIEvent.OnShownSkillBuildScreen?.Invoke();
        }

        /// <summary>
        ///     設定ボタンが作動したときのコールバックです。
        ///     設定画面を表示するイベントを発行します。
        /// </summary>
        private void HandleSettingActivationHandler()
        {
            OutGameUIEvent.OnShownSettingScreen?.Invoke();
        }


        /// <inheritdoc />
        protected override VisualElement InitialFocusElement => _stageSelectButton;

        private const string STAGE_SELECT_BUTTON_NAME = "StageSelect";
        private const string SKILL_TREE_BUTTON_NAME = "SkillTree";
        private const string SKILL_BUILD_BUTTON_NAME = "SkillBuild";
        private const string SETTING_BUTTON_NAME = "Setting";

        private readonly Button _stageSelectButton;
        private readonly Button _skillTreeButton;
        private readonly Button _skillBuildButton;
        private readonly Button _settingButton;
        private IDisposable _stageSelectActivation;
        private IDisposable _skillTreeActivation;
        private IDisposable _skillBuildActivation;
        private IDisposable _settingActivation;
    }
}
